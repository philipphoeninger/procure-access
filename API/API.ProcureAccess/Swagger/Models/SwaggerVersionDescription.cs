// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Swagger.Models;

public class SwaggerVersionDescription
{
    public int MajorVersion { get; set; }

    public int MinorVersion { get; set; }

    public string Status { get; set; }

    public string Description { get; set; }
}
