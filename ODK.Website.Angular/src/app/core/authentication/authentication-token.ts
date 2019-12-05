export interface AuthenticationToken {
    accessToken: string;
    adminChapterIds?: string[];
    chapterId: string;
    memberId: string;
    refreshToken: string;
    subscriptionExpiryDate: Date;
    superAdmin?: boolean;
}
