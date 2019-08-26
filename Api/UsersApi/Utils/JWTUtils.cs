using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DataAccess;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace UsersApi.Utils
{
    public class JWTUtils
    {
        private const string ISSUER = "UADE SSO";
        private const string EXTRA_CLAIMS = "extra_claims";

        public static string CreateJWT(Tenant tenant, User user)
        {
            var signingKey = Convert.FromBase64String(tenant.JwtSigningKey);
            var expiryDuration = tenant.JwtDuration;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = ISSUER,
                Audience = tenant.Id.ToString(),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature)
            };

            if (expiryDuration > 0)
            {
                tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(expiryDuration);
            }

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            if (user.ExtraClaims != null)
            {
                jwtToken.Payload.Add(EXTRA_CLAIMS, JsonConvert.DeserializeObject<dynamic>(user.ExtraClaims));
            }

            var token = jwtTokenHandler.WriteToken(jwtToken);

            return token;
        }
    }
}