using System;
using System.Collections.Generic;
using System.Text;
using OpenIddict.EntityFrameworkCore.Models;


namespace IdentityProvider.DataAccess.Models
{
    public class IdentityOpenIddictApplication : OpenIddictEntityFrameworkCoreApplication<Guid, IdentityOpenIddictAuthorization, IdentityOpenIddictToken>
    {
        public bool IsReserved { get; set; }
        public string Resources { get; set; }
    }
    public class IdentityOpenIddictAuthorization : OpenIddictEntityFrameworkCoreAuthorization<Guid, IdentityOpenIddictApplication, IdentityOpenIddictToken> { }
    public class IdentityOpenIddictScope : OpenIddictEntityFrameworkCoreScope<Guid> { }
    public class IdentityOpenIddictToken : OpenIddictEntityFrameworkCoreToken<Guid, IdentityOpenIddictApplication, IdentityOpenIddictAuthorization> 
    {
        public new bool Status { get; set; }
        public bool IsSignOut { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsUserToken { get; set; }
    }
}
