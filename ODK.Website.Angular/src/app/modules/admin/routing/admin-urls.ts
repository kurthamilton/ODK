import { adminPaths } from './admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { Event } from 'src/app/core/events/event';
import { Member } from 'src/app/core/members/member';
import { Venue } from 'src/app/core/venues/venue';

const chapterPaths = adminPaths.chapter;
const eventsPaths = adminPaths.events;
const eventPaths = eventsPaths.event;
const membersPaths = adminPaths.members;
const memberPaths = membersPaths.member;

const chapterPath = (chapter: Chapter, ...path: string[]): string => url(chapter, adminPaths.chapter.path, ...path);
const eventPath = (chapter: Chapter, event: Event, ...path: string[]): string => eventsPath(chapter, event.id, ...path);
const eventsPath = (chapter: Chapter, ...path: string[]): string => url(chapter, eventPaths.path, ...path);
const memberPath = (chapter: Chapter, member: Member, ...path: string[]): string => membersPath(chapter, member.id, ...path);
const membersPath = (chapter: Chapter, ...path: string[]): string => url(chapter, membersPaths.path, ...path);
const subscriptionsPath = (chapter: Chapter, ...path: string[]): string => membersPath(chapter, ...path);
const venuesPath = (chapter: Chapter, ...path: string[]): string => membersPath(chapter, adminPaths.venues.path, ...path);

const pathJoin = (parts: string[]): string =>parts.filter(x => !!x).join('/');

const url = (chapter: Chapter, ...path: string[]): string => {
  const parts: string[] = [
    chapter.name.toLocaleLowerCase(),
    'admin',
    ...path
  ];

  return `/${pathJoin(parts)}`;
};

export const adminUrls = {
  adminLog: (chapter: Chapter) => url(chapter, adminPaths.admin.path),

  adminMember: (chapter: Chapter, memberId: string) => chapterPath(chapter, adminPaths.adminMembers.path, memberId),
  adminMemberAdd: (chapter: Chapter) => chapterPath(chapter, adminPaths.adminMembers.path, adminPaths.adminMembers.add.path),
  adminMembers: (chapter: Chapter) => chapterPath(chapter, adminPaths.adminMembers.path),

  chapter: (chapter: Chapter) => chapterPath(chapter),
  chapterMedia: (chapter: Chapter) => chapterPath(chapter, chapterPaths.media.path),
  chapterPayments: (chapter: Chapter) => chapterPath(chapter, chapterPaths.payments.path),
  chapterProperties: (chapter: Chapter) => chapterPath(chapter, chapterPaths.properties.path),
  chapterProperty: (chapter: Chapter, property: ChapterProperty) => chapterPath(chapter, chapterPaths.properties.path, property.id),
  chapterPropertyCreate: (chapter: Chapter) => chapterPath(chapter, chapterPaths.properties.path, chapterPaths.properties.create.path),
  chapterQuestion: (chapter: Chapter, question: ChapterQuestion) => chapterPath(chapter, chapterPaths.questions.path, question.id),
  chapterQuestionCreate: (chapter: Chapter) => chapterPath(chapter, chapterPaths.questions.path, chapterPaths.questions.create.path),
  chapterQuestions: (chapter: Chapter) => chapterPath(chapter, chapterPaths.questions.path),

  emailProvider: (chapter: Chapter, provider: ChapterEmailProvider) => url(chapter, adminPaths.emails.path, adminPaths.emails.emailProviders.path, provider.id),
  emailProviderCreate: (chapter: Chapter) => url(chapter, adminPaths.emails.path, adminPaths.emails.emailProviders.path, adminPaths.emails.emailProviders.create.path),
  emailProviders: (chapter: Chapter) => url(chapter, adminPaths.emails.path, adminPaths.emails.emailProviders.path),
  emails: (chapter: Chapter) => url(chapter, adminPaths.emails.path),
  emailsDefault: (chapter: Chapter) => url(chapter, adminPaths.emails.path, adminPaths.emails.default.path),

  event: (chapter: Chapter, event: Event) => eventPath(chapter, event),
  eventAttendees: (chapter: Chapter, event: Event) => eventPath(chapter, event, eventPaths.attendees.path),
  eventCreate: (chapter: Chapter) => eventsPath(chapter, eventsPaths.create.path),
  eventInvites: (chapter: Chapter, event: Event) => eventPath(chapter, event, eventPaths.invites.path),
  events: (chapter: Chapter) => eventsPath(chapter),

  member: (chapter: Chapter, member: Member) => memberPath(chapter, member),
  memberEvents: (chapter: Chapter, member: Member) => memberPath(chapter, member, memberPaths.events.path),
  memberImage: (chapter: Chapter, member: Member) => memberPath(chapter, member, memberPaths.image.path),
  members: (chapter: Chapter) => membersPath(chapter),
  memberSendEmail: (chapter: Chapter, member: Member) => memberPath(chapter, member, memberPaths.sendEmail.path),

  subscription: (chapter: Chapter, subscription: ChapterSubscription) => subscriptionsPath(chapter, subscription.id),
  subscriptionCreate: (chapter: Chapter) => subscriptionsPath(chapter, adminPaths.subscriptions.create.path),
  subscriptions: (chapter: Chapter) => subscriptionsPath(chapter),

  venue: (chapter: Chapter, venue: Venue) => venuesPath(chapter, venue.id),
  venueCreate: (chapter: Chapter) => venuesPath(chapter, adminPaths.venues.create.path),
  venueEvents: (chapter: Chapter, venue: Venue) => venuesPath(chapter, venue.id, adminPaths.venues.venue.events.path),
  venues: (chapter: Chapter) => venuesPath(chapter),
};

Object.freeze(adminUrls);