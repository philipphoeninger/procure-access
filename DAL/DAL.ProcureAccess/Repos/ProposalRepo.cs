namespace DAL.ProcureAccess.Repos;

public class ProposalRepo : TemporalTableBaseRepo<Proposal>, IProposalRepo
{
    #region ctors
    public ProposalRepo(ApplicationDBContext context) : base(context) { }
    internal ProposalRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<Proposal> GetAll()
    // {
    //     return Context.Proposals;
    // }
    #endregion
}
