import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-chapter-layout',
  templateUrl: './chapter-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterLayoutComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
