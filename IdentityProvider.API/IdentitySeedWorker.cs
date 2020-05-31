using IdentityProvider.DataAccess.Databases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;
using IdentityProvider.DataAccess.Models;

namespace IdentityProvider.API
{
    public class IdentitySeedWorker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public IdentitySeedWorker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            await context.Database.EnsureCreatedAsync();

            await RegisterApplicationsAsync(scope.ServiceProvider);
            await RegisterScopesAsync(scope.ServiceProvider);

            static async Task RegisterApplicationsAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<OpenIddictApplicationManager<IdentityOpenIddictApplication>>();
                if (await manager.FindByClientIdAsync("mvc") == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "mvc",
                        ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "MVC client application",
                        PostLogoutRedirectUris =
                        {
                            new Uri("http://localhost:53507/signout-callback-oidc")
                        },
                        RedirectUris =
                        {
                            new Uri("http://localhost:53507/signin-oidc")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "demo_api"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        },

                    });
                }

                if (await manager.FindByClientIdAsync("console") == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "console",
                        ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                        DisplayName = "My client application",
                        Permissions =
                        {
                            Permissions.Endpoints.Token,
                            Permissions.GrantTypes.ClientCredentials
                        },
                    });
                }

                // To test this sample with Postman, use the following settings:
                //
                // * Authorization URL: http://localhost:54540/connect/authorize
                // * Access token URL: http://localhost:54540/connect/token
                // * Client ID: postman
                // * Client secret: [blank] (not used with public clients)
                // * Scope: openid email profile roles
                // * Grant type: authorization code
                // * Request access token locally: yes
                if (await manager.FindByClientIdAsync("postman") == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "postman",
                        ConsentType = ConsentTypes.Systematic,
                        DisplayName = "Postman",
                        RedirectUris =
                        {
                            new Uri("urn:postman")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Device,
                            Permissions.Endpoints.Token,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.DeviceCode,
                            Permissions.GrantTypes.Password,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles
                        }
                    });
                }
            }

            static async Task RegisterScopesAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope<Guid>>>();
                if (await manager.FindByNameAsync("demo_api") == null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        DisplayName = "Demo API access",
                        Name = "demo_api",
                        Resources =
                        {
                            "resource_server"
                        }
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
