﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Venues;
using ODK.Web.Common;
using ODK.Web.Common.Venues.Responses;

namespace ODK.Web.Api.Venues
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class VenuesController : OdkControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService, IMapper mapper)
        {
            _mapper = mapper;
            _venueService = venueService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VenueApiResponse>>> GetVenues(Guid chapterId)
        {
            return await HandleVersionedRequest(
                version => _venueService.GetVenues(version, GetMemberId(), chapterId),
                x => x.Select(_mapper.Map<VenueApiResponse>));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<VenueApiResponse>> Get(Guid id)
        {
            return await HandleVersionedRequest(
                version => _venueService.GetVenue(version, TryGetMemberId(), id),
                _mapper.Map<VenueApiResponse>);
        }

        [AllowAnonymous]
        [HttpGet("Public")]
        public async Task<ActionResult<IEnumerable<VenueApiResponse>>> GetPublicVenues(Guid chapterId)
        {
            return await HandleVersionedRequest(
                version => _venueService.GetPublicVenues(version, chapterId),
                x => x.Select(_mapper.Map<VenueApiResponse>));
        }
    }
}
