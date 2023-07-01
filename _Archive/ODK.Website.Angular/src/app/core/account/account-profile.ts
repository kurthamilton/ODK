import { MemberProfile } from '../members/member-profile';

export interface AccountProfile extends MemberProfile {
    emailAddress: string;
    emailOptIn: boolean;
}
