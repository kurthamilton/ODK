using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Venues;

namespace ODK.Services.Venues
{
    public interface IVenueService
    {
        Task<VersionedServiceResult<IReadOnlyCollection<Venue>>> GetVenues(long? currentVersion, Guid currentMemberId, Guid chapterId);
    }
}
