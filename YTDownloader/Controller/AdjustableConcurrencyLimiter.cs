namespace YTDownloader.Controller;

public sealed class AdjustableConcurrencyLimiter
{
    private const int MinLimit = 1;
    private const int MaxLimit = 32;
    private readonly object _lock = new();
    private readonly Queue<Waiter> _waiters = new();
    private int _activeCount;
    private int _limit;

    public AdjustableConcurrencyLimiter(int limit)
    {
        _limit = ClampLimit(limit);
    }

    public int Limit
    {
        get
        {
            lock (_lock)
            {
                return _limit;
            }
        }
    }

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        lock (_lock)
        {
            if (_activeCount < _limit)
            {
                _activeCount++;
                return Task.CompletedTask;
            }

            var waiter = new Waiter(cancellationToken);
            waiter.CancellationRegistration = cancellationToken.Register(() => waiter.Cancel());
            _waiters.Enqueue(waiter);
            return waiter.Task;
        }
    }

    public void Release()
    {
        lock (_lock)
        {
            if (_activeCount > 0)
                _activeCount--;

            GrantAvailableSlots();
        }
    }

    public void UpdateLimit(int limit)
    {
        lock (_lock)
        {
            _limit = ClampLimit(limit);
            GrantAvailableSlots();
        }
    }

    private void GrantAvailableSlots()
    {
        while (_activeCount < _limit && _waiters.Count > 0)
        {
            var waiter = _waiters.Dequeue();
            if (waiter.IsCompleted)
                continue;

            _activeCount++;
            waiter.SetResult();
        }
    }

    private static int ClampLimit(int limit)
    {
        return Math.Clamp(limit, MinLimit, MaxLimit);
    }

    private sealed class Waiter
    {
        private readonly TaskCompletionSource _taskCompletionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Waiter(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }
        public CancellationTokenRegistration CancellationRegistration { get; set; }
        public Task Task => _taskCompletionSource.Task;
        public bool IsCompleted => Task.IsCompleted;

        public void SetResult()
        {
            CancellationRegistration.Dispose();
            _taskCompletionSource.TrySetResult();
        }

        public void Cancel()
        {
            CancellationRegistration.Dispose();
            _taskCompletionSource.TrySetCanceled(CancellationToken);
        }
    }
}
