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
  
  static toMap<TKey, T>(arr: T[], getKey: (item: T) => TKey): Map<TKey, T> {
    return arr.reduce((map: Map<TKey, T>, current: T): Map<TKey, T> => {
      const key: TKey = getKey(current);
      map.set(key, current);
      return map;
    }, new Map<TKey, T>());
  }
}