// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaFiltersController : BaseCrudController<CriteriaFilter, CriteriaFilterDto>
{
    public CriteriaFiltersController(ICriteriaFilterService service) : base(service)
    {
    }

    // TODO: put more individual Requests here
}
