﻿using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace Hiroshima.WebApi.Services
{
    public interface IAvailability
    {
        Task<Result<AvailabilityDetails, ProblemDetails>> GetAvailabilities(AvailabilityRequest request, string languageCode);
    }
}
