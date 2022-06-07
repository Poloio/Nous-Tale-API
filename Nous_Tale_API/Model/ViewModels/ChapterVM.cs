using Nous_Tale_API.Model.Entities;

namespace Nous_Tale_API.Model.ViewModels
{
    public class ChapterVM
    {
        public int ID { get; set; }
        public int TaleID { get; set; }
        public int? PlayerID { get; set; }
        public int OrderNo { get; set; }
        public string Text { get; set; }
        public string Mood { get; set; }
    }
}