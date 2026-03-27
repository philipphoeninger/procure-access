namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IApproveService<T> where T : BaseDto
{
    int Approve(T dto);
}
