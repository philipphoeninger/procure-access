namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IBaseService<TEntity, TDto>
    where TEntity : BaseEntity, new()
    where TDto : BaseDto, new()
{
    IBaseRepo<TEntity> MainRepo { get; set; }
    IMapper Mapper { get; set; }
    TDto? Create(TDto dto);
    TDto? Read(int id);
    IEnumerable<TDto> ReadAll();
    TDto? Update(TDto dto);
    int Delete(int id);
}
