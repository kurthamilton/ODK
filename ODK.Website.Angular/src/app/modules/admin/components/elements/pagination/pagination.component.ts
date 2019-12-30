import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { PageGroup } from './page-group';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaginationComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef) {
  }

  @Input() container: HTMLElement;
  @Input() page: number;
  @Input() pageCount: number;

  @Output() pageChange: EventEmitter<number> = new EventEmitter<number>();

  activePage = 1;
  pageGroups: PageGroup[];

  private neighbours = 1;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['page']) {
      this.activePage = this.page;
    }

    if (this.pageCount) {
      this.setPageGroups();
    }
  }

  onPageChanged(page: number): void {
    this.activePage = page;
    this.setPageGroups();
    this.changeDetector.detectChanges();

    this.pageChange.emit(page);

    if (this.container && this.container.scrollIntoView) {
      this.container.scrollIntoView();
    }
  }

  private createPageGroups(pages: number[]): PageGroup[] {
    const pageGroups: PageGroup[] = [];

    pages = pages
      // filter out invalid pages
      .filter((value: number): boolean => value >= 1 && value <= this.pageCount)
      // filter out duplicate pages
      .filter((value: number, index: number, self: number[]): boolean => self.indexOf(value) === index)
      .sort((a: number, b: number): number => a - b);

    let prev: number;
    pages.forEach((page: number) => {
      if (!prev || page > prev + 1) {
        pageGroups.push({ pages: [] });
      }

      ArrayUtils.last(pageGroups).pages.push(page);
      prev = page;
    });

    return pageGroups;
  }

  private getPages(): number[] {
    const page: number = this.activePage;
    const pageCount: number = this.pageCount;
    const buffer: number = this.neighbours * 2;

    return [
      ...ArrayUtils.numbersBetween(1, page <= 1 + buffer ? 1 + buffer : 1),
      ...ArrayUtils.numbersBetween(page - this.neighbours, page + this.neighbours),
      ...ArrayUtils.numbersBetween(page >= pageCount - buffer ? pageCount - buffer : pageCount, pageCount)
    ];
  }

  private setPageGroups(): void {
    const pages: number[] = this.getPages();
    this.pageGroups = this.createPageGroups(pages);
  }
}
