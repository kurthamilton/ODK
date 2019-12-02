import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges, OnDestroy } from '@angular/core';

import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-member-image',
  templateUrl: './member-image.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberImageComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private memberService: MemberService
  ) {
  }

  @Input() maxWidth: number;
  @Input() member: Member;
  @Input() update: Observable<boolean>;

  image: string;
  loading = true;

  private destroyed: Subject<{}> = new Subject<{}>();

  ngOnChanges(): void {
    if (!this.member) {
      return;
    }

    if (this.update) {
      this.update.pipe(
        takeUntil(this.destroyed)
      ).subscribe(() => this.loadImage());
    }

    this.loadImage();
  }

  ngOnDestroy(): void {
    this.destroyed.next({});
    this.destroyed.complete();
  }

  private loadImage(): void {
    this.loading = true;
    this.memberService.getMemberImage(this.member.id, this.maxWidth).subscribe((image: string) => {
      this.image = image;
      this.loading = false;
      this.changeDetector.detectChanges();
    });
  }
}
