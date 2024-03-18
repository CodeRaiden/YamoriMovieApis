using YamoriMovieApis.Models.DTO;
using System.Security.Claims;
using YamoriMovieApis.Responses;

namespace YamoriMovieApis.Repositories.Abstract
{
    // token service interface for interfacing with our token service
    public interface ITokenService
    {
        // get token service
        ServiceResponse<TokenResponse> GetToken(IEnumerable<Claim> claim);

        // get refresh token service
        ServiceResponse<string> GetRefreshToken();

        //service to get claims principal from expired token
        ServiceResponse<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    }
}
