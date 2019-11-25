import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { DataType } from 'src/app/core/data-types/data-type';
import { environment } from 'src/environments/environment';

const baseUrl = `${environment.baseUrl}/datatypes`;

const endpoints = {
  dataTypes: baseUrl
};

@Injectable({
  providedIn: 'root'
})
export class DataTypeService {

  constructor(private http: HttpClient) { }

  private dataTypeMap: Map<string, DataType>;
  private dataTypes: DataType[];

  
  getDataTypeMap(): Observable<Map<string, DataType>> {
    if (this.dataTypeMap) {
      return of(this.dataTypeMap);
    }

    return this.getDataTypes().pipe(
      map((dataTypes: DataType[]) => ArrayUtils.toMap(dataTypes, x => x.id)),
      tap((map: Map<string, DataType>) => this.dataTypeMap = map)
    );
  }

  getDataTypes(): Observable<DataType[]> {
    if (this.dataTypes) {
      return of(this.dataTypes);
    }

    return this.http.get(endpoints.dataTypes).pipe(
      map((response: any) => response.map(x => this.mapDataType(x))),
      tap((dataTypes: DataType[]) => this.dataTypes = dataTypes)
    );
  }

  private mapDataType(response: any): DataType {
    return {
      id: response.id,
      name: response.name
    };
  }
}
