namespace DAL.ProcureAccess.Repos.Interfaces;

public interface IApproveRepo<T> where T : BaseDto
{
    int Approve(T dto);
}
