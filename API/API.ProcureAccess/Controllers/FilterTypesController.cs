// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilterTypesController : BaseCrudController<FilterType, FilterTypeDto, FilterTypesController>
{
    public FilterTypesController(
        IAppLogging<FilterTypesController> logger, 
        IFilterTypeService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
