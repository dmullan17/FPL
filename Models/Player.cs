using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FPL.Models
{
    public class Player
    {
        //public Player()
        //{
        //    GamesPlayed = 1;
        //}
        public PlayerHistory PlayerHistory { get; set; }
        public Team Team { get; set; }
        public List<PlayerFixture> Fixtures { get; set; }
        public int FplRank { get; set; }
        public int FplPositionRank { get; set; }
        public float FplIndex { get; set; }
        public int BpsRank { get; set; }
        public int BpsPositionRank { get; set; }
        public int AvgBpsRank { get; set; }
        public decimal? MinsPlayedPercentage { get; set; }
        public int GamesPlayed { get; set; }
        public CostInterval CostInterval { get; set; }
        public decimal PointsPerMillion { get; set; }
        //public int CostIntervalPositionRank { get; set; }
        //public int CostInterval { get; set; }
        //public int PointsRankingForCostInterval { get; set; }
        //public int PointsPositionRankingForCostInterval { get; set; }
        public int? chance_of_playing_next_round { get; set; }
        public int? chance_of_playing_this_round { get; set; }
        public int code { get; set; }
        public int cost_change_event { get; set; }
        public int cost_change_event_fall { get; set; }
        public int cost_change_start { get; set; }
        public int cost_change_start_fall { get; set; }
        public int dreamteam_count { get; set; }
        public int element_type { get; set; }
        public string ep_next { get; set; }
        public string ep_this { get; set; }
        public int event_points { get; set; }
        public string first_name { get; set; }
        public string form { get; set; }
        public int id { get; set; }
        public bool in_dreamteam { get; set; }
        public string news { get; set; }
        public DateTime? news_added { get; set; }
        public int now_cost { get; set; }
        public string photo { get; set; }
        public string points_per_game { get; set; }
        public string second_name { get; set; }
        public string selected_by_percent { get; set; }
        public bool special { get; set; }
        public object squad_number { get; set; }
        public string status { get; set; }

        [JsonProperty("team")]
        public int TeamId { get; set; }
        public int team_code { get; set; }
        public int total_points { get; set; }
        public int transfers_in { get; set; }
        public int transfers_in_event { get; set; }
        public int transfers_out { get; set; }
        public int transfers_out_event { get; set; }
        public string value_form { get; set; }
        public string value_season { get; set; }
        public string web_name { get; set; }
        public int minutes { get; set; }
        public int goals_scored { get; set; }
        public int assists { get; set; }
        public int clean_sheets { get; set; }
        public int goals_conceded { get; set; }
        public int own_goals { get; set; }
        public int penalties_saved { get; set; }
        public int penalties_missed { get; set; }
        public int yellow_cards { get; set; }
        public int red_cards { get; set; }
        public int saves { get; set; }
        public int bonus { get; set; }
        public int bps { get; set; }
        public string influence { get; set; }
        public string creativity { get; set; }
        public string threat { get; set; }
        public string ict_index { get; set; }
        public int? influence_rank { get; set; }
        public int? influence_rank_type { get; set; }
        public int? creativity_rank { get; set; }
        public int? creativity_rank_type { get; set; }
        public int? threat_rank { get; set; }
        public int? threat_rank_type { get; set; }
        public int? ict_index_rank { get; set; }
        public int? ict_index_rank_type { get; set; }
        public int? corners_and_indirect_freekicks_order { get; set; }
        public string corners_and_indirect_freekicks_text { get; set; }
        public int? direct_freekicks_order { get; set; }
        public string direct_freekicks_text { get; set; }
        public int? penalties_order { get; set; }
        public string penalties_text { get; set; }
    }


}