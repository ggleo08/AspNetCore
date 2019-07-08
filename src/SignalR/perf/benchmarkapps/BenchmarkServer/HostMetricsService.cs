using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace BenchmarkServer
{
    public class HostMetricsService : IHostedService
    {
        private const int _millisecondsDelay = 1000;
        private readonly HostMetrics _hostMetrics;

        public HostMetricsService(HostMetrics hostMetrics)
        {
            _hostMetrics = hostMetrics;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Started");
            while (!cancellationToken.IsCancellationRequested)
            {
                _hostMetrics.Update(await GetCpuUsageForProcess(), GetCpuUsageTotal());

                try
                {
                    await Task.Delay(_millisecondsDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        private async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }

        private double GetCpuUsageTotal()
        {
            double totalSize = 0;
            foreach (var aProc in Process.GetProcesses())
                totalSize += aProc.WorkingSet64 / 1024.0;
            return totalSize;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;   
        }
    }
}
