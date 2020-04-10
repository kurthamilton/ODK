import { MemberProperty } from './member-property';

export interface MemberProfile {
  firstName: string;
  joined: Date;
  lastName: string;
  properties: MemberProperty[];
}
