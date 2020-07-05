﻿using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Hiroshima.WebApi.Services
{
    public interface IAvailabilityService
    {
       Task<Result<AvailabilityDetails, ProblemDetails>> GetAvailabilityDetails(AvailabilityRequest request, string languageCode);
    }
}