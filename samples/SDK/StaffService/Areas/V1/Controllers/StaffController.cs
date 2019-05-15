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
using System.Web.Http;
using InventoryService.SDK.Equipment;
using InventoryService.SDK.Persons;

namespace StaffService.Areas.V1.Controllers
{
    public class StaffController : ApiController
    {
        private readonly IPersonsClient personsClient;
        private readonly IEquipmentClient equipmentClient;

        public StaffController(IPersonsClient personsClient, IEquipmentClient equipmentClient)
        {
            this.personsClient = personsClient;
            this.equipmentClient = equipmentClient;
        }

        [HttpGet]
        [Route("staff/{personId}")]
        public async Task<object> GetInfoAsync(int personId)
        {
            var person = await personsClient.GetAsync(personId);
            var equipment = await equipmentClient.GetByPersonAsync(personId);

            return new
            {
                Person = person,
                Equipment = equipment
            };
        }
    }
}
