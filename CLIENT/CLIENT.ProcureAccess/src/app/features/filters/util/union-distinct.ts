export function unionDistinct<T>(
  arrays: T[][],
  key?: (item: T) => unknown
): T[] {
  // Primitive values
  if (!key) {
    return [...new Set(arrays.flat())];
  }

  // Objects
  return [
    ...new Map(
      arrays
        .flat()
        .map(item => [key(item), item])
    ).values()
  ];
}
