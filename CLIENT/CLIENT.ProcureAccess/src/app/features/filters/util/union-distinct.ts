// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

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
