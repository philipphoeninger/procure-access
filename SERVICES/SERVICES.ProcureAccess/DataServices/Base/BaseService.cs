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
    public TDto? Create(TDto dto)
    {
        return null;
        // TEntity entity = Mapper.Map<>();
        // int success = MainRepo.Add(entity);
        // return Convert.ToBoolean(success) ? Read(entity.id) : null;
    }

    public TDto? Read(int id)
    {
        TEntity? entity = MainRepo.Find(id);
        if (entity is null) return null; //gate
        return Mapper.Map<TDto>(entity);
    }

    public IEnumerable<TDto> ReadAll()
    {
        List<TEntity> entities = MainRepo.GetAll().ToList();
        List<TDto> dtos = new List<TDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<TDto>(x)));
        return dtos;
    }

    public TDto? Update(TDto dto)
    {
        TEntity? entity = MainRepo.Find(dto.Id);
        if (entity is null) return null; //gate

        // TODO: update

        int success = MainRepo.Update(entity);
        return Convert.ToBoolean(success) ? Mapper.Map<TDto>(entity) : null;
    }

    public int Delete(int id)
    {
        return -1;
        //return MainRepo.Delete(id); // TODO: timestamp
    }
    #endregion
}
