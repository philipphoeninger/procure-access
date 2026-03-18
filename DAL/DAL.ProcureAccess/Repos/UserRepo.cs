namespace DAL.ProcureAccess.Repos;

public class UserRepo : IUserRepo
{
    private readonly bool _disposeContext;
    public ApplicationDBContext Context { get; }
    private readonly UserManager<User> _userManager;


    #region ctors
    public UserRepo(ApplicationDBContext context, UserManager<User> userManager)
    {
        Context = context;
        _userManager = userManager;
    }
    protected UserRepo(DbContextOptions<ApplicationDBContext> options, UserManager<User> userManager)
        : this(new ApplicationDBContext(options), userManager)
    {
        _disposeContext = true;
    }
    #endregion

    #region methods
    public async Task<User> GetAsync(string userId)
    {
        return await Context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .FirstAsync();
    }

    public async Task<IdentityResult> ChangeEmailAsync(
        string userId,
        string newEmail)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found."
            });

        // Optional: check if email already exists
        var existing = await _userManager.FindByEmailAsync(newEmail);
        if (existing != null && existing.Id != userId)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Email already in use."
            });
        }

        // Generate secure token
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

        if (!result.Succeeded)
            return result;

        // If you use Email as Username
        user.UserName = newEmail;
        user.NormalizedUserName = _userManager.NormalizeName(newEmail);

        await _userManager.UpdateAsync(user);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = "User not found."
            });
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            currentPassword,
            newPassword);

        if (!result.Succeeded)
            return result;

        // Optional but recommended: revoke all sessions
        await _userManager.UpdateSecurityStampAsync(user);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> ResetPasswordAsync(
        string email,
        string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found."
            });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return await _userManager.ResetPasswordAsync(
            user,
            token,
            newPassword);
    }

    public async Task<IdentityResult> RevokeSessionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found."
            });

        // This invalidates all existing login cookies/tokens
        await _userManager.UpdateSecurityStampAsync(user);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found."
            });

        // Anonymize
        user.Email = $"deleted_{Guid.NewGuid()}";
        user.UserName = user.Email;
        // Soft delete
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        // Invalidate all sessions
        await _userManager.UpdateSecurityStampAsync(user);

        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> RestoreUserAsync(string userId)
    {
        var user = await Context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found."
            });

        user.IsDeleted = false;
        user.DeletedAt = null;

        return await _userManager.UpdateAsync(user);
    }

    // public async Task UpdateDarkModeAsync(string userId, bool enabled)
    // {
    //     var user = new User { Id = userId };

    //     _context.Attach(user);

    //     user.UISettings = new UISettings { DarkMode = enabled };

    //     _context.Entry(user)
    //         .Property(u => u.UISettings)
    //         .IsModified = true;

    //     await _context.SaveChangesAsync();
    // }

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
