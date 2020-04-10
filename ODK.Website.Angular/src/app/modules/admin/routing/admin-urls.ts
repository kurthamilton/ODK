import { adminPaths } from './admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterEmail } from 'src/app/core/emails/chapter-email';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { Event } from 'src/app/core/events/event';
import { Member } from 'src/app/core/members/member';
import { Venue } from 'src/app/core/venues/venue';
import { Email } from 'src/app/core/emails/email';

const chapterPaths = adminPaths.chapter;
const eventsPaths = adminPaths.events;
const eventPaths = eventsPaths.event;
const membersPaths = adminPaths.members;
const memberPaths = membersPaths.member;

const chapterPath = (chapter: Chapter, ...path: string[]): string => url(chapter, adminPaths.chapter.path, ...path);
const emailProvidersPath = (chapter: Chapter, ...path: string[]) => superAdminPath(chapter, adminPaths.emailProviders.path, ...path);
const eventPath = (chapter: Chapter, event: Event, ...path: string[]): string => eventsPath(chapter, event.id, ...path);
const eventsPath = (chapter: Chapter, ...path: string[]): string => url(chapter, eventsPaths.path, ...path);
const memberPath = (chapter: Chapter, member: Member, ...path: string[]): string => membersPath(chapter, member.id, ...path);
const membersPath = (chapter: Chapter, ...path: string[]): string => url(chapter, membersPaths.path, ...path);
const subscriptionsPath = (chapter: Chapter, ...path: string[]): string => membersPath(chapter, adminPaths.subscriptions.path, ...path);
const superAdminPath = (chapter: Chapter, ...path: string[]): string => url(chapter, adminPaths.superAdmin.path, ...path);
const venuesPath = (chapter: Chapter, ...path: string[]): string => eventsPath(chapter, adminPaths.venues.path, ...path);

const pathJoin = (parts: string[]): string => parts.filter(x => !!x).join('/');

const url = (chapter: Chapter, ...path: string[]): string => {
  const parts: string[] = [
    chapter.name.toLocaleLowerCase(),
    'admin',
    ...path
  ];

  return `/${pathJoin(parts)}`;
};

export const adminUrls = {
  adminMember: (chapter: Chapter, memberId: string) => membersPath(chapter, adminPaths.adminMembers.path, memberId),
  adminMemberAdd: (chapter: Chapter) => membersPath(chapter, adminPaths.adminMembers.path, adminPaths.adminMembers.add.path),
  adminMembers: (chapter: Chapter) => membersPath(chapter, adminPaths.adminMembers.path),

  chapter: (chapter: Chapter) => chapterPath(chapter),
  chapterEmail: (chapter: Chapter, email: ChapterEmail) => chapterPath(chapter, chapterPaths.emails.path, email.type.toString()),
  chapterEmails: (chapter: Chapter) => chapterPath(chapter, chapterPaths.emails.path),
  chapterLinks: (chapter: Chapter) => chapterPath(chapter, chapterPaths.links.path),
  chapterProperties: (chapter: Chapter) => chapterPath(chapter, chapterPaths.properties.path),
  chapterProperty: (chapter: Chapter, property: ChapterProperty) => chapterPath(chapter, chapterPaths.properties.path, property.id),
  chapterPropertyCreate: (chapter: Chapter) => chapterPath(chapter, chapterPaths.properties.path, chapterPaths.properties.create.path),
  chapterQuestion: (chapter: Chapter, question: ChapterQuestion) => chapterPath(chapter, chapterPaths.questions.path, question.id),
  chapterQuestionCreate: (chapter: Chapter) => chapterPath(chapter, chapterPaths.questions.path, chapterPaths.questions.create.path),
  chapterQuestions: (chapter: Chapter) => chapterPath(chapter, chapterPaths.questions.path),

  emailProvider: (chapter: Chapter, provider: ChapterEmailProvider) => emailProvidersPath(chapter, provider.id),
  emailProviderCreate: (chapter: Chapter) => emailProvidersPath(chapter, adminPaths.emailProviders.create.path),
  emailProviders: (chapter: Chapter) => emailProvidersPath(chapter),

  event: (chapter: Chapter, event: Event) => eventPath(chapter, event),
  eventAttendees: (chapter: Chapter, event: Event) => eventPath(chapter, event, eventPaths.attendees.path),
  eventCreate: (chapter: Chapter) => eventsPath(chapter, eventsPaths.create.path),
  eventInvites: (chapter: Chapter, event: Event) => eventPath(chapter, event, eventPaths.invites.path),
  events: (chapter: Chapter) => eventsPath(chapter),

  media: (chapter: Chapter) => url(chapter, adminPaths.media.path),

  member: (chapter: Chapter, member: Member) => memberPath(chapter, member),
  memberEvents: (chapter: Chapter, member: Member) => memberPath(chapter, member, memberPaths.events.path),
  memberImage: (chapter: Chapter, member: Member) => memberPath(chapter, member, memberPaths.image.path),
  memberImport: (chapter: Chapter) => membersPath(chapter, membersPaths.import.path),
  members: (chapter: Chapter) => membersPath(chapter),
  memberSendEmail: (chapter: Chapter, member: Member) => memberPath(chapter, member, memberPaths.sendEmail.path),

  subscription: (chapter: Chapter, subscription: ChapterSubscription) => subscriptionsPath(chapter, subscription.id),
  subscriptionCreate: (chapter: Chapter) => subscriptionsPath(chapter, adminPaths.subscriptions.create.path),
  subscriptions: (chapter: Chapter) => subscriptionsPath(chapter),

  superAdminEmail: (chapter: Chapter, email: Email) => superAdminPath(chapter, adminPaths.superAdmin.emails.path, email.type.toString()),
  superAdminEmails: (chapter: Chapter) => superAdminPath(chapter, adminPaths.superAdmin.emails.path),
  superAdminErrorLog: (chapter: Chapter) => superAdminPath(chapter, adminPaths.superAdmin.errorLog.path),
  superAdminInstagram: (chapter: Chapter) => superAdminPath(chapter, adminPaths.superAdmin.socialMedia.instagram.path),
  superAdminMembers: (chapter: Chapter) => superAdminPath(chapter, adminPaths.superAdmin.memberEmails.path),
  superAdminPaymentSettings: (chapter: Chapter) => superAdminPath(chapter, adminPaths.superAdmin.paymentSettings.path),

  venue: (chapter: Chapter, venue: Venue) => venuesPath(chapter, venue.id),
  venueCreate: (chapter: Chapter) => venuesPath(chapter, adminPaths.venues.create.path),
  venueEvents: (chapter: Chapter, venue: Venue) => venuesPath(chapter, venue.id, adminPaths.venues.venue.events.path),
  venues: (chapter: Chapter) => venuesPath(chapter),
};

Object.freeze(adminUrls);
