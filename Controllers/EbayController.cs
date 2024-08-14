using eBayExtension.Contracts;
using eBayExtension.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace eBayExtension.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EbayController : ControllerBase
    {
        private readonly IHubContext<EbayApiDataHub> _hub;
        private readonly TimerManager _timer;
        private readonly IEbayDataService _ebayData;
        private readonly ILogger<EbayController> _logger;

        public EbayController(IHubContext<EbayApiDataHub> hub, TimerManager timer, IEbayDataService ebayData, ILogger<EbayController> logger)
        {
            _hub = hub;
            _timer = timer;
            _ebayData = ebayData;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!_timer.IsTimerStarted)
                _timer.PrepareTimer(async () => {
                    await _hub.Clients.All.SendAsync("ReceiveMessage", await _ebayData.GetPostData());
                    //_logger.LogInformation($"Request Completed at {DateTime.Now.ToShortTimeString()}");
                });
            return Ok(new { Message = "Request Completed" });
        }
    }
}
