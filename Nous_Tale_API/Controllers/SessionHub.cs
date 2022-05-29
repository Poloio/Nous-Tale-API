using Microsoft.AspNetCore.SignalR;
using Nous_Tale_API.Model;
using Nous_Tale_API.Model.Entities;
using System;
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

        public async Task CreateRoom(string hostName, int maxPlayers, string password )
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

                roomID = newRoom.ID; // To call client method
            }

            await EnterRoom(hostName, roomID);
        }

        public async Task EnterRoom(string playerName, int roomID)
        {
            using (var dbConhtext = new NousContext())
            {
                var newPlayer = new Player();
                newPlayer.Name = playerName;
                newPlayer.RoomID = roomID;

                var targetRoom = await dbConhtext.Rooms.FindAsync(roomID);
                
                if (targetRoom.Players == null)
                {
                    newPlayer.IsHost = true;
                } 
                else 
                {
                    newPlayer.IsHost = targetRoom.Players.Count == 0;
                }

                await dbConhtext.Players.AddAsync(newPlayer);
                await dbConhtext.SaveChangesAsync();

                // Add to SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"room{roomID}");

                await Clients.Caller.EnterRoom(newPlayer.Room);

                // Notify group that player entered
                await Clients.Group($"room{roomID}")
                    .PlayerEntered(newPlayer);
            }
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
