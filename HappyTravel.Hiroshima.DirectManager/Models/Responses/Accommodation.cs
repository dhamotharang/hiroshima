using System.Collections.Generic;
using HappyTravel.Geography;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Common.Models.Accommodations;
using HappyTravel.Hiroshima.Common.Models.Accommodations.Rooms.OccupancyDefinitions;
using HappyTravel.Hiroshima.Common.Models.Enums;

namespace HappyTravel.Hiroshima.DirectManager.Models.Responses
{
    public readonly struct Accommodation
    {
        public Accommodation(int id, MultiLanguage<string> name, MultiLanguage<string> address, MultiLanguage<TextualDescription> description, 
            GeoPoint coordinates, AccommodationStars rating, string checkInTime, string checkOutTime, 
            ContactInfo contactInfo, PropertyTypes type, MultiLanguage<List<string>> amenities, MultiLanguage<string> additionalInfo, 
            OccupancyDefinition occupancyDefinition, int locationId, MultiLanguage<List<string>> leisureAndSports, Status status, 
            RateOptions rateOptions, int? floors, int? buildYear, string postalCode, List<Room> rooms)
        {
            Id = id;
            Name = name;
            Address = address;
            Description = description;
            Coordinates = coordinates;
            Rating = rating;
            CheckInTime = checkInTime;
            CheckOutTime = checkOutTime;
            ContactInfo = contactInfo;
            Type = type;
            Amenities = amenities;
            AdditionalInfo = additionalInfo;
            OccupancyDefinition = occupancyDefinition;
            LocationId = locationId;
            RateOptions = rateOptions;
            Status = status;
            LeisureAndSports = leisureAndSports;
            Rooms = rooms;
            Floors = floors;
            BuildYear = buildYear;
            PostalCode = postalCode;
        }


        public int Id { get; }
        
        public MultiLanguage<string> Name { get; }
        
        public MultiLanguage<string> Address { get; }
        
        public MultiLanguage<TextualDescription> Description { get; }
        
        public GeoPoint Coordinates { get; }
        
        public AccommodationStars Rating { get; }
        
        public string CheckInTime { get; }
        
        public string CheckOutTime { get; }
        
        public ContactInfo ContactInfo { get; }
        
        public PropertyTypes Type { get; }
        
        public MultiLanguage<List<string>> Amenities { get; }
        
        public MultiLanguage<string> AdditionalInfo { get; }
        
        public OccupancyDefinition OccupancyDefinition { get; }
        
        public int LocationId { get; }
        
        public MultiLanguage<List<string>> LeisureAndSports { get; }
        
        public RateOptions RateOptions { get; }
        
        public int? Floors { get; }
        
        public int? BuildYear { get; }

        public string PostalCode { get; }
        
        public Status Status { get; }
        

        public List<Room> Rooms { get; }
    }
}