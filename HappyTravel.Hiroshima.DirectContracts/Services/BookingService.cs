﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Hiroshima.Common.Infrastructure.Utilities;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Common.Models.Availabilities;
using HappyTravel.Hiroshima.Common.Models.Enums;
using HappyTravel.Hiroshima.Data;
using HappyTravel.Hiroshima.Data.Extensions;
using HappyTravel.Hiroshima.DirectContracts.Extensions.FunctionalExtensions;
using HappyTravel.Hiroshima.DirectContracts.Services.Availability;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Hiroshima.DirectContracts.Services
{
    public class BookingService : IBookingService
    {
        public BookingService(IAvailabilityDataStorage availabilityDataStorage, IAvailabilityService availabilityService, DirectContractsDbContext dbContext)
        {
            _availabilityDataStorage = availabilityDataStorage;
            _availabilityService = availabilityService;
            _dbContext = dbContext;
        }


        public Task<Result<Common.Models.Bookings.BookingOrder>> Get(string referenceCode)
            => Get(_dbContext.BookingOrders.Where(bo => bo.ReferenceCode.Equals(referenceCode)));


        public Task<Result<Common.Models.Bookings.BookingOrder>> Get(Guid bookingId, int managerId)
            => Get(_dbContext.BookingOrders.Where(bo => bo.Id.Equals(bookingId) && bo.ContractManagerId.Equals(managerId)));
           
        
        private async Task<Result<Common.Models.Bookings.BookingOrder>> Get(IQueryable<Common.Models.Bookings.BookingOrder> source)
        {
            var bookingOrder = await source.SingleOrDefaultAsync();
            
            return bookingOrder == null
                ? Result.Failure<Common.Models.Bookings.BookingOrder>($"Failed to retrieve the booking order")
                : Result.Success(bookingOrder);
        }
        

        public async Task<Result<Common.Models.Bookings.BookingOrder>> Book(EdoContracts.Accommodations.BookingRequest bookingRequest, EdoContracts.Accommodations.AvailabilityRequest availabilityRequest, string languageCode)
        {
            return await GetRequiredHash()
                .BindWithTransaction(_dbContext, requiredHash => GetAvailableRates(requiredHash)
                    .Map(CreateBookingEntry)
                    .Map(AddBookingEntry)
                    .Bind(AddRoomOccupancy));
            
            
            async Task<Result<string>> GetRequiredHash()
            {
                var hash = await _availabilityDataStorage.GetHash(bookingRequest.AvailabilityId, bookingRequest.RoomContractSetId);

                return string.IsNullOrEmpty(hash)
                    ? Result.Failure<string>("Failed to retrieve the required hash")
                    : Result.Success(hash);
            }


            async Task<Result<AvailableRates>> GetAvailableRates(string requiredHash)
            {
                var error = "Available rates not found";
                var availability = await _availabilityService.Get(availabilityRequest, languageCode);
                
                if (!availability.AvailableRates.Any())
                    Result.Failure<Common.Models.Availabilities.Availability>(error);

                foreach (var availableRates in availability.AvailableRates)
                {
                   var requiredRates = availableRates.Value.SingleOrDefault(ar => ar.Hash.Equals(requiredHash));
                   if (requiredRates != null)
                       return Result.Success(requiredRates);
                }
                
                return Result.Failure<AvailableRates>(error);
            }
            
            
            (Common.Models.Bookings.BookingOrder, AvailableRates) CreateBookingEntry(AvailableRates availableRates)
            {
                var accommodation = availableRates.Rates.First().Room.Accommodation;
                var utcNow = DateTime.UtcNow;
            
                return (new Common.Models.Bookings.BookingOrder
                {
                    Status = BookingStatuses.WaitingForConfirmation,
                    ReferenceCode = bookingRequest.ReferenceCode,
                    CheckInDate = availabilityRequest.CheckInDate,
                    CheckOutDate = availabilityRequest.CheckOutDate,
                    Created = utcNow,
                    Modified = utcNow,
                    BookingRequest = JsonDocumentUtilities.CreateJDocument(bookingRequest),
                    AvailabilityRequest = JsonDocumentUtilities.CreateJDocument(availabilityRequest),
                    AvailableRates = JsonDocumentUtilities.CreateJDocument(availableRates),
                    LanguageCode = languageCode,
                    ContractManagerId = accommodation.ContractManagerId,
                    AccommodationId = accommodation.Id
                }, availableRates);
            }


            async Task<(Common.Models.Bookings.BookingOrder bookingOrder, AvailableRates availableRates)> AddBookingEntry((Common.Models.Bookings.BookingOrder bookingOrder, AvailableRates availableRates) bookingOrderAndAvailableRates)
            {
                var entry = _dbContext.BookingOrders.Add(bookingOrderAndAvailableRates.bookingOrder);
                await _dbContext.SaveChangesAsync();
                _dbContext.DetachEntry(entry.Entity);
                
                return (entry.Entity, bookingOrderAndAvailableRates.availableRates);
            }
            
            
            async Task<Result<Common.Models.Bookings.BookingOrder>> AddRoomOccupancy((Common.Models.Bookings.BookingOrder bookingOrder, AvailableRates availableRates) bookingOrderAndAvailableRates)
            {
                var utcNow = DateTime.UtcNow;
                
                foreach (var roomId in bookingOrderAndAvailableRates.availableRates.Rates.Select(rd => rd.Room.Id))
                {
                    _dbContext.RoomOccupancies.Add(new RoomOccupancy
                    {
                        RoomId = roomId,
                        Created = utcNow,
                        FromDate = availabilityRequest.CheckInDate,
                        ToDate = availabilityRequest.CheckOutDate,
                        BookingOrderId = bookingOrderAndAvailableRates.bookingOrder.Id
                    });
                }

                await _dbContext.SaveChangesAsync();
                
                return Result.Success(bookingOrderAndAvailableRates.bookingOrder);
            }
        }


        public async Task<Result> Cancel(string referenceCode)
        {
            return await Get(referenceCode)
                .Ensure(BookingCanBeCancelled, $"The booking order '{referenceCode}' has already been cancelled")
                .BindWithTransaction(_dbContext, bookingOrder => RemoveRoomOccupancies(bookingOrder).Map(ChangeBookingStatus));


            bool BookingCanBeCancelled(Common.Models.Bookings.BookingOrder bookingOrder)
            {
                //TODO Add cancellation policies checking
                return bookingOrder.Status == BookingStatuses.Processing || 
                    bookingOrder.Status == BookingStatuses.WaitingForConfirmation ||
                    bookingOrder.Status == BookingStatuses.Confirmed && DateTime.UtcNow.Date < bookingOrder.CheckInDate.Date;
            }
           

            async Task<Result<Common.Models.Bookings.BookingOrder>> RemoveRoomOccupancies(Common.Models.Bookings.BookingOrder bookingOrder)
            {
                var roomOccupancies = await _dbContext.RoomOccupancies.Where(ro => ro.BookingOrderId.Equals(bookingOrder.Id)).ToListAsync();
                _dbContext.RoomOccupancies.RemoveRange(roomOccupancies);
                await _dbContext.SaveChangesAsync();
                
                return Result.Success(bookingOrder);
            }
            
            
            async Task ChangeBookingStatus(Common.Models.Bookings.BookingOrder bookingOrder)
            {
                bookingOrder.Status = BookingStatuses.WaitingForCancellation;
                bookingOrder.Modified = DateTime.UtcNow;
                var entry = _dbContext.BookingOrders.Update(bookingOrder);
                await _dbContext.SaveChangesAsync();
                _dbContext.DetachEntry(entry.Entity);
            }
        }


        public Task<Result<Common.Models.Bookings.BookingOrder>> Confirm(Guid bookingId, int managerId)
        {
            return Get(bookingId, managerId)
                .Ensure(IsBookingStatusValid, $"Failed to confirm the booking order with id '{bookingId}'")
                .Ensure(IsDateValid, "Check-in date less or equal the current date")
                .Map(bookingOrder => SetStatus(bookingOrder, BookingStatuses.Confirmed));
            
            
            bool IsBookingStatusValid(Common.Models.Bookings.BookingOrder bookingOrder) 
                => bookingOrder.Status == BookingStatuses.WaitingForConfirmation;


            bool IsDateValid(Common.Models.Bookings.BookingOrder bookingOrder) 
                => DateTime.UtcNow.Date <= bookingOrder.CheckInDate.Date;
        }

        
        private Task<Common.Models.Bookings.BookingOrder> SetStatus(Common.Models.Bookings.BookingOrder bookingOrder, BookingStatuses status)
        {
            bookingOrder.Modified = DateTime.UtcNow;
            bookingOrder.Status = status;
            return Update(bookingOrder);
        }
        
        
        private async Task<Common.Models.Bookings.BookingOrder> Update(Common.Models.Bookings.BookingOrder bookingOrder)
        {
            var entry = _dbContext.BookingOrders.Update(bookingOrder);
            await _dbContext.SaveChangesAsync();
            _dbContext.DetachEntry(entry.Entity);
            return entry.Entity;
        }
        

        private readonly DirectContractsDbContext _dbContext;
        private readonly IAvailabilityDataStorage _availabilityDataStorage;
        private readonly IAvailabilityService _availabilityService;
    }
}