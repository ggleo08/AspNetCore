using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BenchmarkServer
{
    public class ConnectionCounterService : IHostedService
    {
        private Stopwatch _stopWatch;
        private readonly ConnectionCounter _counter;
        private ConnectionSummary _lastSummary = new ConnectionSummary();
        private const int _millisecondsDelay = 5000;
        private readonly TelemetryClient _appInsights;
        private readonly HostMetrics _host;
        private readonly Dictionary<string, string> _properties;

        public ConnectionCounterService(ConnectionCounter counter, HostMetrics host, TelemetryClient appInsights)
        {
            _counter = counter;
            _stopWatch = new Stopwatch();
            _appInsights = appInsights;
            _host = host;

            if (_host == null)
                throw new ArgumentNullException(nameof(_host));

            _properties = new Dictionary<string, string>
            {
                {"Id", Guid.NewGuid().ToString() }
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Started ConnectionCounterService");
            while (!cancellationToken.IsCancellationRequested)
            {
                var summary = _counter.Summary;

                if (summary.PeakConnections > 0)
                {
                    if (_stopWatch.ElapsedTicks == 0)
                        _stopWatch.Start();

                    var elapsed = _stopWatch.Elapsed;

                    var metrics = new Dictionary<string, double>() {
                        {"Elapsed", _stopWatch.ElapsedMilliseconds},
                        {nameof(summary.CurrentConnections), summary.CurrentConnections },
                        {nameof(summary.PeakConnections), summary.PeakConnections},
                        {nameof(summary.TotalConnected), summary.TotalConnected},
                        {nameof(summary.TotalDisconnected), summary.TotalDisconnected},
                        {$"Window{nameof(summary.TotalConnected)}", summary.TotalConnected - _lastSummary.TotalConnected },
                        {$"Window{nameof(summary.TotalDisconnected)}", summary.TotalDisconnected - _lastSummary.TotalDisconnected },
                        {$"Window{nameof(summary.CurrentConnections)}", summary.CurrentConnections - _lastSummary.CurrentConnections },
                        {nameof(_host.ProcessCpu), _host.ProcessCpu},
                        {nameof(_host.TotalCpu), _host.TotalCpu }
                    };

                    Console.WriteLine(@"[{0:hh\:mm\:ss}] Current: {1}, peak: {2}, [+: {3}, -: {4}, rate: {5}] cpu proc: {6}%, host: {7}%",
                        elapsed,
                        summary.CurrentConnections,
                        summary.PeakConnections,
                        summary.TotalConnected - _lastSummary.TotalConnected,
                        summary.TotalDisconnected - _lastSummary.TotalDisconnected,
                        summary.CurrentConnections - _lastSummary.CurrentConnections,
                        _host.ProcessCpu,
                        _host.TotalCpu);

                    _lastSummary = summary;

                    _appInsights.TrackEvent("Status", _properties, metrics);
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
