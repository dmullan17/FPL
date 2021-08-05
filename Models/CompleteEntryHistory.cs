using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Models
{
    public class CompleteEntryHistory
    {
        [JsonProperty("current")]
        public List<EntryHistory> CurrentSeasonEntryHistory { get; set; }

        [JsonProperty("past")]
        public List<Season> PastSeasons { get; set; }

        [JsonProperty("chips")]
        public List<BasicChip> ChipsUsed { get; set; }

        public int TotalTransfersMade { get; set; }

        public int TotalTransfersCost { get; set; }

        //public List<int> Last5GwPoints { get; set } = 

        public List<int> GetLast5GwPoints()
        {
            List<int> last5 = new List<int>();

            var last6 = CurrentSeasonEntryHistory.Skip(Math.Max(0, CurrentSeasonEntryHistory.Count() - 6)).ToList();

            if (last6.Count != 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5)
                    {
                        continue;
                    }
                    last5.Add(last6[i].points);
                }

                last5.Reverse();
            }

            return last5;
        }
    }
}
