using System.Collections.Concurrent;
using AttendAI.Application.Biometrics;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Biometrics;

public sealed class InMemoryBiometricEnrollmentRateLimiter : IBiometricEnrollmentRateLimiter
{
    private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> _attempts = new(StringComparer.Ordinal);
    private readonly BiometricEnrollmentOptions _options;

    public InMemoryBiometricEnrollmentRateLimiter(IOptions<BiometricEnrollmentOptions> options)
    {
        _options = options.Value;
    }

    public bool TryAcquire(string userId, DateTimeOffset nowUtc)
    {
        var permitLimit = Math.Max(1, _options.EnrollmentRateLimitPermitCount);
        var window = TimeSpan.FromMinutes(Math.Max(1, _options.EnrollmentRateLimitWindowMinutes));
        var queue = _attempts.GetOrAdd(userId, _ => new Queue<DateTimeOffset>());

        lock (queue)
        {
            while (queue.Count > 0 && nowUtc - queue.Peek() > window)
            {
                queue.Dequeue();
            }

            if (queue.Count >= permitLimit)
            {
                return false;
            }

            queue.Enqueue(nowUtc);
            return true;
        }
    }
}
