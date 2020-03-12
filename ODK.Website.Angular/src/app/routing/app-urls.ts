import { appPaths } from './app-paths';
import { Chapter } from '../core/chapters/chapter';
import { Event } from '../core/events/event';
import { Member } from '../core/members/member';

function chapterUrl(chapter: Chapter, path?: string): string {
    return `/${chapter.name.toLocaleLowerCase()}` + (path ? `/${path}` : '');
}

const chapterPaths = appPaths.chapter.childPaths;

export const appUrls = {
    about: (chapter: Chapter) => chapterUrl(chapter, chapterPaths.about.path),
    adminChapter: (chapter: Chapter) => chapterUrl(chapter, 'admin'),
    chapter: (chapter: Chapter) => chapterUrl(chapter),
    contact: (chapter: Chapter) => chapterUrl(chapter, chapterPaths.contact.path),    
    event: (chapter: Chapter, event: Event) => chapterUrl(chapter, `${chapterPaths.events.path}/${event.id}`),
    events: (chapter: Chapter) => chapterUrl(chapter, chapterPaths.events.path),
    home: (chapter: Chapter) => chapter ? chapterUrl(chapter, '') : `/${appPaths.home.path}`,    
    login: (chapter: Chapter) => chapter ? chapterUrl(chapter, chapterPaths.login.path) : `/${appPaths.login.path}`,    
    member: (chapter: Chapter, member: Member) => chapterUrl(chapter, `${chapterPaths.members.path}/${member.id}`),
    members: (chapter: Chapter) => chapterUrl(chapter, chapterPaths.members.path),    
    privacy: `/${appPaths.privacy.path}`    
};

Object.freeze(appUrls);