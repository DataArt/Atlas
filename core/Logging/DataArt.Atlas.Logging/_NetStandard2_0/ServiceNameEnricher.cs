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
using Serilog.Core;
using Serilog.Events;

namespace DataArt.Atlas.Logging
{
    public class ServiceNameEnricher : ILogEventEnricher
    {
        private readonly string serviceType;
        private LogEventProperty cachedProperty;

        public ServiceNameEnricher(string serviceType)
        {
            this.serviceType = serviceType;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (serviceType != null)
            {
                cachedProperty = cachedProperty ?? propertyFactory.CreateProperty("ServiceName", serviceType);
                logEvent.AddPropertyIfAbsent(cachedProperty);
            }
        }
    }
}
#endif