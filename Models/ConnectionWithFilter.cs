using eBayExtension.Utils;

namespace eBayExtension.Models
{
    public class ConnectionWithFilter
    {
        public ConnectionWithFilter()
        {
            Filters = new();
        }
        public required string ConnectionId { get; set; }
        public List<Filter>? Filters { get; set; }
        public FilterCommands Command { get; set; }
    }
}
