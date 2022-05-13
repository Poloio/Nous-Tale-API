namespace Nous_Tale_API.Model.Entities
{
    public class Chapter
    {
        public int ChapterID { get; set; }
        public int TaleID { get; set; }
        public int PlayerID { get; set; }
        public int OrderNo { get; set; }
        public string Text { get; set; }
        // Mood?

        public Tale Tale { get; set; }
        public Player Player { get; set; }
    }
}
