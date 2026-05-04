export function intersectionBy<T>(
  arrays: T[][],
  key: (item: T) => unknown
): T[] {
  if (arrays.length === 0) {
    return [];
  }

  const sets = arrays
    .slice(1)
    .map(arr => new Set(arr.map(key)));

  return arrays[0].filter(item =>
    sets.every(set => set.has(key(item)))
  );
}
