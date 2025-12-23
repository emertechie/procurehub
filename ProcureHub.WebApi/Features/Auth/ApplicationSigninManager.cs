using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProcureHub.Features.Users;

namespace ProcureHub.WebApi.Features.Auth;

public class ApplicationSigninManager : SignInManager<Models.User>
{
    private readonly UserSigninValidator _userSigninValidator;

    public ApplicationSigninManager(
        UserManager<Models.User> userManager,
        IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<Models.User> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<Models.User>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<Models.User> confirmation,
        // Inject application-specific user validator:
        UserSigninValidator userSigninValidator
    ) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _userSigninValidator = userSigninValidator;
    }

    public override async Task<bool> CanSignInAsync(Models.User user)
    {
        if (!await base.CanSignInAsync(user))
        {
            return false;
        }

        // Check application-specific rules:
        return await _userSigninValidator.CanSignInAsync(user);
    }
}
