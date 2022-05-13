using System.Collections.Generic;

namespace Nous_Tale_API.Model.Entities
{
    public class Player
    {
        public int PlayerID { get; set; }
        public int RoomID { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; } 
        // public Color ...

        public Room Room { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
    }
}