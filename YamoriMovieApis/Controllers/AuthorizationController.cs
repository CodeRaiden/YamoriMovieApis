using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Xml.Linq;
using YamoriMovieApis.Models;
using YamoriMovieApis.Models.Domain;
using YamoriMovieApis.Models.DTO;
using YamoriMovieApis.Repositories.Abstract;

namespace YamoriMovieApis.Controllers
{ 
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly DatabaseContext _cxt;
        //private readonly ILogger _logger;
        // we include our userManager of type ApplicationUser and RoleManager of type IdentityRole
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        public AuthorizationController(DatabaseContext cxt, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            this._cxt = cxt;
            //this._logger = logger;
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordModel model)
        {
            var status = new Status();

            if (!ModelState.IsValid)
            {
                status.StatusCode = 0;
                status.Message = "Please pass in all required fields.";
                //_logger.LogInformation("Please pass in all required fields.");
                return Ok(status);
            }

            // we will get the user from the database
            var user = await _userManager.FindByNameAsync(model.UserName);

            // return a message if the user is not found
            if (user is null)
            {
                status.StatusCode = 0;
                status.Message = "User with the username \" "+ model.UserName + "\" does not exist.";
                //_logger.LogInformation("User not found.");
                return Ok(status);
            }

            //here we will check (with the help of the compare data anotation in the model declaration) to compare if the entered CurrentPassword is the same as the users Password
            if (!await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                status.StatusCode = 0;
                status.Message = "Incorrect password detected.";
                //_logger.LogInformation("Incorrect password detected... Current user password is invalid");
                return Ok(status);
            }

            // we change the user passworrd and store it in a variable "result"
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "Something went wrong... Please try again latter.";
                //_logger.LogInformation("Something went wrong... Please try again latter.");
                return Ok(status);
            }

            status.StatusCode = 1;
            status.Message = "Password was changed successfully!";
            //_logger.LogInformation("Something went wrong... Please try again latter.");
            return Ok(status);

        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginModel model)
        {
            if (model == null)
            {
                //_logger.LogInformation("Fields can not be empty");
                return BadRequest("Fields can not be empty");
            }

            // find the user with the user name in the database
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {// we will get the user roles and store it in a variable
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {// if the user exists then we will create a new claims list for the user consisting of the user name and a unique Guid id generated for the user
                    // and convert this id to a string
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                // then we will add all the userRole claims into the new claims list
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                // then we will generate a token for the user by passing in the auth claims
                var token = _tokenService.GetToken(authClaims);
                // then we will generate refresh token
                var refreshToken = _tokenService.GetRefreshToken();
                // we store the user token info in a variable so we can update it with the authclaims
                var tokenInfo = await _cxt.TokenInfos.FirstOrDefaultAsync(i => i.UserName == user.UserName);

                // create a new tokenInfo on the user if it does not exist
                if(tokenInfo == null)
                {
                    var Info = new TokenInfo
                    {
                        UserName = user.UserName,
                        RefreshToken = refreshToken.Data,
                        RefreshTokenExpiry = DateTime.Now.AddDays(7)
                    };
                    // we add the created tokeninfo to the database
                    _cxt.TokenInfos.Add(Info);
                }
                else
                {
                    tokenInfo.RefreshToken = refreshToken.Data;
                    tokenInfo.RefreshTokenExpiry = DateTime.Now.AddDays(7);

                }
                try{   
                    // save changes
                    _cxt.SaveChanges();
                }
                catch(Exception ex) {
                    //_logger.LogError(ex.Message);
                    return BadRequest(ex.Message);
                }

                return Ok(new LoginResponse
                {
                    Name = user.UserName,
                    Username = user.UserName,
                    Token = token.Data.TokenString,
                    RefreshToken = refreshToken.Data,
                    Expiration = token.Data.ValidTo,
                    StatusCode = 1,
                    Message = "User logged in successfully"
                });

            }

            // if user is not found
            return Ok(
                new LoginResponse
                {
                    StatusCode = 0,
                    Message = "Invalid Username or Password",
                    Token = "",
                    Expiration = null
                }
                );

        }

        [HttpPost]
        public async Task<IActionResult> Registration([FromBody]RegistrationModel model)
        {
            var status = new Status();

            if (!ModelState.IsValid)
            {
                status.StatusCode = 0;
                status.Message = "Please pass in all required fields";
                //_logger.LogInformation("Please pass in all required fields");
                return Ok(status);
            }

            //check if user exists in database
            var userExists = await _userManager.FindByNameAsync(model.Username);

            if(userExists != null)
            {
                status.StatusCode = 0;
                status.Message = "Invalid Username";
                //_logger.LogInformation("Invalid Username");
                return Ok(status);
            }

            // we create the user object
            var user = new ApplicationUser()
            {
                UserName = model.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = model.Email,
                Name = model.Name,
            };

            // create the user entry in the database based on the user object
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "Something went wrong... Try again latter";
                //_logger.LogInformation("Something went wrong... Try again latter");
                return Ok(status);
            }

            //add user role
            //we will create a user role i.e registering the user role as "UserRoles.User" if it does not already exist
            //Note: if we want to register an admin user, we will simply replace the Roles.User with Roles.Admin
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }

            // and we will add the user role (based on the user) to the database if it does exist
            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            status.StatusCode = 1;
            status.Message = "User registered successfully!";
            //_logger.LogInformation("User registered successfully!");
            return Ok(status);
        }

        // We will comment out the RegistrationAdmin code after registering an admin because we only want a single admin registered

        //[HttpPost]
        //public async Task<IActionResult> RegistrationAdmin([FromBody] RegistrationModel model)
        //{
        //    try
        //    {        
        //        var status = new Status();

        //        if (!ModelState.IsValid)
        //        {
        //            status.StatusCode = 0;
        //            status.Message = "Please pass in all required fields";
        //            //_logger.LogInformation("Please pass in all required fields");
        //            return Ok(status);
        //        }

        //        //check if user exists in database
        //        var userExists = await _userManager.FindByNameAsync(model.Username);

        //        if (userExists != null)
        //        {
        //            status.StatusCode = 0;
        //            status.Message = "Invalid Username";
        //            //_logger.LogInformation("Invalid Username");
        //            return Ok(status);
        //        }

        //        // we create the user object
        //        var user = new ApplicationUser()
        //        {
        //            UserName = model.Username,
        //            SecurityStamp = Guid.NewGuid().ToString(),
        //            Email = model.Email,
        //            Name = model.Name,
        //        };

        //        // create the user entry in the database based on the user object
        //        var result = await _userManager.CreateAsync(user, model.Password);

        //        if (!result.Succeeded)
        //        {
        //            status.StatusCode = 0;
        //            status.Message = "Something went wrong... Try again latter";
        //            //_logger.LogInformation("Something went wrong... Try again latter");
        //            return Ok(status);
        //        }

        //        //add user role
        //        //we will create a user role i.e registering the user role as "UserRoles.Admin" if it does not already exist
        //        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        //        }

        //        // and we will add the user role (based on the user) to the database if it does exist
        //        if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
        //        {
        //            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        //        }

        //        status.StatusCode = 1;
        //        status.Message = "User registered successfully!";
        //        //_logger.LogInformation("User registered successfully!");
        //        return Ok(status);

        //    }
        //    catch(Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}



    }
}
