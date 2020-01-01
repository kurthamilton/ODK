import { Directive, Input, Output, EventEmitter } from '@angular/core';

import { SortDirection } from './sort-direction';
import { SortEvent } from './sort-event';

const rotate: {[key: string]: SortDirection} = { 'asc': 'desc', 'desc': 'asc' };

@Directive({
  selector: 'th[sortable]',
  host: {
    '[class.sort-asc]': 'sorted === "asc"',
    '[class.sort-desc]': 'sorted === "desc"',
    '(click)': 'onClick()'
  }
})
export class SortableHeaderDirective {

  @Input() direction: SortDirection;
  @Input() sortable: string;
  @Output() sort = new EventEmitter<SortEvent>();
  
  sorted: SortDirection;

  emit(): void {
    this.sorted = rotate[this.sorted] || this.direction || 'asc';
    this.sort.emit({
      column: this.sortable, 
      direction: this.sorted
    });
  }
  
  reset(): void {
    this.sorted = null; 
  }

  onClick(): void {
    this.emit();
  }
}
