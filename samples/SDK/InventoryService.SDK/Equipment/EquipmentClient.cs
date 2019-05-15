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
using System.Threading.Tasks;
using DataArt.Atlas.Infrastructure.Interfaces;
using DataArt.Atlas.WebCommunication;
using InventoryService.SDK.Equipment.Models;

namespace InventoryService.SDK.Equipment
{
    public class EquipmentClient : IEquipmentClient
    {
        private readonly IRequestFactory requestFactory;
        private const string RoutePrefix = "api/v1";

        public EquipmentClient(Func<ISdkClient, IRequestFactory> requestFactoryFunc)
        {
            requestFactory = requestFactoryFunc(this);
        }

        public Task<EquipmentModel> GetAsync(int equipmentId)
        {
            return requestFactory.GetRequest(RoutePrefix)
                .AddUrlSegment("equipment")
                .AddUrlSegment(equipmentId.ToString())
                .GetAsync<EquipmentModel>();
        }

        public Task<EquipmentModel[]> GetByPersonAsync(int personId)
        {
            return requestFactory.GetRequest(RoutePrefix)
                .AddUrlSegment("persons")
                .AddUrlSegment(personId.ToString())
                .AddUrlSegment("equipment")
                .GetAsync<EquipmentModel[]>();
        }
    }
}
