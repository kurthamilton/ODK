﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Venues;
using ODK.Services.Venues;
using ODK.Web.Api.Admin.Venues.Requests;
using ODK.Web.Api.Venues.Responses;

namespace ODK.Web.Api.Admin.Venues
{
    [Authorize]
    [ApiController]
    [Route("Admin/Venues")]
    public class AdminVenuesController : OdkControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IVenueAdminService _venueAdminService;

        public AdminVenuesController(IVenueAdminService venueAdminService, IMapper mapper)
        {
            _mapper = mapper;
            _venueAdminService = venueAdminService;
        }

        [HttpPost]
        public async Task<ActionResult<VenueApiResponse>> Create([FromForm] CreateVenueApiRequest request)
        {
            CreateVenue venue = _mapper.Map<CreateVenue>(request);
            Venue created = await _venueAdminService.CreateVenue(GetMemberId(), venue);
            return _mapper.Map<VenueApiResponse>(created);
        }

        [HttpGet("{id}")]
        public async Task<VenueApiResponse> Get(Guid id)
        {
            Venue venue = await _venueAdminService.GetVenue(GetMemberId(), id);
            return _mapper.Map<VenueApiResponse>(venue);
        }

        [HttpPut("{id}")]
        public async Task<VenueApiResponse> Update(Guid id, [FromForm] CreateVenueApiRequest request)
        {
            CreateVenue update = _mapper.Map<CreateVenue>(request);
            Venue venue = await _venueAdminService.UpdateVenue(GetMemberId(), id, update);
            return _mapper.Map<VenueApiResponse>(venue);
        }
    }
}