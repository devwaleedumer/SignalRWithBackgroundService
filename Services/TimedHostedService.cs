using eBayExtension.Contracts;
using eBayExtension.Models;
using eBayExtension.Utils;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace eBayExtension.Services
{
    public class TimedHostedService : BackgroundService
    {
        private readonly ILogger<TimedHostedService> _logger;
        private TimerManager? _timer;
        private object _listLock;
        private readonly Channel<ConnectionWithFilter> _addFilterChannel;
        private readonly IEbayDataService _ebayDataService;
        private readonly SendDataService _sendDataService;
        private readonly ConcurrentDictionary<string, List<FilterTimer>> _timerWithAppliedFilters;
        public TimedHostedService(ILogger<TimedHostedService> logger, TimerManager? timer, Channel<ConnectionWithFilter> channel, IEbayDataService ebayDataService, SendDataService sendDataService)
        {
            _logger = logger;
            _timer = timer;
            _addFilterChannel = channel;
            _ebayDataService = ebayDataService;
            _sendDataService = sendDataService;
            _timerWithAppliedFilters = new();
            _listLock = new();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isAddFilterReaderDataAvailable = await _addFilterChannel.Reader.WaitToReadAsync();
            while (isAddFilterReaderDataAvailable && !stoppingToken.IsCancellationRequested)
            {
                var result = await _addFilterChannel.Reader.ReadAsync().ConfigureAwait(false);
                // New instance will be provided
                switch (result.Command)
                {
                    // When New Connection is Connected
                    case FilterCommands.OnConnectionStart:
                        // initialize the connection
                        _timerWithAppliedFilters.TryAdd(result.ConnectionId, new List<FilterTimer>());
                        break;
                    // When  there are Already Multiple filters present but Due to Some reason
                    // i.e connection went away and was restored then the whole array is sent to server
                    case FilterCommands.AddMultipleFilter:
                        lock (_listLock)
                        {
                            if (result.Filters!.Count > 0)
                            {
                                _timerWithAppliedFilters.TryGetValue(result.ConnectionId, out List<FilterTimer>? filterTimerss);

                                if (filterTimerss is null)
                                {
                                    filterTimerss = new();
                                }
                                foreach (var filter in result.Filters)
                                {
                                    filterTimerss.Add(new FilterTimer(_timer!.PrepareAndReturnTimer(async () =>
                                                                    {
                                                                        var responseData = await _ebayDataService.GetPostbyFilter(filter);
                                                                        await _sendDataService.SendSampleDataByConnectionId(result.ConnectionId, responseData);
                                                                    }), filter));
                                }   
                            }
                        }
                        break;
                    // one item will be received for removal 
                    // we will update filter list
                    case FilterCommands.RemoveFilter:
                        if (_timerWithAppliedFilters.TryGetValue(result.ConnectionId, out List<FilterTimer>? filterTimers))
                        {
                            var receivedFilterToBeRemoved = result.Filters!.FirstOrDefault();
                            if (receivedFilterToBeRemoved != null)
                            {
                                lock (_listLock)
                                {
                                    var filter = filterTimers.FirstOrDefault((f) => f.Filter.Name == receivedFilterToBeRemoved.Name);
                                    if (filter != null)
                                    {
                                        // Stop the timer
                                        filter.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                                        // Release the acquired resourses
                                        filter.Timer.Dispose();
                                        filterTimers.Remove(filter);
                                    }
                                    _timerWithAppliedFilters.TryUpdate(result.ConnectionId, filterTimers, _timerWithAppliedFilters[result.ConnectionId]);
                                }
                            }
                        }
                        break;
                    case FilterCommands.AddSingleFilter:
                        if (_timerWithAppliedFilters.TryGetValue(result.ConnectionId, out List<FilterTimer>? filterTimer))
                        {
                            //Get Value
                            var receivedFilterToBeAdded = result.Filters!.FirstOrDefault();
                            if (receivedFilterToBeAdded is not null)
                            {
                                lock (_listLock)
                                {
                                    filterTimer.Add(new FilterTimer( new Timer(async (state) =>    {  var responseData = await _ebayDataService.GetPostbyFilter(receivedFilterToBeAdded);
                                                                                                        await _sendDataService.SendSampleDataByConnectionId(result.ConnectionId, responseData);
                                                                                                        },null,0,2000), receivedFilterToBeAdded));
                                }
                                _timerWithAppliedFilters.TryUpdate(result.ConnectionId, filterTimer, _timerWithAppliedFilters[result.ConnectionId]);
                            }
                        }
                        break;
                    case FilterCommands.OnConnectionEnd:
                        _timerWithAppliedFilters.TryRemove(result.ConnectionId, out _);
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
