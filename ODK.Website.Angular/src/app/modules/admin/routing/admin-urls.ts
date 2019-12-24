import { adminPaths } from './admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { Member } from 'src/app/core/members/member';
import { Venue } from 'src/app/core/venues/venue';

function chapterUrl(chapter: Chapter, path?: string): string {
    return `/${chapter.name.toLocaleLowerCase()}/admin` + (path ? `/${path}` : '');
}

export const adminUrls = {
    chapter: (chapter: Chapter) => chapterUrl(chapter),
    event: (chapter: Chapter, event: Event) => chapterUrl(chapter, `${adminPaths.events.path}/${event.id}`),
    eventCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.events.path}/${adminPaths.events.create.path}`),
    eventInvites: (chapter: Chapter, event: Event) => chapterUrl(chapter, `${adminPaths.events.path}/${event.id}/${adminPaths.events.event.invites.path}`),
    eventResponses: (chapter: Chapter, event: Event) => chapterUrl(chapter, `${adminPaths.events.path}/${event.id}/${adminPaths.events.event.responses.path}`),
    events: (chapter: Chapter) => chapterUrl(chapter, adminPaths.events.path),
    member: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${adminPaths.members.path}/${member.id}`),
    members: (chapter: Chapter) => chapterUrl(chapter, adminPaths.members.path),
    venue: (chapter: Chapter, venue: Venue) => chapterUrl(chapter, `${adminPaths.venues.path}/${venue.id}`),
    venues: (chapter: Chapter) => chapterUrl(chapter, adminPaths.venues.path),
    venueCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.venues.path}/${adminPaths.venues.create.path}`)
};

Object.freeze(adminUrls);