using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace YamoriMovieApis.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    // this endpoint will require authorization which will only be accessible by an Admin
    [Authorize(Roles="Admin")]
    public class AdminController : ControllerBase
    {
        public IActionResult GetData()
        {
            return Ok("Data from admin controller");
        }
    }
}
