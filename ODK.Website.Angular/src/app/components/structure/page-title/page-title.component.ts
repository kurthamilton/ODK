import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

@Component({
  selector: 'app-page-title',
  templateUrl: './page-title.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PageTitleComponent {
  @Input() title: string;
}
