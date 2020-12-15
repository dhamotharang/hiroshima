﻿using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Hiroshima.DirectContracts.Services
{
    public interface IBookingService
    {
        Task<Result<Common.Models.Bookings.BookingOrder>> Get(string referenceCode);

        Task<Result<Common.Models.Bookings.BookingOrder>> Get(Guid bookingId, int managerId);

        Task<Result<Common.Models.Bookings.BookingOrder>> Confirm(Guid bookingId, int managerId);
        
        Task<Result<Common.Models.Bookings.BookingOrder>> Book (EdoContracts.Accommodations.BookingRequest rooms, EdoContracts.Accommodations.AvailabilityRequest availabilityRequest, string languageCode);

        Task<Result> Cancel(string referenceCode);
    }
}