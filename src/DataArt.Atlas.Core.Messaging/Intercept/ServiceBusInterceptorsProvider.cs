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
using System.Collections.Generic;
using System.Linq;

namespace DataArt.Atlas.Messaging.Intercept
{
    public static class ServiceBusInterceptorsProvider
    {
        private static readonly object Sync = new object();
        private static IServiceBusInterceptor[] interceptors = new IServiceBusInterceptor[0];

        public static IEnumerable<IServiceBusInterceptor> Get()
        {
            return interceptors;
        }

        public static void Add(IServiceBusInterceptor interceptor)
        {
            lock (Sync)
            {
                interceptors = interceptors.Concat(new[] { interceptor }).ToArray();
            }
        }
    }
}
