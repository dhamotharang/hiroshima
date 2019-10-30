﻿using Hiroshima.Common.Models;
using NetTopologySuite.Geometries;

namespace Hiroshima.DbData.Models.Location
{
    public class Location
    {
        public int Id { get; set; }
        public Point Coordinates { get; set; }
        public MultiLanguage<string> Address { get; set; }
        public Locality Locality { get; set; }
        public int LocalityId { get; set; }
        public Accommodation.Accommodation Accommodation { get; set; }
        public int AccommodationId { get; set; }
    }
}
