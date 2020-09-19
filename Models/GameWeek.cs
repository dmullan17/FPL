using Newtonsoft.Json;
using System;

namespace FPL.Models
{
    public class GameWeek
    {
        public int id { get; set; }

        public string Name { get; set; }
        public DateTime deadline_time { get; set; }
        public int average_entry_score { get; set; }
        public bool finished { get; set; }
        public bool data_checked { get; set; }
        public int? highest_scoring_entry { get; set; }
        public int deadline_time_epoch { get; set; }
        public int deadline_time_game_offset { get; set; }
        public int? highest_score { get; set; }
        public bool is_previous { get; set; }
        public bool is_current { get; set; }
        public bool is_next { get; set; }
        //public List<ChipPlay> chip_plays { get; set; }
        public int? most_selected { get; set; }
        public int? most_transferred_in { get; set; }
        public int? top_element { get; set; }
        //public TopElementInfo top_element_info { get; set; }
        public int transfers_made { get; set; }
        public int? most_captained { get; set; }
        public int? most_vice_captained { get; set; }



    }
}