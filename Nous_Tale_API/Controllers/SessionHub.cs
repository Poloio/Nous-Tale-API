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

        public async Task CreateRoom(int maxPlayers, string password )
        {
            int roomID;
            using (var context = new NousContext())
            {
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
                var inserted = await rooms.AddAsync(newRoom);
                roomID = inserted.Entity.RoomID;
            }
            await Clients.Caller.EnterRoom(roomID);
        }
    }
}
