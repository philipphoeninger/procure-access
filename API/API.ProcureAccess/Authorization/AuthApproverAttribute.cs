namespace API.ProcureAccess.Authorization;

public class AuthApproverAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var userPermissions = context.HttpContext.User.Claims.Where(x => x.Type == CustomClaimTypes.Permission).Select(x => x.Value);
        if (!userPermissions.Any(x => x == Permissions.ObjectsApprove))
        {
            throw new UnauthorizedAccessException("Not authorized for approving objects.");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
