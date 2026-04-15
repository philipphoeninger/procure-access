namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IBaseService<TEntity, TDto>
    where TEntity : BaseEntity, new()
    where TDto : BaseDto, new()
{
    IBaseRepo<TEntity> MainRepo { get; set; }
    IMapper Mapper { get; set; }
    Task<Result<TDto?>> Create(TDto dto);
    Task<Result<TDto?>> Read(int id);
    Task<Result<IEnumerable<TDto>>> ReadAll();
    Task<Result<TDto?>> Update(TDto dto);
    int Delete(int id);
}
