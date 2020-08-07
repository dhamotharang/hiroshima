﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Hiroshima.Common.Infrastructure;
using HappyTravel.Hiroshima.DirectManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Hiroshima.WebApi.Controllers.DirectManager
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/management/locations")]
    [Produces("application/json")]
    public class LocationManagementController : ControllerBase
    {
        public LocationManagementController(ILocationManagementService locationManagementService)
        {
            _locationManagementService = locationManagementService;
        }

        
        /// <summary>
        /// Retrieves a location by country name and locality name (city name). If the locality doesn't exist adds a new one 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Hiroshima.DirectManager.Models.Responses.Location), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrAddLocation([FromBody] Hiroshima.DirectManager.Models.Requests.Location location)
        { 
            var (_, isFailure, response, error) = await _locationManagementService.GetOrAdd(location.Country, location.Locality);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(response);
        }
        
        
        /// <summary>
        /// Retrieves all locations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Hiroshima.DirectManager.Models.Responses.Location>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetLocations([FromQuery] int? skip = 0, [FromQuery] int? take = 500) 
            => Ok(await _locationManagementService.Get(take!.Value, skip!.Value));

        
        /// <summary>
        /// Retrieves all country names
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        [HttpGet("countries")]
        [ProducesResponseType(typeof(List<string>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCountryNames([FromQuery] int? skip = 0, [FromQuery] int? take = 500) 
            => Ok(await _locationManagementService.GetCountryNames(take!.Value, skip!.Value));

        
        private readonly ILocationManagementService _locationManagementService;
    }
}