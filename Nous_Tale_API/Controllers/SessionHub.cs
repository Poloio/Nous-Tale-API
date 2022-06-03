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
                    Code = GenerateUniqueRoomCode()
                };

                await rooms.AddAsync(newRoom);
                await context.SaveChangesAsync();
                roomCode = newRoom.Code;
            }
            return roomCode;
        }

        public async Task<List<PlayerVM>> EnterRoom(string playerName, string playerEmoji, string roomId)
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
                    IsHost = targetRoom.Players == null || targetRoom.Players.Count() == 0
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
                int hostPlayerID;

                if (deletedPlayer.IsHost)
                {
                    // Set other room host
                    var newHost = context.Players.First(p => p.RoomID == deletedPlayer.RoomID);
                    newHost.IsHost = true;
                    hostPlayerID = newHost.ID;
                } else
                {
                    // -1 indicates that the host is still the same
                    hostPlayerID = -1;
                }

                await context.SaveChangesAsync();

                await Clients.Caller.ReturnToMenu();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId
                    , $"room{deletedPlayer.RoomID}");
                // Notify player exited
                await Clients.Group($"room{deletedPlayer.RoomID}")
                    .PlayerExited(playerID, hostPlayerID);
            }
            
        }
    }
}
