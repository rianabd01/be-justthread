using System;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace be_justthread.Services
{
    public class JwtValidator
    {
        private readonly SupabaseOptions _opts;
    private readonly ConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;

        private readonly bool _isConfigured;

        public JwtValidator(Microsoft.Extensions.Options.IOptions<SupabaseOptions> opts)
        {
            _opts = opts?.Value ?? new SupabaseOptions();
            var supabaseUrl = _opts.Url?.TrimEnd('/');
            if (string.IsNullOrEmpty(supabaseUrl))
            {
                // mark as not configured so DI doesn't throw at startup; Validate() will handle missing config
                _isConfigured = false;
                return;
            }

            var openIdConfigUrl = $"{supabaseUrl}/auth/v1/.well-known/openid-configuration";
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdConfigUrl, new OpenIdConnectConfigurationRetriever());
            _isConfigured = true;
        }

        public ClaimsPrincipal Validate(string token)
        {
            if (!_isConfigured)
                throw new InvalidOperationException("Supabase configuration is missing. Set Supabase:Url and Supabase:Audience in configuration (user-secrets or env vars) before validating tokens.");

            var cfg = _configurationManager!.GetConfigurationAsync().GetAwaiter().GetResult();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _opts.Url!.TrimEnd('/') + "/",
                ValidAudiences = new[] { _opts.Audience },
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => cfg.SigningKeys
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, validationParameters, out _);
        }
    }
}
