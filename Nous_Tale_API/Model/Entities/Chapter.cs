﻿namespace Nous_Tale_API.Model.Entities
{
    public class Chapter
    {
        public int ID { get; set; }
        public int TaleID { get; set; }
        public int? PlayerID { get; set; }
        public int OrderNo { get; set; }
        public string Text { get; set; }
        public EChapterMood Mood { get; set; }

        public virtual Tale Tale { get; set; }
        public virtual Player Player { get; set; }
    }
}
