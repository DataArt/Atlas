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
using System.Threading.Tasks;

namespace DataArt.Atlas.Messaging.Consume
{
#pragma warning disable SA1649 // File name must match first type name
    public interface IConsumer<TBusType, TRoutingType, in TMessageType> : IConsumer
#pragma warning restore SA1649 // File name must match first type name
        where TBusType : class, IConsumerBusType
        where TRoutingType : class, IConsumerRountingType
        where TMessageType : class, new()
    {
        Task Consume(TMessageType message);
    }
}
