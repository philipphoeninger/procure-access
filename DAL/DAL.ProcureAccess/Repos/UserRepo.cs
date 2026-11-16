// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Repos;

public class UserRepo : IUserRepo
{
    private readonly bool _disposeContext;
    public ApplicationDBContext Context { get; }
    private readonly UserManager<User> _userManager;
    private readonly TimeProvider _timeProvider;

    /// <summary>The <see cref="IdentityError.Code"/> of a delete for a user that does not exist.</summary>
    public const string UserNotFoundCode = "UserNotFound";

    #region ctors
    public UserRepo(ApplicationDBContext context, UserManager<User> userManager, TimeProvider timeProvider)
    {
        Context = context;
        _userManager = userManager;
        _timeProvider = timeProvider;
    }
    protected UserRepo(
        DbContextOptions<ApplicationDBContext> options, UserManager<User> userManager, TimeProvider timeProvider)
        : this(new ApplicationDBContext(options), userManager, timeProvider)
    {
        _disposeContext = true;
    }
    #endregion

    #region methods
    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Code = UserNotFoundCode,
                Description = "User not found."
            });

        // Anonymize
        user.Email = $"deleted_{Guid.NewGuid()}@anonymous.de";
        user.UserName = user.Email;
        // Soft delete
        user.IsDeleted = true;
        user.DeletedAt = _timeProvider.GetUtcNow().UtcDateTime;

        // Invalidate all sessions
        await _userManager.UpdateSecurityStampAsync(user);

        return await _userManager.UpdateAsync(user);
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private bool _isDisposed;
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }
        if (disposing)
        {
            if (_disposeContext)
            {
                Context.Dispose();
            }
        }
        _isDisposed = true;
    }
    #endregion

    ~UserRepo() // finalizer
    {
        Dispose(false);
    }
}
