public class TimerManager
{
    private Timer? Timer;
    private AutoResetEvent? _autoResetEvent;
    private Action? _action;
    public DateTime TimerStarted { get; set; }
    public bool IsTimerStarted { get; set; }

    public void PrepareTimer(Action action)
    {
        _action = action;
        _autoResetEvent = new AutoResetEvent(false);
        Timer = new Timer(Execute, _autoResetEvent, 1000, 2000);
        TimerStarted = DateTime.Now;
        IsTimerStarted = true;
    }

    public Timer PrepareAndReturnTimer(Action action)
    {
        _action = action;
        _autoResetEvent = new AutoResetEvent(false);
        Timer = new Timer(Execute, _autoResetEvent, 1000, 2000);
        TimerStarted = DateTime.Now;
        IsTimerStarted = true;

        return Timer;
    }

    public void Execute(object? stateInfo)
    {
        _action();
        //if ((DateTime.Now - TimerStarted).TotalSeconds > 20)
        //{
        //    IsTimerStarted = false;
        //    Timer?.Dispose();

        //}
    }

    public void StopTimer()
    {
        Timer?.Dispose();
    }
}