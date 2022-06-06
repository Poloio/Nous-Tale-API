using Nous_Tale_API.Model.Entities;
using System.Collections.Generic;

namespace Nous_Tale_API.Model.ViewModels
{
    public class RoomWithPlayers
    {
        public Room Room { get; set; }
        public List<PlayerVM> Players { get; set; }
    }
}
