using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YamoriMovieApis.Models.Domain;
using YamoriMovieApis.Models.DTO;
using YamoriMovieApis.Repositories.Abstract;

namespace YamoriMovieApis.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly DatabaseContext _ctx;
        private readonly ITokenService _service;
        //private readonly ILogger _logger;

        // Dependency injection
        public TokenController(DatabaseContext ctx, ITokenService service)
        {
            this._ctx = ctx;
            this._service = service;
            //this._logger = logger;
        }

        [HttpPost]
        public IActionResult Refresh(RefreshTokenRequest tokenApiModel)
        {
            //_logger.LogInformation("Executing Request....");

            if(tokenApiModel is null)
            {
                return BadRequest("Bad client request");
            }

            var accessToken = tokenApiModel.AccessToken;
            var refreshToken = tokenApiModel.RefreshToken;
            // we generate and store the principal for the entered token
            var principal = _service.GetPrincipalFromExpiredToken(accessToken);
            // we get the username of the token and store this in a variable username
            var username = principal.Data.Identity.Name;
            // then we query the database using LINQ, to get the user from the TokenInfos table
            var user = _ctx.TokenInfos.FirstOrDefault(u => u.UserName == username);

            if(user == null || user.RefreshToken != tokenApiModel.RefreshToken || user.RefreshTokenExpiry <= DateTime.Now)
            {
                //_logger.LogError("Request failed");
                return BadRequest("Invalid client request");
            }

            // we generate a new accesstoken and refreshToken
            var newAccessToken = _service.GetToken(principal.Data.Claims);
            var newRefreshToken = _service.GetRefreshToken();
            // then we assign them to the user
            user.RefreshToken = newRefreshToken.Data;
            _ctx.SaveChanges();
            //_logger.LogInformation("Request completed successfully");
            return Ok(new RefreshTokenRequest
            {
                AccessToken = newAccessToken.Data.TokenString,
                RefreshToken = newRefreshToken.Data
            });
             
        }
    // Revoke Token to delete a token from the database
        [HttpPost,Authorize]
        public IActionResult Revoke()
        {
            try
            {
                // first we fetch the user using the ClaimsPrincipal.ControllBase.User.Identity.Name
                var username = User.Identity.Name;
                var user = _ctx.TokenInfos.SingleOrDefault(u => u.UserName == username);
                if(user == null)
                {
                    return BadRequest("User not found");
                }
                // after getting the user from the database we will set the user's refreshtoken to null inorder to revoke it
                user.RefreshToken = null;
                _ctx.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            } 

        }
    }

    
}
