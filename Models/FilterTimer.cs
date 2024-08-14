namespace eBayExtension.Models;

public class FilterTimer
{
    public Timer Timer { get; set; }
    public Filter Filter { get; set; }
    public FilterTimer(Timer timer, Filter filter)
    {
        Timer = timer;
        Filter = filter;
    }
}
