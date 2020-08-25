﻿using System.Collections.Generic;

namespace HappyTravel.Hiroshima.Data.Models.Seasons
{
    public class Season
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ContractId { get; set; }
        public List<SeasonRange> SeasonRanges { get; set; }
    }
}