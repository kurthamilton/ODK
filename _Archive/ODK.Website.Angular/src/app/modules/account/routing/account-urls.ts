import { accountPaths } from './account-paths';
import { appPaths } from 'src/app/routing/app-paths';
import { Chapter } from 'src/app/core/chapters/chapter';

function chapterUrl(chapter: Chapter, path: string) {
  return `/${(chapter ? `${chapter.name.toLocaleLowerCase()}/` : '')}${appPaths.account.path}/${path}`;
}

export const accountUrls = {
  delete: (chapter: Chapter) => chapterUrl(chapter, accountPaths.delete.path),
  emails: (chapter: Chapter) => chapterUrl(chapter, accountPaths.emails.path),
  join: (chapter: Chapter) => chapterUrl(chapter, accountPaths.join.path),
  logout: (chapter: Chapter) => chapterUrl(chapter, accountPaths.logout.path),
  password: {
    change: (chapter: Chapter) => chapterUrl(chapter, accountPaths.password.change.path),
    forgotten: (chapter: Chapter) => chapterUrl(chapter, accountPaths.password.forgotten.path),
    reset: (chapter: Chapter) => chapterUrl(chapter, accountPaths.password.reset.path),
  },
  profile: (chapter: Chapter) => chapterUrl(chapter, ''),
  subscription: (chapter: Chapter) => chapterUrl(chapter, accountPaths.subscription.path),
  updateEmailAddress: (chapter: Chapter) => chapterUrl(chapter, accountPaths.updateEmailAddress.path)
};

Object.freeze(accountUrls);
