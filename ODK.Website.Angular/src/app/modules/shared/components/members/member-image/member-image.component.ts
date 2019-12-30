import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges, OnDestroy } from '@angular/core';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
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

  @Input() isTop: boolean;
  @Input() maxWidth: number;
  @Input() member: Member;
  @Input() update: Observable<boolean>;

  image: string;
  loading = true;

  ngOnChanges(): void {
    if (!this.member) {
      return;
    }

    if (this.update) {
      this.update.pipe(
        takeUntil(componentDestroyed(this))
      ).subscribe(() => this.loadImage());
    }

    this.loadImage();
  }

  ngOnDestroy(): void {
    this.changeDetector.detach();
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
