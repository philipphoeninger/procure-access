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
    public virtual TDto? Create(TDto dto)
    {
        TEntity entity = Mapper.Map<TEntity>(dto);
        int success = MainRepo.Add(entity);
        return Convert.ToBoolean(success) ? Read(entity.Id) : null;
    }

    public virtual TDto? Read(int id)
    {
        TEntity? entity = MainRepo.Find(id);
        if (entity is null) return null; //gate
        return Mapper.Map<TDto>(entity);
    }

    public virtual IEnumerable<TDto> ReadAll()
    {
        List<TEntity> entities = MainRepo.GetAll().ToList();
        List<TDto> dtos = new List<TDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<TDto>(x)));
        return dtos;
    }

    public virtual TDto? Update(TDto dto)
    {
        TEntity? entity = MainRepo.Find(dto.Id);
        if (entity is null) return null; //gate

        // update
        TEntity updatedEntity = Mapper.Map<TEntity>(dto);
        int success = MainRepo.Update(entity);

        return Convert.ToBoolean(success) ? Mapper.Map<TDto>(updatedEntity) : null;
    }

    public virtual int Delete(int id)
    {
        long binaryNow = DateTime.Now.ToBinary();
        byte[] arrayNow = BitConverter.GetBytes(binaryNow);
        return MainRepo.Delete(id, arrayNow);
    }
    #endregion
}
