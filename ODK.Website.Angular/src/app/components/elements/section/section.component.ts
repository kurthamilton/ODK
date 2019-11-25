import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

@Component({
  selector: 'app-section',
  templateUrl: './section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectionComponent {
  @Input() hero: boolean;
  @Input() theme: string;
  @Input() title: string;
}
