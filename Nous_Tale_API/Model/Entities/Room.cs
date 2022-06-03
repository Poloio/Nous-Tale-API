using System.Collections.Generic;

namespace Nous_Tale_API.Model.Entities
{
    public class Room
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public bool IsPrivate { get; set; }
        public string Password { get; set; }
        public int MaxPlayers { get; set; }
        public int ReadyCount { get; set; }

        public ICollection<Tale> Tales { get; set; }
        public ICollection<Player> Players { get; set; }

    }

}
