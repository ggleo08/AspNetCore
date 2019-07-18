using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BenchmarkServer
{
    public class HostedCounterService : IHostedService
    {
        private Stopwatch _stopWatch;
        private readonly ILogger<HostedCounterService> _logger;
        private readonly ConnectionCounter _counter;
        private ConnectionSummary _lastSummary;
        private const int _millisecondsDelay = 1000;

        public HostedCounterService(ILogger<HostedCounterService> logger, ConnectionCounter counter)
        {
            _logger = logger;
            _counter = counter;
            _stopWatch = new Stopwatch();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var summary = _counter.Summary;

                if (summary.PeakConnections > 0)
                {
                    if (_stopWatch.ElapsedTicks == 0)
                        _stopWatch.Start();

                    var elapsed = _stopWatch.Elapsed;

                    if (_lastSummary != null)
                        Console.WriteLine(@"[{0:hh\:mm\:ss}] Current: {1}, peak: {2}, connected: {3}, disconnected: {4}, rate: {5}/s, Received: {6:0.##e+00}",
                            elapsed,
                            summary.CurrentConnections,
                            summary.PeakConnections,
                            summary.TotalConnected - _lastSummary.TotalConnected,
                            summary.TotalDisconnected - _lastSummary.TotalDisconnected,
                            summary.CurrentConnections - _lastSummary.CurrentConnections,
                            summary.ReceivedCount
                            );

                    _lastSummary = summary;
                }

                try
                {
                    await Task.Delay(_millisecondsDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;   
        }
    }
}
