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
using DataArt.Atlas.Core.Correlation;
using DataArt.Atlas.Core.Idempotency;
using DataArt.Atlas.Messaging.Intercept;

namespace DataArt.Atlas.Core.Application.Messaging
{
    internal class ServiceBusInterceptor : IServiceBusInterceptor
    {
        public void OnPublish(IPublishContext context)
        {
            context.SetHeader(nameof(CorrelationContext.CorrelationId), CorrelationContext.CorrelationId.ToString());
        }

        public void OnConsume(IConsumeContext context)
        {
            CorrelationContext.SetCorrelationId(context.GetHeader(nameof(CorrelationContext.CorrelationId)));

            if (!context.MessageId.HasValue)
            {
                throw new InvalidOperationException("Service bus message id should be specified");
            }

            MessagingContext.MessageId = context.MessageId;
        }
    }
}
