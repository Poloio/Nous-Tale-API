namespace Nous_Tale_API.Model.ViewModels
{
    public class PlayerVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; }
        // public Color ...

        public string Emoji { get; set; }
        public int RoomID { get; set; }
    }
}
