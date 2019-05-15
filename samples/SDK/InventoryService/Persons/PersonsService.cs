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
using System.Linq;
using System.Collections.Generic;
using DataArt.Atlas.Infrastructure.Exceptions;
using InventoryService.SDK.Persons.Models;

namespace InventoryService.Persons
{
    internal class PersonsService : IPersonsService
    {
        private readonly List<PersonModel> allPersons = new List<PersonModel>
        {
            new PersonModel
            {
                PersonId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@gmail.com",
                Phone = "1-800-3456789"
            },
            new PersonModel
            {
                PersonId = 2,
                FirstName = "Steve",
                LastName = "Mark",
                Email = "steve.mark@gmail.com",
                Phone = "1-800-2342329"
            }
        };

        public async Task<PersonModel> GetAsync(int personId)
        {
            return allPersons.FirstOrDefault(p => p.PersonId == personId) ?? throw new NotFoundException(personId); 
        }
    }
}
