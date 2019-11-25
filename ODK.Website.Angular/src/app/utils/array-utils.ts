export class ArrayUtils {
  static toMap<TKey, T>(arr: T[], getKey: (item: T) => TKey): Map<TKey, T> {
    return arr.reduce((map: Map<TKey, T>, current: T): Map<TKey, T> => {
      const key: TKey = getKey(current);
      map.set(key, current);
      return map;
    }, new Map<TKey, T>());
  }
}