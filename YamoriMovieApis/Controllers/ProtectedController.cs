using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace YamoriMovieApis.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    // this endpoint will require an authorized user to gain access
    [Authorize]
    public class ProtectedController : ControllerBase
    {
        public IActionResult GetData()
        {
            return Ok("Data from protected controller");
        }
    }
}
