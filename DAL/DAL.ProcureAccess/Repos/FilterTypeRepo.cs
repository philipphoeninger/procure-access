// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Repos;

public class FilterTypeRepo : TemporalTableBaseRepo<FilterType>, IFilterTypeRepo
{
    #region ctors
    public FilterTypeRepo(ApplicationDBContext context) : base(context) { }
    internal FilterTypeRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<FilterType> GetAll()
    // {
    //     return Context.FilterTypes;
    // }
    #endregion
}