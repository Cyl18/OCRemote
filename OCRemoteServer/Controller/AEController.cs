using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OCRemoteServer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AEController : ControllerBase
    {
        [HttpGet("{name}")]
        public async Task<IActionResult> GetImage(string name)
        {
            var path = $@"C:\Users\cyl18\Documents\Persistance\MultiMC\instances\GT_New_Horizons_2.4.1_Java_17-20\.minecraft\dumps\itempanel_icons\{name}.png";
            if (!System.IO.File.Exists(path))
            {
                return File(
                    await System.IO.File.ReadAllBytesAsync(
                        path.Replace(name, "MissingNo")),
                    "image/png");
            }
            return File(
                await System.IO.File.ReadAllBytesAsync(
                    path),
                "image/png");
        } 

    }
}
