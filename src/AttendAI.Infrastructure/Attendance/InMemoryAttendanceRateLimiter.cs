using System.Collections.Concurrent;
using AttendAI.Application.Attendance;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Attendance;

public sealed class InMemoryAttendanceRateLimiter : IAttendanceRateLimiter
{
    private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> _attempts = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastAttempt = new(StringComparer.Ordinal);
    private readonly AttendanceOptions _options;

    public InMemoryAttendanceRateLimiter(IOptions<AttendanceOptions> options)
    {
        _options = options.Value;
    }

    public bool TryAcquire(string userId, DateTimeOffset nowUtc)
    {
        var permitLimit = Math.Max(1, _options.MaximumAttemptsPerWindow);
        var window = TimeSpan.FromMinutes(Math.Max(1, _options.AttemptWindowMinutes));
        var cooldown = TimeSpan.FromSeconds(Math.Max(0, _options.CooldownSeconds));
        var queue = _attempts.GetOrAdd(userId, _ => new Queue<DateTimeOffset>());

        lock (queue)
        {
            if (_lastAttempt.TryGetValue(userId, out var lastAttempt) && nowUtc - lastAttempt < cooldown)
            {
                return false;
            }

            while (queue.Count > 0 && nowUtc - queue.Peek() > window)
            {
                queue.Dequeue();
            }

            if (queue.Count >= permitLimit)
            {
                return false;
            }

            queue.Enqueue(nowUtc);
            _lastAttempt[userId] = nowUtc;
            return true;
        }
    }
}
