// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/** Role names of the legacy JWT; the platform roles of ARCHITECTURE §9.1 since S4-25. */
export enum EnUserRole {
    member = 'Member',
    moderator = 'Moderator',
    catalogEditor = 'CatalogEditor',
    platformAdmin = 'PlatformAdmin'
}
