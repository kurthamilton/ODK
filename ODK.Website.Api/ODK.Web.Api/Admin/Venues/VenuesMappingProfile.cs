﻿using AutoMapper;
using ODK.Services.Venues;
using ODK.Web.Api.Admin.Venues.Requests;
using ODK.Web.Api.Admin.Venues.Responses;

namespace ODK.Web.Api.Admin.Venues
{
    public class VenuesMappingProfile : Profile
    {
        public VenuesMappingProfile()
        {
            MapRequests();

            MapResponses();
        }

        private void MapRequests()
        {
            CreateMap<CreateVenueApiRequest, CreateVenue>();
        }

        private void MapResponses()
        {
            CreateMap<VenueStats, VenueStatsApiResponse>();
        }
    }
}
