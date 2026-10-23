// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaFiltersController : BaseCrudController<CriteriaFilter, CriteriaFilterDto, CriteriaFiltersController>
{
    public CriteriaFiltersController(
        IAppLogging<CriteriaFiltersController> logger,
        ICriteriaFilterService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
