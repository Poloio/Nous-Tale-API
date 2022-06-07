using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Nous_Tale_API.Model;
using Nous_Tale_API.Model.Entities;
using Nous_Tale_API.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nous_Tale_API.Controllers
{
    public class SessionHub : Hub<IPlayerClient>
    {
        #region Lobbying methods
        private string GenerateUniqueRoomCode()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string code = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            using (var context = new NousContext())
            {
                bool codeAlreadyExists = context.Rooms.Where(r => r.Code == code).Any();
                if (codeAlreadyExists)
                    code = GenerateUniqueRoomCode();
            }

            return code;
        }

        public async Task<string> CreateRoom(int maxPlayers, string password)
        {
            string roomCode;
            using (var context = new NousContext())
            {
                // Create Room
                var rooms = context.Rooms;
                var newRoom = new Room()
                {
                    Password = password,
                    IsPrivate = password != null,
                    MaxPlayers = maxPlayers,
                    Code = GenerateUniqueRoomCode(),
                    ReadyCount = 0
                };

                await rooms.AddAsync(newRoom);
                await context.SaveChangesAsync();
                roomCode = newRoom.Code;
            }
            return roomCode;
        }

        public Room GetRoomFromCode(string roomCode)
        {
            Room targetRoom;
            using (var dbContext = new NousContext())
            {
                targetRoom = dbContext.Rooms.Single(r => r.Code == roomCode);
            }
            return targetRoom;
        }

        public bool RoomExists(string roomCode)
        {
            bool exists = false;
            using (var dbContext = new NousContext())
            {
                exists = dbContext.Rooms.Any(r => r.Code == roomCode);
            }
            return exists;
        }

        public async Task<List<PlayerVM>> EnterRoom(string playerName, string playerEmoji, int roomId)
        {
            using (var dbContext = new NousContext())
            {
                // Create and add caller player

                var targetRoom = await dbContext.Rooms.FindAsync(roomId);

                var newPlayer = new Player()
                {
                    Name = playerName,
                    Emoji = playerEmoji,
                    RoomID = targetRoom.ID,
                    IsHost = targetRoom.Players == null || targetRoom.Players.Count() == 0,
                    ConnectionID = Context.ConnectionId
                };

                await dbContext.Players.AddAsync(newPlayer);
                await dbContext.SaveChangesAsync();

                // Notify group that player entered
                await Clients.Group($"room{roomId}")
                    .PlayerEntered(newPlayer);

                await Groups.AddToGroupAsync(Context.ConnectionId, $"room{roomId}");

                // VM to avoid circular references
                var vmList = new List<PlayerVM>();
                foreach (var player in targetRoom.Players)
                {
                    var newVmPlayer = new PlayerVM()
                    {
                        Name = player.Name,
                        RoomID = player.ID,
                        Emoji = player.Emoji,
                        ID = player.ID,
                        IsHost = player.IsHost
                    };
                    vmList.Add(newVmPlayer);
                }
                return vmList;
            }
        }

        public async Task ExitRoom(int playerID)
        {
            using (var context = new NousContext())
            {
                // Delete player
                var deletedPlayer = await context.Players.FindAsync(playerID);
                context.Players.Remove(deletedPlayer);
                int hostPlayerID = -1;// -1 indicates that the host is still the same

                if (deletedPlayer.IsHost)
                {
                    if (deletedPlayer.Room.Players.Count > 1)
                    {
                        // Set other room host
                        var newHost = context.Players.First(p => p.RoomID == deletedPlayer.RoomID);
                        newHost.IsHost = true;
                        hostPlayerID = newHost.ID;
                    } else
                    {
                        // Remove room
                        context.Remove(deletedPlayer.Room);
                        context.RemoveRange(deletedPlayer.Room.Players);
                    }
                }

                await context.SaveChangesAsync();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId
                    , $"room{deletedPlayer.RoomID}");
                // Notify player exited
                await Clients.Group($"room{deletedPlayer.RoomID}")
                    .PlayerExited(playerID, hostPlayerID);
            }

        }

        public async Task ToggleReady(bool isReady, int roomID)
        {
            using (var dbContext = new NousContext())
            {
                var room = await dbContext.Rooms.FindAsync(roomID);
                if (isReady)
                    room.ReadyCount++;
                else
                    room.ReadyCount--;

                await dbContext.SaveChangesAsync();

                await Clients.Group($"room{roomID}")
                    .ReadyCountChanged(room.ReadyCount);
            }
        }

        public RoomWithPlayers GetRoomAndPlayers(string roomCode)
        {
            var vm = new RoomWithPlayers()
            {
                Players = new List<PlayerVM>()
            };

            using (var dbContext = new NousContext())
            {
                vm.Room = dbContext.Rooms.Single(r => r.Code == roomCode);
                // VM to avoid circular references
                foreach (var player in vm.Room.Players)
                {
                    var newVmPlayer = new PlayerVM()
                    {
                        Name = player.Name,
                        RoomID = player.ID,
                        Emoji = player.Emoji,
                        ID = player.ID,
                        IsHost = player.IsHost
                    };
                    vm.Players.Add(newVmPlayer);
                }
            }
            return vm;
        }
        #endregion

        #region Game methods
        public async Task CreateTales(int roomId)
        {
            var returnList = new List<TaleVM>();
            using (var dbContext = new NousContext())
            {
                var targetRoom = await dbContext.Rooms.FindAsync(roomId);
                // Create the tales
                foreach (var player in targetRoom.Players)
                {
                    await dbContext.Tales.AddAsync(new Tale()
                    {
                        RoomID = targetRoom.ID,
                        Chapters = new List<Chapter>(targetRoom.Players.Count),
                        Title = "" // Avoid undefined in TS
                    });
                }
                await dbContext.SaveChangesAsync();

                // Set a mood for each chapter
                Array enumValues = Enum.GetValues(typeof(EChapterMood));
                Random random = new Random();
                foreach (var tale in targetRoom.Tales)
                {
                    foreach (var chapter in tale.Chapters)
                    {
                        chapter.Mood = (EChapterMood)enumValues.GetValue(random.Next(enumValues.Length));
                    }
                }
                await dbContext.SaveChangesAsync();

                // Set viewmodel to call client
                foreach (var tale in targetRoom.Tales)
                {
                    var taleVm = new TaleVM()
                    {
                        ID = tale.ID,
                        RoomID = tale.RoomID,
                        Title = tale.Title,
                        Chapters = new List<ChapterVM>()
                    };

                    foreach (var chapter in tale.Chapters)
                    {
                        taleVm.Chapters.Add(new ChapterVM()
                        {
                            ID = chapter.ID,
                            TaleID = chapter.TaleID,
                            PlayerID = chapter.PlayerID,
                            Text = chapter.Text,
                            Mood = chapter.Mood.ToString()
                        });
                    }

                    returnList.Add(taleVm);
                }
            }
            await Clients.Group($"room{roomId}").TalesCreated(returnList);
        }

        public async Task RoundReadyChanged(int roomId, bool isReady)
        {
            using (var dbContext = new NousContext())
            {
                var targetRoom = await dbContext.Rooms.FindAsync(roomId);
                if (isReady) targetRoom.RoundReadyCount++;
                    else targetRoom.RoundReadyCount--;
                
                await dbContext.SaveChangesAsync();
                // Clients don't need to know when one player is ready
                if (targetRoom.RoundReadyCount == targetRoom.Players.Count) 
                    await Clients.Group($"room{roomId}").EveryoneIsReady();
            }
            
        }

        public async Task UpdateTale(TaleVM updatedTale, int taleIndex)
        {
            using (var dbContext = new NousContext())
            {
                var actualTale = await dbContext.Tales.FindAsync(updatedTale.ID);
                dbContext.Update(actualTale);
                await dbContext.SaveChangesAsync();
            }
            await Clients.OthersInGroup($"room{updatedTale.RoomID}").TaleWasUpdated(updatedTale, taleIndex);
        }
        #endregion

        #region Connection management
        
        public async override Task<Task> OnDisconnectedAsync(Exception exception)
        {
            using (var dbContext = new NousContext())
            {
                var disconnectedPlayer = dbContext.Players.Single(p => p.ConnectionID == Context.ConnectionId);
                await ExitRoom(disconnectedPlayer.ID);
                await dbContext.SaveChangesAsync();
            }
            return base.OnDisconnectedAsync(exception);
        }
        #endregion
    }
}
