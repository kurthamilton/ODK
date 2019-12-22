import { Venue } from 'src/app/core/venues/venue';
import { VenueStats } from 'src/app/core/venues/venue-stats';

export interface ListVenueViewModel {
  stats: VenueStats;
  venue: Venue;
}