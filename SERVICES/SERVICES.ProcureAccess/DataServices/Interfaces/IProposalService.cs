namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IProposalService : IBaseService<Proposal, ProposalDto>
{
    public Task<ProposalDto> UpsertAsync(UpsertProposalCommand cmd);
    public Task<ProposalDto> ReviewAsync(ReviewProposalCommand cmd);
    public Task<ProposalDto> ApproveAsync(ApproveProposalCommand cmd);
}
