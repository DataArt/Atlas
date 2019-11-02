//--------------------------------------------------------------------------------------------------
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
//--------------------------------------------------------------------------------------------------
using System;
using System.Threading;
using DataArt.Atlas.CallContext.Correlation;
using DataArt.Atlas.Hosting.HealthCheck;
using DataArt.Atlas.Infrastructure.Startup;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Core.HealthCheck
{
    public abstract class RecurrentHealthCheck : Startable, IDisposable
    {
#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<RecurrentHealthCheck>();
#endif
        private readonly TimeSpan recurrenceInterval;
        private readonly IHealthReporter healthReporter;

        private readonly TimeSpan timeToLivePadding = TimeSpan.FromSeconds(30);

        private Timer timer;

        public void Dispose()
        {
            timer?.Dispose();
        }

        protected RecurrentHealthCheck(TimeSpan recurrenceInterval, IHealthReporter healthReporter)
        {
            this.recurrenceInterval = recurrenceInterval;
            this.healthReporter = healthReporter;
        }

        protected abstract string Property { get; }

        protected abstract HealthState HealthCheck();

        protected override void StartInternal(CancellationToken cancellationToken)
        {
            timer = new Timer(state => HealthCheckInternal(), null, TimeSpan.Zero, recurrenceInterval);
        }

        private void HealthCheckInternal()
        {
            using (new CorrelatedSequence())
            {
                var healthState = HealthState.Error;

                try
                {
                    healthState = HealthCheck();
                }
                catch (Exception e)
                {
#if NET452
                    Log.Error(e, "Recurrent {HealthCheckName} error", Name);
#endif

#if NETSTANDARD2_0
                    logger.LogError(e, "Recurrent {HealthCheckName} error", Name);
#endif
                }

                healthReporter.ReportHealthRecurrent(Property, healthState, recurrenceInterval + timeToLivePadding);
            }
        }
    }
}
