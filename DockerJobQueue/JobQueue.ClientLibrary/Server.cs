using JobQueue.Common;
using System;

namespace JobQueue.ClientLibrary
{
    public class Server
    {
        public Action<Job> JobReceived;
        public Action JobStopped;
        public Action<Job> JobUpdated;
        private string _Url;
        public Server(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("The URL is required for connecting with the JobQue service Th url format is https://localhost/JobServer.");
            }

            _Url = url;
        }
        private void HeartBeats()
        {

        }
    }
}
