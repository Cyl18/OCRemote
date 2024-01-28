using System.Collections.Concurrent;
using System.Numerics;
using System.Text.Json;
using GammaLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace OCRemoteServer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OCReportController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Report()
        {
            using var s = HttpContext.Request.Body;
            using var sr = new StreamReader(s);

            RemoteManager.Callback(await sr.ReadToEndAsync());
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetEvent()
        {
            Dictionary<int, string> list = new();
            while (RemoteManager.commandQueue.TryDequeue(out var x))
            {
                list.Add(x.Item1, x.Item2);
            }
            if (list.IsEmpty())
            {
                return NoContent();
            }
            var json = JsonSerializer.Serialize(list);
            return Content(json);
        }
    }
}
