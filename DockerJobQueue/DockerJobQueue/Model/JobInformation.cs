using JobQueue.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DockerJobQueue.Model
{
    internal class JobInformation
    {
        public Job Job { get; set; }
        public string DockerId { get; set; }
        public DateTime? LastAccessed { get; set; }
    }
}
