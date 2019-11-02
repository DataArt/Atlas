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
using DataArt.Atlas.Messaging.Intercept;
using MassTransit;

namespace DataArt.Atlas.Messaging.MassTransit.Intercept
{
    internal class PublishContext : IPublishContext
    {
        private readonly SendContext sendContext;

        public PublishContext(SendContext sendContext)
        {
            this.sendContext = sendContext;
        }

        public void SetHeader(string key, string value)
        {
            sendContext.Headers.Set(key, value);
        }
    }
}
