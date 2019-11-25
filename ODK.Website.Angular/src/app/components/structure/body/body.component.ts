import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

@Component({
  selector: 'app-body',
  templateUrl: './body.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BodyComponent {

  @Input() title: string;
}
