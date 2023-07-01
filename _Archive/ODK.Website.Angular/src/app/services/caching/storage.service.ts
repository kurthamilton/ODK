import { Injectable } from '@angular/core';

const dateRegex = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*))(?:Z|(\+|-)([\d|:]*))?$/;

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  get<T>(key: string): T {
    return this.getFromStorage(key, localStorage);
  }

  set<T>(key: string, value: T): void {
    this.addToStorage(key, value, localStorage);
  }

  private addToStorage<T>(key: string, value: T, storage: Storage): void {
    value ? storage.setItem(key, JSON.stringify(value)) : storage.removeItem(key);
  }

  private getFromStorage<T>(key: string, storage: Storage): T {
    const text: string = storage.getItem(key);
    if (!text) {
      return null;
    }

    try {
      return this.parseJson(text);
    } catch {
      this.set(key, null);
      return null;
    }
  }

  private parseJson<T>(text: string): T {
    return JSON.parse(text, this.reviver);
  }

  private reviver(_: string, value: any): any {
    if (typeof value === 'string') {
        const match = dateRegex.exec(value);
        if (match) {
          return new Date(value);
        }
    }
    return value;
  }
}
