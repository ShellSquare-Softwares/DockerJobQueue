using System;

namespace JobQueue.Common
{
    public class Job
    {
        public string JobId { get; set; }
        public string Configuration { get; set; }
        public DateTime LastChanged { get; set; }
    }
}
