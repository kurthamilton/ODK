import { Member } from './member';

export interface AdminMember extends Member {
  emailOptIn: boolean;
}
