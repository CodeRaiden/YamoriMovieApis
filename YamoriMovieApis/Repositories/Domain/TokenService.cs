using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using YamoriMovieApis.Models.DTO;
using YamoriMovieApis.Repositories.Abstract;
using YamoriMovieApis.Responses;

namespace YamoriMovieApis.Repositories.Domain
{
    //ITokenService Interface Implementation Class
    public class TokenService : ITokenService
    {
        // appsettings/Iconfiguration field
        private readonly IConfiguration _configuration;

        //dependency injection
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // service to generate principal
        public ServiceResponse<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
        {
            var response = new ServiceResponse<ClaimsPrincipal>();

            // here since we want to make the token inactive when the expiry data is exceeded, then we will set the principal claims to invalid to expire the token 
            var tokenValidationParameters = new TokenValidationParameters
            {
                // here we remove the validation from the token to set the token as invalid
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false,
            };

            // here we will get the principal from the expired token
            // here we will create a variable to hold the tokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();
            // then we will create a variable of type SecurityToken to hold the securityToken we will create soon
            SecurityToken securityToken;
            // then we will validate the token using the jwtsecuritytokenhandler object "tokenandler" method "ValidateToken" which would take in the "token" and "tokenValidationParameter" in it's parameters, and output the result in the "securityToken" variable and set it as the principal
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            // then we will store the securityToken as a JwtSecurirtyToken object
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            // then we will check if the jwtSecurityToken == null or the jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase) is not true
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            response = response.Successful("Principal created");
            response.Data = principal;
            return response;
        }

        // service to generate the refresh token
        public ServiceResponse<string> GetRefreshToken()
        {
            var response = new ServiceResponse<string>();
            // here we will generate the refresh token
            // first we will generate a random of byte 32
            var randomNumber = new byte[32];
            
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                var generatedRSN = Convert.ToBase64String(randomNumber);
                if (generatedRSN == null)
                {
                    response = response.Failure("Failed to generate random number");
                    return response;
                }

                response = response.Successful("Random number generated successfully");
                response.Data = generatedRSN;
                return response;
            }

        }

        // Service to generate token
        public ServiceResponse<TokenResponse> GetToken(IEnumerable<Claim> claim)
        {
            // before we begin, create a serviceResponse type var "response"
            var response = new ServiceResponse<TokenResponse>();

            // firstly we create the variable to hold the authorization signin key
            var authSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            // we generate the token
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(7),
                claims: claim,
                // we will hash the authSignInKey and set it as the signingCredentials
                signingCredentials: new SigningCredentials(authSignInKey, SecurityAlgorithms.HmacSha256)
                );

            // we create the token string
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // then we create a token response
            var tokenResponse = new TokenResponse()
            {
                TokenString = tokenString,
                ValidTo = token.ValidTo,
            };

            if (tokenResponse.TokenString == null)
            {
                response = response.Failure("Failed to create token");
            }
            else
            {
                response.Data = tokenResponse;
                response = response.Successful("Token created");
            }

            return response;
        }
    }
}
