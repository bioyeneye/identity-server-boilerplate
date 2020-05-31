using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityProvider.Domain.Services
{
    public class ApplicationManager : OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>
    {
        public ApplicationManager(IOpenIddictApplicationCache<OpenIddictEntityFrameworkCoreApplication<Guid>> cache, IOpenIddictApplicationStoreResolver resolver, ILogger<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>> logger, IOptionsMonitor<OpenIddictCoreOptions> options) : base(cache, resolver, logger, options)
        {
        }

        public override ValueTask<OpenIddictEntityFrameworkCoreApplication<Guid>> CreateAsync(OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            return base.CreateAsync(descriptor, cancellationToken);
        }
    }
}
