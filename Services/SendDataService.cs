using eBayExtension.Hubs;
using eBayExtension.Models;
using Microsoft.AspNetCore.SignalR;

namespace eBayExtension.Services
{
    public class SendDataService
    {
        private readonly IHubContext<EbayApiDataHub> _ebayHub;
        public SendDataService(IHubContext<EbayApiDataHub> ebayHub)
        {
            _ebayHub = ebayHub;
        }

        public async Task SendSampleDataByConnectionId(string connectionID,IEnumerable<Post> post)
        {
            await  _ebayHub.Clients.Client(connectionID).SendAsync("filter", post);
        }


    }
}
