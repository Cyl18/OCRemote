using System.Collections.Concurrent;
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
            var time = DateTime.Now;
            while (DateTime.Now - time < TimeSpan.FromSeconds(5))
            {
                CancellationToken cancellationToken = HttpContext.RequestAborted;
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (RemoteManager.commandQueue.TryDequeue(out var x))
                {
                    return Content(x);
                }

                SpinWait.SpinUntil(() => RemoteManager.commandQueue.Count > 0, 5);
            }

            return NoContent();
        }
    }
}
