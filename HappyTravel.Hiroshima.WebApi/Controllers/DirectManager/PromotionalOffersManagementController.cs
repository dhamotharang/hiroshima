﻿using System;
using System.Collections.Generic;
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
    [Route("api/{v:apiVersion}/management/")]
    [Produces("application/json")]
    public class PromotionalOffersManagementController : ControllerBase
    {
        public PromotionalOffersManagementController(IPromotionalOfferManagementService promotionalOfferManagementService)
        {
            _promotionalOfferManagementService = promotionalOfferManagementService;
        }


        /// <summary>
        /// Retrieves contracted promotional offers
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="roomIds">List of room ids</param>
        /// <param name="validFrom">Allows to set a lower bound date to get contracted promotional offers</param>
        /// <param name="validTo">Allows to set an upper bound date to get contracted promotional offers</param>
        /// <returns></returns>
        [HttpGet("contracts/{contractId}/promotionaloffers")]
        [ProducesResponseType(typeof(List<Hiroshima.DirectManager.Models.Responses.PromotionalOffer>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetPromotionalOffers([FromRoute] int contractId, [FromQuery(Name = "roomId")] List<int> roomIds = null, [FromQuery] DateTime? validFrom = null, [FromQuery] DateTime? validTo = null)
        {
            var (_, isFailure, response, error) = await _promotionalOfferManagementService.Get(contractId, roomIds == null ? new List<int>(): roomIds , validFrom, validTo);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(response);
        }


        /// <summary>
        /// Adds new promotional offers
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="promotionalOffers"></param>
        /// <returns></returns>
        [HttpPost("contracts/{contractId}/promotionaloffers")]
        [ProducesResponseType(typeof(List<Hiroshima.DirectManager.Models.Responses.PromotionalOffer>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddPromotionalOffers([FromRoute] int contractId, [FromBody] List<Hiroshima.DirectManager.Models.Requests.PromotionalOffer> promotionalOffers)
        {
            var (_, isFailure, response, error) = await _promotionalOfferManagementService.Add(contractId, promotionalOffers);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(response);
        }
        
        
        /// <summary>
        /// Removes contracted promotional offers by ID
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="promotionalOfferIds">IDs to remove</param>
        /// <returns></returns>
        [HttpDelete("contracts/{contractId}/promotionaloffers")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemovePromotionalOffers([FromRoute] int contractId, [FromBody] List<int> promotionalOfferIds)
        {
            var (_, isFailure, error) = await _promotionalOfferManagementService.Remove(contractId, promotionalOfferIds);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }

        private readonly IPromotionalOfferManagementService _promotionalOfferManagementService;
    }
}