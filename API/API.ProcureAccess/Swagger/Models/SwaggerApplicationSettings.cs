// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Swagger.Models;

public class SwaggerApplicationSettings
{
    public string Title { get; set; } = null!;

    public List<SwaggerVersionDescription> Descriptions { get; set; } = new List<SwaggerVersionDescription>();

    public string ContactName { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;
}
