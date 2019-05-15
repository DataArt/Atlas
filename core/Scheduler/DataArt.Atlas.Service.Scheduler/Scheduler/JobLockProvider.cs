#region License
// =================================================================================================
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =================================================================================================
#endregion
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace DataArt.Atlas.Service.Scheduler.Scheduler
{
    internal class JobLockProvider : IDisposable
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public async Task<JobLock> AcquireAsync(JobKey jobKey)
        {
            var semaphore = semaphores.GetOrAdd(jobKey.ToString(), key => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            return new JobLock(semaphore);
        }

        public void Dispose()
        {
            if (semaphores == null)
            {
                return;
            }

            var keys = semaphores.Keys.ToList();

            foreach (var key in keys)
            {
                if (semaphores.TryRemove(key, out var sempahore))
                {
                    sempahore.Dispose();
                }
            }
        }
    }
}
