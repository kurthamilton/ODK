import { Member } from 'src/app/core/members/member';
import { MemberEmail } from 'src/app/core/members/member-email';

export interface MemberEmailViewModel {
  member: Member;
  memberEmail: MemberEmail;
}
