using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityProvider.API.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>> _applicationManager;
        private readonly OpenIddictTokenManager<OpenIddictEntityFrameworkCoreToken<Guid>> tokenManager;

        public AuthorizationController(OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>> applicationManager, OpenIddictTokenManager<OpenIddictEntityFrameworkCoreToken<Guid>> tokenManager)
        {
            _applicationManager = applicationManager;
            this.tokenManager = tokenManager;
        }

        //var request = HttpContext.GetOpenIddictServerRequest();

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            //OpenIddictRequest request
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.
                var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
                if (application == null)
                {
                    throw new InvalidOperationException("The application details cannot be found in the database.");
                }

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    Claims.Name, Claims.Role);

                var clientDetails = await _applicationManager.GetClientIdAsync(application);
                var clientName = await _applicationManager.GetDisplayNameAsync(application);

                // Use the client_id as the subject identifier.
                identity.AddClaim(Claims.Subject, clientDetails, Destinations.AccessToken, Destinations.IdentityToken);
                identity.AddClaim(Claims.Name, clientName, Destinations.AccessToken, Destinations.IdentityToken);

                var tokens = await tokenManager.FindBySubjectAsync(application.ClientId).ToListAsync();
                if (tokens.Count > 0)
                {
                    foreach (var token in tokens)
                    {
                        _ = tokenManager.UpdateAsync(token);
                    }
                }

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }
    }
}
