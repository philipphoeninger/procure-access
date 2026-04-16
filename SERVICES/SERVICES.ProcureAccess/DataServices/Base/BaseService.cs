namespace SERVICES.ProcureAccess.DataServices.Base;

public abstract class BaseService<TEntity, TDto> : IBaseService<TEntity, TDto>
    where TEntity : BaseEntity, new()
    where TDto : BaseDto, new()
{
    public IBaseRepo<TEntity> MainRepo { get; set; }
    public IMapper Mapper { get; set; }

    #region ctors
    public BaseService(IBaseRepo<TEntity> repo, IMapper mapper)
    {
        MainRepo = repo;
        Mapper = mapper;
    }
    #endregion

    #region methods
    public virtual async Task<Result<TDto?>> Create(TDto dto)
    {
        TEntity entity = Mapper.Map<TEntity>(dto);
        int success = MainRepo.Add(entity);
        return Convert.ToBoolean(success) ? await Read(entity.Id) : null;
    }

    public virtual async Task<Result<TDto?>> Read(int id)
    {
        TEntity? entity = MainRepo.Find(id);
        if (entity is null) return null; //gate
        return Result<TDto?>.Success(Mapper.Map<TDto>(entity));
    }

    public virtual async Task<Result<IEnumerable<TDto>>> ReadAll()
    {
        List<TEntity> entities = MainRepo.GetAll().ToList();
        List<TDto> dtos = new List<TDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<TDto>(x)));
        return Result<IEnumerable<TDto>>.Success(dtos);
    }

    public virtual async Task<Result<TDto?>> Update(TDto dto)
    {
        TEntity? entity = MainRepo.Find(dto.Id);
        if (entity is null) return null; //gate

        // update
        int success = MainRepo.Update(entity);
        TEntity updatedEntity = MainRepo.Find(dto.Id);

        return Convert.ToBoolean(success) 
            ? Result<TDto?>.Success(Mapper.Map<TDto>(updatedEntity)) 
            : Result<TDto?>.Failure("Entity could not be updated.");
    }

    public virtual int Delete(int id)
    {
        long binaryNow = DateTime.Now.ToBinary();
        byte[] arrayNow = BitConverter.GetBytes(binaryNow);
        return MainRepo.Delete(id, arrayNow);
    }
    #endregion
}
