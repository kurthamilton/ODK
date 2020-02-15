import { AdminMember } from 'src/app/core/members/admin-member';
import { MemberSubscription } from 'src/app/core/members/member-subscription';

export interface AdminListMemberViewModel {
  member: AdminMember;
  subscription: MemberSubscription;
}