// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilterTypesController : BaseCrudController<FilterType, FilterTypeDto>
{
    public FilterTypesController(IFilterTypeService service) : base(service)
    {
    }

    // TODO: put more individual Requests here
}
