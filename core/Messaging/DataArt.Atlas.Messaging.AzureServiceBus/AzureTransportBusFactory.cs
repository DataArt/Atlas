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
using System.Collections.Generic;
using System.Linq;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Messaging.MassTransit.Intercept;
using GreenPipes.Specifications;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.ServiceBus;
using Serilog;

namespace DataArt.Atlas.Messaging.AzureServiceBus
{
    internal static class AzureTransportBusFactory
    {
        public static IBusControl Create(
            ServiceBusSettings settings,
            Action<IReceiveEndpointConfigurator> receiveEndpointConfigurator,
            Action<IReceiveEndpointConfigurator> fanoutReceiveEndpointConfigurator)
        {
            if (fanoutReceiveEndpointConfigurator != null)
            {
                DeleteAbandonedSubscriptions(settings);
            }

            return Bus.Factory.CreateUsingAzureServiceBus(x =>
            {
                x.UseSerilog();

                var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", settings.AzureNamespace, GetPath(settings));

                var host = x.Host(serviceUri, h =>
                {
                    h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", settings.AzureSharedAccessKey, TokenScope.Namespace);
                });

                if (receiveEndpointConfigurator != null)
                {
                    x.ReceiveEndpoint(settings.QueueName, receiveEndpointConfigurator);
                }

                if (fanoutReceiveEndpointConfigurator != null)
                {
                    x.ReceiveEndpoint(host, fanoutReceiveEndpointConfigurator);
                }

                x.ConfigurePublish(cfg =>
                    cfg.AddPipeSpecification(
                        new DelegatePipeSpecification<SendContext>(sendContext => sendContext.ApplyInterceptors())));
            });
        }

        private static string GetPath(ServiceBusSettings settings)
        {
            return string.IsNullOrWhiteSpace(settings.AzurePath) ? string.Empty : settings.AzurePath;
        }

        /// <summary>
        /// Method is used for abandoned subscriptions deleting (subscriptions to temporaries queues on fanout topics).
        /// Should be fixed in new Masstransit releases.
        /// </summary>
        /// <param name="settings">Service bus settings.</param>
        private static void DeleteAbandonedSubscriptions(ServiceBusSettings settings)
        {
            var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", settings.AzureNamespace, string.Empty);
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", settings.AzureSharedAccessKey, TokenScope.Namespace);
            var namespaceManager = new NamespaceManager(serviceUri, tokenProvider);

            var queues = namespaceManager.GetQueues().Select(q => $"{namespaceManager.Address.AbsoluteUri.ToLower()}{q.Path.ToLower()}").ToList();

            var toExecute = new List<Action>();

            foreach (var topic in namespaceManager.GetTopics().Where(t => t.SubscriptionCount > 0))
            {
                foreach (var subscription in namespaceManager.GetSubscriptions(topic.Path).Where(s => !string.IsNullOrEmpty(s.ForwardTo)))
                {
                    if (!queues.Contains(subscription.ForwardTo.ToLower()))
                    {
                        toExecute.Add(() =>
                        {
                            namespaceManager.DeleteSubscription(topic.Path, subscription.Name);
                            Log.Information(
                                "Abandoned subscription removed: {topic} -> {subscription} -> {queue}",
                                topic.Path,
                                subscription.Name,
                                subscription.ForwardTo);
                        });
                    }

                    Log.Debug("Found abandoned subscription: {topic} -> {subscription} -> {queue}", topic.Path, subscription.Name, subscription.ForwardTo);
                }
            }

            foreach (var execute in toExecute)
            {
                execute();
            }
        }
    }
}