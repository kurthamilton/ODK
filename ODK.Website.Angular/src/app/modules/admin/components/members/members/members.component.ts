import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AdminListMemberViewModel } from './admin-list-member.view-model';
import { AdminMember } from 'src/app/core/members/admin-member';
import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DateUtils } from 'src/app/utils/date-utils';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MemberFilterViewModel } from '../member-filter/member-filter.view-model';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { SortEvent } from '../../../directives/sortable-header/sort-event';
import { SubscriptionType } from 'src/app/core/account/subscription-type';

@Component({
  selector: 'app-members',
  templateUrl: './members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MembersComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private chapterAdminService: ChapterAdminService,
    private memberAdminService: MemberAdminService,
    private authenticationService: AuthenticationService
  ) {
  }

  filter: MemberFilterViewModel;
  subscriptionTypes = SubscriptionType;
  subscriptionWarningDate: Date = DateUtils.addDays(DateUtils.today(), 7);
  superAdmin: boolean;
  today: Date = DateUtils.today();
  viewModels: AdminListMemberViewModel[];
  
  private chapter: Chapter;
  private imageQueue: AdminMember[];
  private members: AdminMember[];
  private memberSubscriptions: MemberSubscription[];
  private sortBy: SortEvent;
  private subscriptionMap: Map<string, MemberSubscription>;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      this.memberAdminService.getAdminMembers(this.chapter.id).pipe(
        tap((members: AdminMember[]) => this.members = members)
      ),
      this.memberAdminService.getMemberSubscriptions(this.chapter.id).pipe(
        tap((subscriptions: MemberSubscription[]) => this.memberSubscriptions = subscriptions)
      )
    ]).subscribe(() => {      
      this.subscriptionMap = ArrayUtils.toMap(this.memberSubscriptions, x => x.memberId);
      this.filter = {
        name: '',
        types: this.route.snapshot.queryParamMap.getAll('type').length 
          ? this.route.snapshot.queryParamMap.getAll('type').map(x => parseInt(x, 10))
          : [
            SubscriptionType.Trial, SubscriptionType.Full, SubscriptionType.Partial
          ]
      };

      this.filterMembers(this.filter);
      this.changeDetector.detectChanges();
    });

    const token: AuthenticationToken = this.authenticationService.getToken();
    this.superAdmin = token.superAdmin === true;
  }

  getMemberUrl(member: AdminMember): string {
    return adminUrls.member(this.chapter, member);
  }

  onFilterChange(filter: MemberFilterViewModel): void {
    this.filterMembers(filter);
    this.sortMembers();
    this.changeDetector.detectChanges();
  }

  onSort(sortBy: SortEvent) {
    this.sortBy = sortBy;
    this.sortMembers();
    this.changeDetector.detectChanges();
  }

  onUploadPicture(files: FileList): void {
    if (files.length === 0) {
      return;
    }

    this.imageQueue = [];

    for (let i = 0; i < files.length; i++) {
      const file: File = files.item(i);
      const name: string = file.name.substr(0, file.name.indexOf('.'));
      const member: AdminMember = this.members.find(x => x.fullName.toLocaleLowerCase() === name.toLocaleLowerCase());
      if (member) {
        this.imageQueue.push(member);
        this.uploadPicture(member, file);
      }
    }
  }

  private filterMembers(filter: MemberFilterViewModel): void {
    this.viewModels = this.members
      .filter(member => {
        if (filter.types && filter.types.length && !filter.types.includes(this.subscriptionMap.get(member.id).type)) {
          return false;
        }

        if (filter.name && !member.fullName.toLocaleLowerCase().includes(filter.name.toLocaleLowerCase())) {
          return false;
        }

        return true;
      })
      .map((member: AdminMember): AdminListMemberViewModel => {
        return {
          member: member,
          subscription: this.subscriptionMap.get(member.id)
        };
      });
  }

  private sortMembers(): void {
    this.viewModels.sort((a, b) => {
      if (this.sortBy?.direction === 'desc') {
        [a, b] = [b, a];
      }

      switch (this.sortBy?.column) {
        case 'expires':
          return DateUtils.compare(a.subscription.expiryDate, b.subscription.expiryDate);
        case 'emailOptIn':
          return a.member.emailOptIn === b.member.emailOptIn ? 0 : 
           (a.member.emailOptIn ? 1 : -1);
        default:
          return a.member.fullName.localeCompare(b.member.fullName);
      }      
    });
  }

  private uploadPicture(member: AdminMember, image: File): void {
    this.memberAdminService.updateImage(member.id, image).subscribe(() => {
      this.imageQueue.splice(this.imageQueue.indexOf(member), 1);
    });
  }
}
