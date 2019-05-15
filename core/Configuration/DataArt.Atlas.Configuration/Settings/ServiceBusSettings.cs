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
namespace DataArt.Atlas.Configuration.Settings
{
    public sealed class ServiceBusSettings
    {
        public string QueueName { get; set; }

        #region RabbitMQ transport

        public string RabbitUrl { get; set; }

        public string RabbitUsername { get; set; }

        public string RabbitPassword { get; set; }

        #endregion

        #region Azure Service Bus transport

        public string AzureNamespace { get; set; }

        public string AzurePath { get; set; }

        public string AzureSharedAccessKey { get; set; }

        #endregion
    }
}
