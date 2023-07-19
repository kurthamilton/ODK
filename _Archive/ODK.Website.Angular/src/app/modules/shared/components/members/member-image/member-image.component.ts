import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges, OnDestroy } from '@angular/core';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { LoadingSpinnerOptions } from '../../elements/loading-spinner/loading-spinner-options';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-member-image',
  templateUrl: './member-image.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberImageComponent implements OnChanges, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private memberService: MemberService
  ) {
  }

  @Input() isTop: boolean;
  @Input() maxWidth: number;
  @Input() member: Member;
  @Input() update: Observable<boolean>;

  image: string;
  imageUrl: string;
  loading = true;
  loadingOptions: LoadingSpinnerOptions = {
    overlay: true,
    small: true
  };

  ngOnChanges(): void {
    if (!this.member) {
      return;
    }

    if (this.update) {
      this.update.pipe(
        takeUntil(componentDestroyed(this))
      ).subscribe(() => {
        this.loadImage();
        this.changeDetector.detectChanges();
      });
    }

    this.loadImage();
  }

  ngOnDestroy(): void {
    this.changeDetector.detach();
  }

  onLoad(): void {
    this.loading = false;
    this.changeDetector.detectChanges();
  }

  private loadImage(): void {
    const forceReload: boolean = !!this.imageUrl;
    this.imageUrl = this.memberService.getMemberImageUrl(this.member.id, this.maxWidth, forceReload);
  }
}
