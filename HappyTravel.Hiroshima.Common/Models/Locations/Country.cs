﻿using System.Text.Json;

namespace HappyTravel.Hiroshima.Common.Models.Locations
{
    public class Country
    {
        public string Code { get; set; }
        
        public JsonDocument Name { get; set; }
    }
}