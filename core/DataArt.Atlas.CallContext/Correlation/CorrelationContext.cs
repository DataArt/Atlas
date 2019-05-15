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

#if NETSTANDARD2_0
using System.Threading;
#endif

namespace DataArt.Atlas.CallContext.Correlation
{
    public static class CorrelationContext
    {
        public const string CorrelationIdName = "CorrelationId";

#if NETSTANDARD2_0
        private static readonly AsyncLocal<Guid> CorrelationIdStorage = new AsyncLocal<Guid>();
#endif

        public static Guid CorrelationId
        {
            get
            {
#if NET452
                return (Guid)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(CorrelationIdName);
#endif

#if NETSTANDARD2_0
                return CorrelationIdStorage.Value;
#endif
            }

            set
            {
#if NET452
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(CorrelationIdName, value);
#endif

#if NETSTANDARD2_0
                CorrelationIdStorage.Value = value;
#endif

            }
        }

        public static void SetCorrelationId(string value = null)
        {
            CorrelationId = value != null ? Guid.Parse(value) : Guid.NewGuid();
        }
    }
}
