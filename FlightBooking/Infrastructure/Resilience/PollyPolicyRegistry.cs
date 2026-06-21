

using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

namespace FlightBooking.Web.Infrastructure.Resilience
{
    public static class PollyPolicyRegistry
    {
        public static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"[Resilience Warning] Transient failure encountered. Retry effort: {retryCount}. Delaying for: {timespan.TotalSeconds}s.");
            });
        }

        // 2. Circuit Breaker Policy: Breaks the circuit for 30 seconds if 2 consecutive failures occur
        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        Console.WriteLine($"[CRITICAL] Circuit tripped into OPEN state for {breakDelay.TotalSeconds}s due to consecutive dependency failures.");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("[Resilience Info] Circuit closed successfully. Traffic restored to normal flow.");
                    });
        }
    }
}