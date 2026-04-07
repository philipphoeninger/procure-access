namespace SERVICES.ProcureAccess.DataServices;

public class ProposalService : BaseService<Proposal, ProposalDto>, IProposalService
{
    public ProposalService(IProposalRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override IEnumerable<ProposalDto> ReadAll()
    {
        List<Proposal> entities = MainRepo.GetAll().ToList();
        entities = entities.FindAll(x => 
            !(
                (x.Product != null && x.Product.IsDeleted) || 
                (x.Criterion != null && x.Criterion.IsDeleted)
            ));
        List<ProposalDto> dtos = new List<ProposalDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProposalDto>(x)));
        return dtos;
    }
}
