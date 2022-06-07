using System.Collections.Generic;

namespace Nous_Tale_API.Model.Entities
{
    public class Player
    {
        public int ID { get; init; }
        public string Name { get; init; }
        public bool IsHost { get; set; }
        public string Emoji { get; init; }
        public string GroupID { get; init; }
        public string ConnectionID { get; set; } 

        public virtual int RoomID { get; set; }
        public virtual Room Room { get; set; }
        public virtual List<Chapter> Chapters { get; set; }
    }
}