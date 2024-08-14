using eBayExtension.Contracts;
using eBayExtension.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace eBayExtension.Hubs
{
    public class EbayApiDataHub : Hub
    {
        // Manage state about 
        private readonly IEbayDataService _ebayData;
        private ConcurrentDictionary<string, Filter> _clientData;
        private readonly Channel<ConnectionWithFilter> _channel;
        private readonly ILogger<EbayApiDataHub> _logger;
        public EbayApiDataHub(IEbayDataService ebayData, Channel<ConnectionWithFilter> channel, ILogger<EbayApiDataHub> logger)
        {
            _ebayData = ebayData;
            _clientData = new ConcurrentDictionary<string, Filter>();
            _channel = channel;
            _logger = logger;
        }
        public override  Task OnConnectedAsync()
        {
            // incoming connection
             _channel.Writer.TryWrite(new ConnectionWithFilter { ConnectionId = Context.ConnectionId, Filters = new List<Filter>() ,Command = Utils.FilterCommands.OnConnectionStart});

            return base.OnConnectedAsync();

        }

        public async Task Filters(List<Filter> filter)
        {
            await _channel.Writer.WriteAsync(new ConnectionWithFilter { ConnectionId = Context.ConnectionId, Filters = filter ,Command = Utils.FilterCommands.AddMultipleFilter});

        }     
        public async Task Filter(List<Filter> filter)
        {
            await _channel.Writer.WriteAsync(new ConnectionWithFilter { ConnectionId = Context.ConnectionId, Filters = filter ,Command = Utils.FilterCommands.AddSingleFilter});
            
        }
        public async Task RemoveFilter(List<Filter> filter)
        {
            await _channel.Writer.WriteAsync(new ConnectionWithFilter { ConnectionId = Context.ConnectionId, Filters = filter ,Command = Utils.FilterCommands.RemoveFilter});
            
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _channel.Writer.TryWrite(new ConnectionWithFilter { ConnectionId = Context.ConnectionId, Filters = new List<Filter>() ,Command = Utils.FilterCommands.OnConnectionEnd});
            return base.OnDisconnectedAsync(exception);
        }
    }
}
    