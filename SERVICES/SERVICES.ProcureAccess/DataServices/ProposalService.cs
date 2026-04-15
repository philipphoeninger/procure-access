namespace SERVICES.ProcureAccess.DataServices;

public class ProposalService : BaseService<Proposal, ProposalDto>, IProposalService
{
    private IHttpContextAccessor _httpContextAccessor;
    private readonly IProductService _productService;
    private readonly ICriterionService _criterionService;

    public ProposalService(
        IProposalRepo repo, 
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IProductService productService,
        ICriterionService criterionService) : base(repo, mapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _productService = productService;
        _criterionService = criterionService;
    }

    // CREATE / UPSERT PROPOSAL (stores snapshot)
    public async Task<ProposalDto> UpsertAsync(UpsertProposalCommand cmd)
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null; //gate
        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var userPermissions = user
                        .Claims
                        .Where(x => x.Type == CustomClaimTypes.Permission)
                        .Select(x => x.Value);

        var strategy = MainRepo.Context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await MainRepo.Context.Database.BeginTransactionAsync();

            try
            {
                ValidateCommand(cmd);

                Proposal entity;

                if (cmd.Id.HasValue)
                {
                    entity = await MainRepo.Context.Proposals
                        .FirstAsync(p => p.Id == cmd.Id.Value);
                    
                    if (!userPermissions.Any(x => x == Permissions.ObjectsApprove) &&
                        entity.ProposerId != userId)
                        throw new Exception("User is not authorized to update this proposal.");
                }
                else
                {
                    entity = new Proposal
                    {
                        ProposerId = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    MainRepo.Context.Proposals.Add(entity);
                }

                // Store snapshot instead of real entity
                if (cmd.Product != null)
                {
                    entity.ProductSnapshot = JsonSerializer.Serialize(cmd.Product);
                    entity.CriterionSnapshot = null;
                }

                if (cmd.Criterion != null)
                {
                    entity.CriterionSnapshot = JsonSerializer.Serialize(cmd.Criterion);
                    entity.ProductSnapshot = null;
                }

                await MainRepo.Context.SaveChangesAsync();
                await tx.CommitAsync();

                return await MapToDtoAsync(entity);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    // GET SINGLE PROPOSAL (with snapshot)
    public override async Task<Result<ProposalDto?>> Read(int id)
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null; //gate

        Proposal entity = await MainRepo.Context.Proposals
            .Include(p => p.Product)
            .Include(p => p.Criterion)
            .FirstAsync(p => p.Id == id);

        ProposalDto dto = await MapToDtoAsync(entity);

        if (dto is null) return null; //gate

        var userPermissions = user
                        .Claims
                        .Where(x => x.Type == CustomClaimTypes.Permission)
                        .Select(x => x.Value);

        if (!userPermissions.Any(x => x == Permissions.ObjectsApprove) &&
            dto.ProposerId != user.FindFirstValue(ClaimTypes.NameIdentifier))
            return null; //gate

        return Result<ProposalDto?>.Success(dto);
    }

    public override async Task<Result<IEnumerable<ProposalDto>>> ReadAll()
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null; //gate

        var userPermissions = user
                        .Claims
                        .Where(x => x.Type == CustomClaimTypes.Permission)
                        .Select(x => x.Value);

        List<Proposal> entities = new List<Proposal>();

        if (userPermissions.Any(x => x == Permissions.ObjectsApprove))
        {
            entities = MainRepo.Context.Proposals
                .Include(x => x.Product)
                .Include(x => x.Criterion)
                .Where(x => 
                !(
                    (x.Product != null && x.Product.IsDeleted) || 
                    (x.Criterion != null && x.Criterion.IsDeleted)
                )).ToList();
        }
        else
        {
            entities = MainRepo.Context.Proposals
                .Include(x => x.Product)
                .Include(x => x.Criterion)
                .Where(x => 
                !(
                    (x.Product != null && x.Product.IsDeleted) || 
                    (x.Criterion != null && x.Criterion.IsDeleted)
                ) && 
                x.ProposerId == user.FindFirstValue(ClaimTypes.NameIdentifier)).ToList();
        }

        var dtos = new List<ProposalDto>();
        entities.ForEach(async x => {
            var dto = await MapToDtoAsync(x);
            dtos.Add(dto);
        });

        return Result<IEnumerable<ProposalDto>>.Success(dtos);
    }

    public override int Delete(int id)
    {
        Proposal? proposalEntity = MainRepo.Find(id);
        if (proposalEntity is null) return -1; //gate

        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return -1; //gate

        var userPermissions = user
                        .Claims
                        .Where(x => x.Type == CustomClaimTypes.Permission)
                        .Select(x => x.Value);

        if (!userPermissions.Any(x => x == Permissions.ObjectsApprove) ||
            proposalEntity.ProposerId != user.FindFirstValue(ClaimTypes.NameIdentifier)) return -1; //gate

        // delete
        long binaryNow = DateTime.Now.ToBinary();
        byte[] arrayNow = BitConverter.GetBytes(binaryNow);
        return MainRepo.Delete(id, arrayNow);
    }

    // REVIEW (approver/proposer edits snapshot)
    public async Task<ProposalDto> ReviewAsync(ReviewProposalCommand cmd)
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null; //gate
        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var userPermissions = user
                        .Claims
                        .Where(x => x.Type == CustomClaimTypes.Permission)
                        .Select(x => x.Value);

        var proposal = await MainRepo.Context.Proposals
            .FirstAsync(p => p.Id == cmd.ProposalId);

        if (proposal.FinishedAt != null)
            throw new Exception("Proposal already finalized.");

        if (!userPermissions.Any(x => x == Permissions.ObjectsApprove) &&
            proposal.ProposerId != userId)
                throw new Exception("User is not authorized to update this proposal.");
                
        if (cmd.Product != null)
        {
            proposal.ProductSnapshot = JsonSerializer.Serialize(cmd.Product);
            proposal.CriterionSnapshot = null;
        }

        if (cmd.Criterion != null)
        {
            proposal.CriterionSnapshot = JsonSerializer.Serialize(cmd.Criterion);
            proposal.ProductSnapshot = null;
        }

        await MainRepo.Context.SaveChangesAsync();

        return await MapToDtoAsync(proposal);
    }

    // APPROVE / REJECT
    public async Task<ProposalDto> ApproveAsync(ApproveProposalCommand cmd)
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        var userPermissions = user
                .Claims
                .Where(x => x.Type == CustomClaimTypes.Permission)
                .Select(x => x.Value);
        if (!userPermissions.Any(x => x == Permissions.ObjectsApprove))
            throw new Exception("User is not authorized to approve proposals.");

        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        var strategy = MainRepo.Context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await MainRepo.Context.Database.BeginTransactionAsync();
        
            try
            {
                var proposal = await MainRepo.Context.Proposals
                    .Include(p => p.Product)
                    .Include(p => p.Criterion)
                    .FirstOrDefaultAsync(p => p.Id == cmd.ProposalId);

                if (proposal == null)
                    throw new Exception("Proposal not found.");

                if (proposal.FinishedAt != null)
                    throw new Exception("Proposal already processed.");

                proposal.Status = cmd.IsApproved 
                    ? ProposalStatus.Approved 
                    : ProposalStatus.Rejected;
                proposal.Note = cmd.Note;
                proposal.ApproverId = userId;
                proposal.FinishedAt = DateTime.UtcNow;

                if (cmd.IsApproved)
                {
                    await ApplyApprovalSideEffects(proposal);
                }

                await MainRepo.Context.SaveChangesAsync();
                await tx.CommitAsync();

                return await MapToDtoAsync(proposal);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    // APPLY SNAPSHOT → REAL ENTITY
    private async Task ApplyApprovalSideEffects(Proposal proposal)
    {
        if (!string.IsNullOrEmpty(proposal.ProductSnapshot))
        {
            var dto = JsonSerializer.Deserialize<CreateProductDto>(proposal.ProductSnapshot);

            var product = Mapper.Map<Product>(dto);

            MainRepo.Context.Products.Add(product);

            proposal.Product = product;
            proposal.Criterion = null;
        }

        if (!string.IsNullOrEmpty(proposal.CriterionSnapshot))
        {
            var dto = JsonSerializer.Deserialize<CreateCriterionDto>(proposal.CriterionSnapshot);

            var criterion = Mapper.Map<Criterion>(dto);

            MainRepo.Context.Criteria.Add(criterion);

            proposal.Criterion = criterion;
            proposal.Product = null;
        }
    }

    // MAPPING (entity → DTO + snapshot restore)
    private async Task<ProposalDto> MapToDtoAsync(Proposal proposal)
    {
        var dto = Mapper.Map<ProposalDto>(proposal);

        // If not yet approved → use snapshot
        if (proposal.FinishedAt == null)
        {
            if (!string.IsNullOrEmpty(proposal.ProductSnapshot))
            {
                dto.Product = JsonSerializer.Deserialize<ProductDto>(proposal.ProductSnapshot);
            }

            if (!string.IsNullOrEmpty(proposal.CriterionSnapshot))
            {
                dto.Criterion = JsonSerializer.Deserialize<CriterionDto>(proposal.CriterionSnapshot);
            }
        }

        return dto;
    }

    // VALIDATION
    private void ValidateCommand(UpsertProposalCommand cmd)
    {
        var hasProduct = cmd.Product != null;
        var hasCriterion = cmd.Criterion != null;

        if (hasProduct == hasCriterion)
        {
            throw new Exception("Exactly one of Product or Criterion must be provided.");
        }
    }
}
