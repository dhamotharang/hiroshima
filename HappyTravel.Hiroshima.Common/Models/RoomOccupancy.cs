﻿using System;
using HappyTravel.Hiroshima.Common.Models.Accommodations.Rooms;
using HappyTravel.Hiroshima.Common.Models.Bookings;

namespace HappyTravel.Hiroshima.Common.Models
{
    public class RoomOccupancy
    {
        public int Id { get; set; }
        
        public DateTime FromDate { get; set; }
        
        public DateTime ToDate { get; set; }
        
        public int RoomId {get; set; }
        
        public Room Room { get; set; }
        
        public Guid BookingOrderId { get; set; }
        
        public BookingOrder BookingOrder { get; set; } 
        
        public DateTime Created { get; set; }
    }
}