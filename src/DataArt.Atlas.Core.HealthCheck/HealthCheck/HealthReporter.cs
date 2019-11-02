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
using DataArt.Atlas.Hosting.HealthCheck;

namespace DataArt.Atlas.Core.HealthCheck
{
    internal sealed class HealthReporter : IHealthReporter
    {
        private readonly Action<string, HealthState, string> reportHealthStateAction;
        private readonly Action<string, HealthState, TimeSpan, string> reportRecurrentHealthStateAction;

        public HealthReporter(
            Action<string, HealthState, string> reportHealthStateAction,
            Action<string, HealthState, TimeSpan, string> reportRecurrentHealthStateAction)
        {
            this.reportHealthStateAction = reportHealthStateAction;
            this.reportRecurrentHealthStateAction = reportRecurrentHealthStateAction;
        }

        public void ReportHealth(string property, HealthState healthState, string description = null)
        {
            reportHealthStateAction(property, healthState, description);
        }

        public void ReportHealthRecurrent(string property, HealthState healthState, TimeSpan timeToLive, string description = null)
        {
            reportRecurrentHealthStateAction(property, healthState, timeToLive, description);
        }
    }
}
