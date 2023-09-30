using Microsoft.AspNetCore.Mvc;

namespace BirdNames.Blazor.Controllers;
[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
  [HttpGet]
  [Route("echo")]
  public IActionResult Get()
  {
    return Ok(DateTime.Now.ToString("F"));
  }
}
