using System.Collections.Generic;

namespace Nous_Tale_API.Model.ViewModels
{
    public class TaleVM
    {
        public int ID { get; set; }
        public int RoomID { get; set; }
        public string Title { get; set; }
        public List<ChapterVM> Chapters { get; set; }
    }
}
