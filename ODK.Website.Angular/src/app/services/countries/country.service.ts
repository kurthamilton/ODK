import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { Country } from 'src/app/core/countries/country';
import { environment } from 'src/environments/environment';

const baseUrl = `${environment.apiBaseUrl}/countries`;

const endpoints = {
  countries: baseUrl
};

@Injectable({
  providedIn: 'root'
})
export class CountryService {

  constructor(private http: HttpClient) { }

  getCountries(): Observable<Country[]> {
    return this.http.get(endpoints.countries).pipe(
      map((response: any) => response.map(x => this.mapCountry(x)))
    );
  }
  
  getCountry(id: string): Observable<Country> {
    return this.getCountries().pipe(
      map(countries => countries.find(x => x.id === id))
    );
  }

  private mapCountry(response: any): Country {
    return {
      continent: response.continent,
      currencyCode: response.currencyCode,
      currencySymbol: response.currencySymbol,
      id: response.id,
      name: response.name
    };
  }
}
