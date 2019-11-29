import { adminPaths } from './admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';

function chapterUrl(chapter: Chapter, path?: string): string {
    return `/${chapter.name.toLocaleLowerCase()}/admin` + (path ? `/${path}` : '');
}

export const adminUrls = {
    chapter: (chapter: Chapter) => chapterUrl(chapter),
    event: (chapter: Chapter, event: Event) => chapterUrl(chapter, `${adminPaths.events.path}/${event.id}`),
    eventCreate: (chapter: Chapter) => chapterUrl(chapter, `${adminPaths.events.path}/${adminPaths.events.create.path}`),
    events: (chapter: Chapter) => chapterUrl(chapter, adminPaths.events.path)
};

Object.freeze(adminUrls);