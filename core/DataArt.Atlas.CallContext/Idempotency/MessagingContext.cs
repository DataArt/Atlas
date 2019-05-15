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

namespace DataArt.Atlas.CallContext.Idempotency
{
    public static class MessagingContext
    {
#if NET452
        private const string MessageIdName = "MessageId";
        private const string IsIdempotentName = "ProvideIdempotency";
        private const string IsConsumedName = "IsConsumed";
#endif

#if NETSTANDARD2_0
        private static readonly AsyncLocal<Guid?> MessageIdStorage = new AsyncLocal<Guid?>();
        private static readonly AsyncLocal<bool?> ProvideIdempotencyMarkerStorage = new AsyncLocal<bool?>();
        private static readonly AsyncLocal<bool> IsConsumedMarkerStorage = new AsyncLocal<bool>();
#endif

        public static Guid? MessageId
        {
            get
            {
#if NET452
                return (Guid?)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(MessageIdName);
#endif

#if NETSTANDARD2_0
                return MessageIdStorage.Value;
#endif
            }

            set
            {
#if NET452
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(MessageIdName, value);
#endif

#if NETSTANDARD2_0
                MessageIdStorage.Value = value;
#endif
            }
        }

        public static bool? ProvideIdempotency
        {
            get
            {
#if NET452
                return (bool?)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(IsIdempotentName);
#endif

#if NETSTANDARD2_0
                return ProvideIdempotencyMarkerStorage.Value;
#endif
            }

            set
            {
#if NET452
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(IsIdempotentName, value);
#endif

#if NETSTANDARD2_0
                ProvideIdempotencyMarkerStorage.Value = value;
#endif
            }
        }

        public static bool IsConsumed
        {
            get
            {
#if NET452
                return (bool?)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(IsConsumedName) ?? false;
#endif

#if NETSTANDARD2_0
                return IsConsumedMarkerStorage.Value;
#endif
            }

            set
            {
#if NET452
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(IsConsumedName, value);
#endif

#if NETSTANDARD2_0
                IsConsumedMarkerStorage.Value = value;
#endif
            }
        }
    }
}
