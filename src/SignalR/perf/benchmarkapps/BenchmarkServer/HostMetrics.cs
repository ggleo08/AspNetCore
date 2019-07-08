namespace BenchmarkServer
{
    public class HostMetrics
    {
        public double ProcessCpu { get; private set; }
        public double TotalCpu { get; private set; }

        private readonly object _lock = new object();

        public void Update(double processCpu, double totalCpu)
        {
            lock (_lock) {
                ProcessCpu = processCpu;
                TotalCpu = totalCpu;
            }
        }
    }
}
