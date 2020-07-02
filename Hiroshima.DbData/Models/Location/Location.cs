﻿using System.Text.Json;
using Hiroshima.Common.Models;

namespace Hiroshima.DbData.Models.Location
{
    public class Location
    {
        public int Id { get; set; }
        public JsonDocument Name { get; set; }
        public LocationTypes Type { get; set; }
        public string CountryCode { get; set; }
        public int ParentId { get; set; }
    }
}
