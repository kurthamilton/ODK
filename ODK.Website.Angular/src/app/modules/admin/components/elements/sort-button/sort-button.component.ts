import { Component, ChangeDetectionStrategy, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-sort-button',
  templateUrl: './sort-button.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SortButtonComponent {

  @Output() sort: EventEmitter<number> = new EventEmitter<number>();

  onSort(): void {
    this.sort.emit();
  }
}
