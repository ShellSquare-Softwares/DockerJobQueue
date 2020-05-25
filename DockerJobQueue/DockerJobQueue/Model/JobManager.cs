using JobQueue.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DockerJobQueue.Model
{
    internal class JobManager
    {
        ReaderWriterLockSlim _Slim = new ReaderWriterLockSlim();
        private Dictionary<string, Job> _Jobs = new Dictionary<string, Job>();
        private Dictionary<string, JobInformation> _RunningJobs = new Dictionary<string, JobInformation>();

        public void Add(Job job)
        {
            if (string.IsNullOrWhiteSpace(job.JobId))
            {
                throw new ArgumentException("The job is not set with the JobId. Job id is usually a GUID value.");
            }

            try
            {
                _Slim.EnterUpgradeableReadLock();

                if (_RunningJobs.ContainsKey(job.JobId) || _Jobs.ContainsKey(job.JobId))
                {
                    throw new ArgumentException($"The job with the id {job.JobId} is already exist in the system");
                }

                try
                {
                    _Slim.EnterWriteLock();
                    _Jobs.Add(job.JobId, job);
                }
                finally
                {
                    _Slim.ExitWriteLock();
                }


            }
            finally
            {
                _Slim.ExitUpgradeableReadLock();
            }
        }

        public void Remove(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ArgumentException("The job is not provided. Job id is usually a GUID value.");
            }

            try
            {
                _Slim.EnterUpgradeableReadLock();

                bool jobPresentInRunning = false;
                bool jobPresent = false;
                if (_RunningJobs.ContainsKey(jobId))
                {
                    jobPresentInRunning = true;
                }

                if (jobPresentInRunning == false)
                {
                    if (_Jobs.ContainsKey(jobId))
                    {
                        jobPresent = true;
                    }
                }

                if (jobPresentInRunning == false && jobPresent == false)
                {
                    throw new ArgumentException($"The job with the id {jobId} is not exist in the system");
                }

                try
                {
                    _Slim.EnterWriteLock();
                    if (jobPresentInRunning)
                    {
                        _RunningJobs.Remove(jobId);
                    }

                    if (jobPresent)
                    {
                        _Jobs.Remove(jobId);
                    }
                }
                finally
                {
                    _Slim.ExitWriteLock();
                }
            }
            finally
            {
                _Slim.ExitUpgradeableReadLock();
            }
        }

        public DateTime GetLastChanged(string jobId, string dockerId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ArgumentException("The jobId is not provided. Job id is usually a GUID value.");
            }

            if (string.IsNullOrWhiteSpace(dockerId))
            {
                throw new ArgumentException("The dockerId is not provided.");
            }

            try
            {
                _Slim.EnterReadLock();
                if (!_RunningJobs.TryGetValue(jobId, out JobInformation jobInformation))
                {
                    throw new ArgumentException($"The job with the id {jobId} is not exist in the system");
                }

                if (jobInformation.DockerId != dockerId)
                {
                    throw new AccessViolationException($"The docker id provided is not matching with the docker id of the job {dockerId} = {jobInformation.DockerId}");
                }

                jobInformation.LastAccessed = DateTime.UtcNow;

                return jobInformation.Job.LastChanged;
            }
            finally
            {
                _Slim.ExitReadLock();
            }
        }

        public Job GetJob(string dockerId)
        {
            try
            {
                _Slim.EnterUpgradeableReadLock();
                if (_Jobs.Values.Count > 0)
                {
                    try
                    {
                        _Slim.EnterWriteLock();

                        var job = _Jobs.Values.First();

                        var jobInformation = new JobInformation()
                        {
                            Job = job,
                            DockerId = dockerId,
                            LastAccessed = DateTime.UtcNow
                        };

                        _RunningJobs.Add(job.JobId, jobInformation);

                        _Jobs.Remove(job.JobId);

                        return job;
                    }
                    finally
                    {
                        _Slim.ExitWriteLock();
                    }

                }
            }
            finally
            {
                _Slim.ExitUpgradeableReadLock();
            }

            return null;
        }
    }
}