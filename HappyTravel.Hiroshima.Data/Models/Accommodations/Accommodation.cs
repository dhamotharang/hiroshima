﻿using System.Text.Json;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Common.Models.Enums;
using NetTopologySuite.Geometries;

namespace HappyTravel.Hiroshima.Data.Models.Accommodations
{
    public class Accommodation
    {
        public int Id { get; set; }
        public JsonDocument Name { get; set; }
        public JsonDocument Address { get; set; }
        public JsonDocument TextualDescription { get; set; }
        public Point Coordinates { get; set; }
        public AccommodationRating Rating { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public JsonDocument Pictures { get; set; }
        public Contacts Contacts { get; set; }
        public PropertyTypes PropertyType { get; set; }
        public JsonDocument AccommodationAmenities { get; set; }
        public JsonDocument AdditionalInfo { get; set; }
        public OccupancyDefinition OccupancyDefinition { get; set; }
        public int LocationId { get; set; }
    }
}

