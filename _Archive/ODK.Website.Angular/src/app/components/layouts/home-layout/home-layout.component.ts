import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

@Component({
  selector: 'app-home-layout',
  templateUrl: './home-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeLayoutComponent {
  @Input() title: string;
}
