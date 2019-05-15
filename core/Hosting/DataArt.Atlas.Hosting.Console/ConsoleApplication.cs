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
#if NETSTANDARD2_0
using System;
using System.Reflection;
using DataArt.Atlas.Hosting.HealthCheck;

namespace DataArt.Atlas.Hosting.Console
{
    internal class ConsoleApplication : IApplication
    {
        public Action<string, HealthState, string> ReportHealthStateAction => (property, state, description) => { };

        public Action<string, HealthState, TimeSpan, string> ReportRecurrentHealthStateAction => (property, state, timeToLive, description) => { };

        public Func<string> GetApplicationVersionFunction => () => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public Func<string> GetServiceVersionFunction => () => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public Func<string> GetCodePackageVersionFunction => () => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public Func<string, string> GetDataPackageVersionFunction => packageName => $"{packageName}.{DateTime.Now.Ticks}";

        public Func<string, string> GetServiceResourcePathFunction => resourceName => string.Empty;
    }
}
#endif
