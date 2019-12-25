import { adminPaths } from './admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { Member } from 'src/app/core/members/member';
import { Venue } from 'src/app/core/venues/venue';

function chapterPath(chapter: Chapter, path?: string): string {
  return chapterUrl(chapter, adminPaths.chapter.path + (path ? `/${path}` : ''));
}

function chapterUrl(chapter: Chapter, path?: string): string {
    return `/${chapter.name.toLocaleLowerCase()}/admin` + (path ? `/${path}` : '');
}

function eventPath(chapter: Chapter, event: Event, path?: string): string {
  return chapterUrl(chapter, `${adminPaths.events.path}/${event.id}`) + (path ? `/${path}` : '');
}

const chapterPaths = adminPaths.chapter;
const eventPaths = adminPaths.events.event;

export const adminUrls = {
    chapter: (chapter: Chapter) => chapterPath(chapter),
    chapterAbout: (chapter: Chapter) => chapterPath(chapter, chapterPaths.about.path),
    chapterEmails: (chapter: Chapter) => chapterPath(chapter, chapterPaths.emails.path),
    chapterPayments: (chapter: Chapter) => chapterPath(chapter, chapterPaths.payments.path),
    event: (chapter: Chapter, event: Event) => eventPath(chapter, event),
    eventCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.events.path}/${adminPaths.events.create.path}`),
    eventInvites: (chapter: Chapter, event: Event) => eventPath(chapter, event, eventPaths.invites.path),
    eventResponses: (chapter: Chapter, event: Event) => eventPath(chapter, event, eventPaths.responses.path),
    events: (chapter: Chapter) => chapterUrl(chapter, adminPaths.events.path),
    member: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${adminPaths.members.path}/${member.id}`),
    memberImage: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${adminPaths.members.path}/${member.id}/${adminPaths.members.member.image.path}`),
    members: (chapter: Chapter) => chapterUrl(chapter, adminPaths.members.path),
    venue: (chapter: Chapter, venue: Venue) => chapterUrl(chapter, `${adminPaths.venues.path}/${venue.id}`),
    venueEvents: (chapter: Chapter, venue: Venue) => chapterUrl(chapter, `${adminPaths.venues.path}/${venue.id}/${adminPaths.venues.venue.events.path}`),
    venues: (chapter: Chapter) => chapterUrl(chapter, adminPaths.venues.path),
    venueCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.venues.path}/${adminPaths.venues.create.path}`)
};

Object.freeze(adminUrls);