import { Member } from 'src/app/core/members/member';
import { MemberSubscription } from 'src/app/core/members/member-subscription';

export interface AdminListMemberViewModel {
  member: Member;
  subscription: MemberSubscription;
}