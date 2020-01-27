import { adminPaths } from './admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
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
  adminLog: (chapter: Chapter) => chapterUrl(chapter, adminPaths.admin.path),
  chapter: (chapter: Chapter) => chapterPath(chapter),
  chapterAdminMember: (chapter: Chapter, memberId: string) => chapterPath(chapter, `${chapterPaths.adminMembers.path}/${memberId}`),
  chapterAdminMemberAdd: (chapter: Chapter) => chapterPath(chapter, `${chapterPaths.adminMembers.path}/${adminPaths.chapter.adminMembers.add.path}`),
  chapterAdminMembers: (chapter: Chapter) => chapterPath(chapter, chapterPaths.adminMembers.path),
  chapterPayments: (chapter: Chapter) => chapterPath(chapter, chapterPaths.payments.path),
  chapterProperties: (chapter: Chapter) => chapterPath(chapter, chapterPaths.properties.path),
  chapterProperty: (chapter: Chapter, property: ChapterProperty) => chapterPath(chapter, `${chapterPaths.properties.path}/${property.id}`),
  chapterPropertyCreate: (chapter: Chapter) => chapterPath(chapter, `${chapterPaths.properties.path}/${adminPaths.chapter.properties.create.path}`),
  chapterQuestion: (chapter: Chapter, question: ChapterQuestion) => chapterPath(chapter, `${chapterPaths.questions.path}/${question.id}`),
  chapterQuestionCreate: (chapter: Chapter) => chapterPath(chapter, `${chapterPaths.questions.path}/${adminPaths.chapter.questions.create.path}`),
  chapterQuestions: (chapter: Chapter) => chapterPath(chapter, chapterPaths.questions.path),
  chapterSubscription: (chapter: Chapter, subscription: ChapterSubscription) => chapterPath(chapter, `${adminPaths.chapter.subscriptions.path}/${subscription.id}`),
  chapterSubscriptionCreate: (chapter: Chapter) => chapterPath(chapter, `${adminPaths.chapter.subscriptions.path}/${adminPaths.chapter.subscriptions.create.path}`),
  chapterSubscriptions: (chapter: Chapter) => chapterPath(chapter, adminPaths.chapter.subscriptions.path),
  emailProvider: (chapter: Chapter, provider: ChapterEmailProvider) => chapterUrl(chapter, `${adminPaths.emails.path}/${adminPaths.emails.emailProviders.path}/${provider.id}`),
  emailProviderCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.emails.path}/${adminPaths.emails.emailProviders.path}/${adminPaths.emails.emailProviders.create.path}`),
  emailProviders: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.emails.path}/${adminPaths.emails.emailProviders.path}`),
  emails: (chapter: Chapter) => chapterUrl(chapter, adminPaths.emails.path),
  emailsDefault: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.emails.path}/${adminPaths.emails.default.path}`),
  event: (chapter: Chapter, event: Event) => eventPath(chapter, event),
  eventAttendees: (chapter: Chapter, event: Event) => eventPath(chapter, event, adminPaths.events.event.attendees.path),
  eventCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.events.path}/${adminPaths.events.create.path}`),
  eventInvites: (chapter: Chapter, event: Event) => eventPath(chapter, event, eventPaths.invites.path),
  events: (chapter: Chapter) => chapterUrl(chapter, adminPaths.events.path),
  media: (chapter: Chapter) => chapterUrl(chapter, adminPaths.media.path),
  member: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${adminPaths.members.path}/${member.id}`),
  memberEvents: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${adminPaths.members.path}/${member.id}/${adminPaths.members.member.events.path}`),
  memberImage: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${adminPaths.members.path}/${member.id}/${adminPaths.members.member.image.path}`),
  members: (chapter: Chapter) => chapterUrl(chapter, adminPaths.members.path),
  memberSendEmail: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${adminPaths.members.path}/${member.id}/${adminPaths.members.member.sendEmail.path}`),
  venue: (chapter: Chapter, venue: Venue) => chapterUrl(chapter, `${adminPaths.venues.path}/${venue.id}`),
  venueEvents: (chapter: Chapter, venue: Venue) => chapterUrl(chapter, `${adminPaths.venues.path}/${venue.id}/${adminPaths.venues.venue.events.path}`),
  venues: (chapter: Chapter) => chapterUrl(chapter, adminPaths.venues.path),
  venueCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.venues.path}/${adminPaths.venues.create.path}`)
};

Object.freeze(adminUrls);