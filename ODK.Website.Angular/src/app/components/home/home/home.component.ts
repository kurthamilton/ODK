import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Country } from 'src/app/core/countries/country';
import { CountryService } from 'src/app/services/countries/country.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private chapterService: ChapterService,
    private countryService: CountryService
  ) {
  }

  chapters: Chapter[];
  continents: string[];
  countries: Country[];

  ngOnInit(): void {
    forkJoin([
      this.chapterService.getChapters().pipe(
        tap((chapters: Chapter[]) => this.chapters = chapters)
      ),
      this.countryService.getCountries().pipe(
        tap((countries: Country[]) => this.countries = countries)
      )
    ]).subscribe(() => {
      this.continents = this.countries
        .map(x => x.continent)
        .filter((value: string, index: number, self: string[]) => self.indexOf(value) === index);
      this.changeDetector.detectChanges();

      if (this.route.snapshot.fragment) {
        const element: HTMLElement = document.getElementById(this.route.snapshot.fragment);
        if (element) {
          element.scrollIntoView();
        }
      }
    });
  }
}
