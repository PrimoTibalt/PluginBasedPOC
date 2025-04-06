using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdProvider;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "Your role(s)", new [] { "role" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            { };

    public static IEnumerable<Client> Clients =>
        new Client[]
            {
                new Client() {
                    ClientId = "pluginswebapp",
                    ClientSecrets = {new("secret".Sha256())},
                    ClientName = "PluginsPOC",
                    RedirectUris = {
                        @"https://localhost:5000/signin-oidc",
                    },
                    AllowedScopes = { 
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles"
                    },
                    AllowedGrantTypes = GrantTypes.Code,
                    PostLogoutRedirectUris = { "https://localhost:5000/signout-callback-oidc" },
                }
             };
}
