export class ArrayUtils {
  static groupValues<T, TKey, TValue>(arr: T[], getKey: (item: T) => TKey, getValue: (item: T) => TValue): Map<TKey, TValue[]> {
    return arr.reduce((group: Map<TKey, TValue[]>, current: T): Map<TKey, TValue[]> => {
      const key: TKey = getKey(current);
      if (!group.has(key)) {
        group.set(key, []);
      }

      const value: TValue = getValue(current);
      group.get(key).push(value);
      return group;
    }, new Map<TKey, TValue[]>());
  }
  
  static hasValues<T>(arr: T[]): boolean {
    return !!arr && arr.length > 0;
  }
  
  static segment<T>(arr: T[], segmentSize: number): T[][] {
    const segments: T[][] = [];

    if (!segmentSize || segmentSize < 0) {
      segmentSize = 1;
    }

    arr.forEach((element: T, i: number) => {
      if (i % segmentSize === 0) {
        segments.push([]);
      }

      this.last(segments).push(element);
    });

    return segments;
  }

  static toMap<TKey, T>(arr: T[], getKey: (item: T) => TKey): Map<TKey, T> {
    return arr.reduce((map: Map<TKey, T>, current: T): Map<TKey, T> => {
      const key: TKey = getKey(current);
      map.set(key, current);
      return map;
    }, new Map<TKey, T>());
  }  

  private static last<T>(arr: T[]): T {
    if (!this.hasValues(arr)) {
      return;
    }

    return arr[arr.length - 1];
  }
}