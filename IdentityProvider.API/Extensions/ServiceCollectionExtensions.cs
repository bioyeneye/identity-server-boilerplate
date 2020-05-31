using IdentityProvider.DataAccess.Databases;
using IdentityProvider.DataAccess.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityProvider.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationIdentityService(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseOpenIddict<Guid>();
                options.UseOpenIddict<IdentityOpenIddictApplication, IdentityOpenIddictAuthorization, IdentityOpenIddictScope, IdentityOpenIddictToken, Guid>();
                options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); });
            });

            // Register the Identity services.
            services.AddIdentity<IdentityApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            //services.AddOpenIddict<IdentityOpenIddictApplication, IdentityOpenIddictAuthorization, IdentityOpenIddictScope, IdentityOpenIddictToken>()
            services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<IdentityDbContext>()
                           .ReplaceDefaultEntities<Guid>();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {


                    // Enable the authorization, device, logout, token, userinfo and verification endpoints.
                    options.SetAuthorizationEndpointUris("/connect/authorize")
                           .SetDeviceEndpointUris("/connect/device")
                           .SetLogoutEndpointUris("/connect/logout")
                           .SetTokenEndpointUris("/connect/token")
                           .SetUserinfoEndpointUris("/connect/userinfo")
                           .SetVerificationEndpointUris("/connect/verify");

                    options.UseRollingTokens();

                    // Note: the Mvc.Client sample only uses the code flow and the password flow, but you
                    // can enable the other flows if you need to support implicit or client credentials.
                    options.AllowAuthorizationCodeFlow()
                           .AllowClientCredentialsFlow()
                           .AllowDeviceCodeFlow()
                           .AllowPasswordFlow()
                           .AllowRefreshTokenFlow();


                    // Mark the "email", "profile", "roles" and "demo_api" scopes as supported scopes.
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "demo_api");

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate()
                           .AddEphemeralSigningKey();

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .EnableStatusCodePagesIntegration()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableLogoutEndpointPassthrough()
                           .EnableTokenEndpointPassthrough()
                           .EnableUserinfoEndpointPassthrough()
                           .EnableVerificationEndpointPassthrough()
                           .DisableTransportSecurityRequirement(); // During development, you can disable the HTTPS requirement.

                    // Note: if you don't want to specify a client_id when sending
                    // a token or revocation request, uncomment the following line:
                    //
                    options.AcceptAnonymousClients();

                    // Note: if you want to process authorization and token requests
                    // that specify non-registered scopes, uncomment the following line:
                    //
                    // options.DisableScopeValidation();

                    // Note: if you don't want to use permissions, you can disable
                    // permission enforcement by uncommenting the following lines:
                    //
                    // options.IgnoreEndpointPermissions()
                    //        .IgnoreGrantTypePermissions()
                    //        .IgnoreScopePermissions();

                    // Note: when issuing access tokens used by third-party APIs
                    // you don't own, you can disable access token encryption:
                    options.DisableAccessTokenEncryption();
                })


                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Configure the audience accepted by this resource server.
                    // The value MUST match the audience associated with the
                    // "demo_api" scope, which is used by ResourceController.
                    options.AddAudiences("resource_server");

                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();

                    // For applications that need immediate access token or authorization
                    // revocation, the database entry of the received tokens and their
                    // associated authorizations can be validated for each API call.
                    // Enabling these options may have a negative impact on performance.
                    //
                    options.EnableAuthorizationEntryValidation();
                    options.EnableTokenEntryValidation();
                });
            return services;
        }

        public static IServiceCollection AddApplicationCorsService(this IServiceCollection services, string corsPolicyName = "applicationcorspolicy")
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: corsPolicyName,
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin();
                                      builder.AllowAnyMethod();
                                      builder.AllowAnyHeader();
                                      //builder.AllowCredentials();
                                  });
            });

            return services;
        }

        public static IServiceCollection AddApplicationSSlEnforce(this IServiceCollection services, bool enforceSSl = true)
        {
            if (enforceSSl)
            {
                services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(60);
                    //options.ExcludedHosts.Add("example.com");
                    //options.ExcludedHosts.Add("www.example.com");
                });

                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    options.HttpsPort = 5001;
                });
            }

            return services;
        }
    }
}
