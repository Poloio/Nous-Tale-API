using System.Collections.Generic;

namespace Nous_Tale_API.Model.Entities
{
    public class Tale
    {
        public int TaleID { get; set; }
        public int RoomID { get; set; }
        public int PlayerID { get; set; }
        public string Title { get; set; }

        public Room Room { get; set; }
        public Player Player { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
    }
}