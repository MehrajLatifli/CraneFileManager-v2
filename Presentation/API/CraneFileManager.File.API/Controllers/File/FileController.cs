using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneFileManager.File.API.Controllers.File
{
    [ApiVersion(1, Deprecated = false)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpGet]
        [Route("check")]
        [Produces("application/json")]
        public IActionResult GetCheck()
        {
            // You can perform any necessary checks here (e.g., database connection).
            // For simplicity, we will just return a 200 OK response.

            return Ok(new { status = "Healthy" });
        }
    }
}
