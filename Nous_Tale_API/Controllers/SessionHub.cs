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
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<int> CreateRoom(int maxPlayers, string password)
        {
            int roomID;
            using (var context = new NousContext())
            {
                // Create Room
                var rooms = context.Rooms;
                var newRoom = new Room();

                if (password != null)
                {
                    newRoom.Password = password;
                    newRoom.IsPrivate = true;
                } else
                {
                    newRoom.IsPrivate = false;
                }
                newRoom.MaxPlayers = maxPlayers;
                newRoom.Code = GenerateUniqueRoomCode();
                await rooms.AddAsync(newRoom);
                await context.SaveChangesAsync();
                roomID = newRoom.ID;
                
            }
            return roomID;

        }

        public async Task<List<PlayerVM>> EnterRoom(string playerName, int roomID)
        {
            using (var dbContext = new NousContext())
            {
                // Create and add caller player
                var newPlayer = new Player();
                newPlayer.Name = playerName;
                newPlayer.RoomID = roomID;

                var targetRoom = await dbContext.Rooms.FindAsync(roomID);
                
                if (targetRoom.Players == null)
                {
                    newPlayer.IsHost = true;
                } 
                else 
                {
                    newPlayer.IsHost = targetRoom.Players.Count == 0;
                }

                await dbContext.Players.AddAsync(newPlayer);
                await dbContext.SaveChangesAsync();

                // Notify group that player entered
                await Clients.Group($"room{roomID}")
                    .PlayerEntered(newPlayer);

                // VM to avoid circular references
                var vmList = new List<PlayerVM>();
                foreach (var player in targetRoom.Players)
                {
                    var newVmPlayer = new PlayerVM();
                    newVmPlayer.Name = player.Name;
                    newVmPlayer.RoomID = player.RoomID;
                    newVmPlayer.ID = player.ID;
                    newVmPlayer.IsHost = player.IsHost;
                    vmList.Add(newVmPlayer);
                }
                return vmList;
            }
         }

        public async Task ConnectToGroup(int roomID)
        {
            // Add to SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room{roomID}");
        }

        public async Task ExitRoom(int playerID)
        {
            using (var context = new NousContext())
            {
                // Delete player
                var deletedPlayer = await context.Players.FindAsync(playerID);
                context.Players.Remove(deletedPlayer);

                if (deletedPlayer.IsHost)
                {
                    // Set other room host
                    context.Players.First(p => p.RoomID == deletedPlayer.RoomID)
                        .IsHost = true;
                }

                await context.SaveChangesAsync();

                await Clients.Caller.ReturnToMenu();

                // Notify player exited
                await Clients.Group($"room{deletedPlayer.RoomID}")
                    .PlayerExited(playerID);
            }
        }
    }
}
