import { Directive, ContentChildren, QueryList, AfterContentInit, OnDestroy, Input, ChangeDetectorRef } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { SortEvent } from '../sortable-header/sort-event';
import { SortableHeaderDirective } from '../sortable-header/sortable-header.directive';

@Directive({
  selector: 'table[sortable]'
})
export class SortableTableDirective implements AfterContentInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef) {

  }

  @Input() sortable: string;

  @ContentChildren(SortableHeaderDirective) headers: QueryList<SortableHeaderDirective>;

  ngAfterContentInit(): void {
    if (!this.headers) {
      return;
    }

    const initial: SortableHeaderDirective = this.headers.find(x => x.sortable === this.sortable);
    if (initial) {
      initial.emit();
    }

    this.headers.forEach(header => {
      header.sort.pipe(
        takeUntil(componentDestroyed(this))
      ).subscribe((event: SortEvent) => {
        this.headers.forEach(x => {
          if (x.sortable !== event.column) {
            x.reset();
          }
        });
        this.changeDetector.detectChanges();
      })
    });
  }

  ngOnDestroy(): void { }
}
