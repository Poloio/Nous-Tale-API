using System.Collections.Generic;

namespace Nous_Tale_API.Model.Entities
{
    public class Player
    {
        public int ID { get; init; }
        public string Name { get; init; }
        public bool IsHost { get; set; }
        public string Emoji { get; init; }
        // public Color ...

        public int RoomID { get; set; }
        public Room Room { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
    }
}