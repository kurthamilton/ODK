export interface AuthenticationToken {
    accessToken: string;
    adminChapterIds?: string[];
    chapterId: string;
    memberId: string;
    membershipDisabled: boolean;
    refreshToken: string;
    subscriptionExpiryDate: Date;
    superAdmin?: boolean;
}
