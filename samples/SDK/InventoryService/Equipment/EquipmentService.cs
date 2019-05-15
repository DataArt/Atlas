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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataArt.Atlas.Infrastructure.Exceptions;
using InventoryService.SDK.Equipment.Models;

namespace InventoryService.Equipment
{
    public class EquipmentService : IEquipmentService
    {
        private readonly List<EquipmentModel> allEquipment = new List<EquipmentModel>
        {
            new EquipmentModel { EquipmentId = 1, PersonId = 1, EquipmentName = "PDA" },
            new EquipmentModel { EquipmentId = 2, PersonId = 1, EquipmentName = "Laptop" },
            new EquipmentModel { EquipmentId = 3, PersonId = 2, EquipmentName = "Table" },
        };

        public async Task<EquipmentModel> GetAsync(int equipmentId)
        {
            return allEquipment.FirstOrDefault(e => e.EquipmentId == equipmentId) ??
                   throw new NotFoundException(equipmentId);
        }

        public async Task<EquipmentModel[]> GetByPersonAsync(int personId)
        {
            return allEquipment.Where(e => e.PersonId == personId).ToArray();
        }
    }
}
