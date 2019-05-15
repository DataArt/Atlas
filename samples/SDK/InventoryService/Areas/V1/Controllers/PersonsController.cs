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
using InventoryService.SDK.Persons.Models;
using InventoryService.Persons;
using System.Threading.Tasks;
using System.Web.Http;

namespace InventoryService.Areas.V1.Controllers
{
    [RoutePrefix("api/v1/persons")]
    public class PersonsController : ApiController
    {
        private readonly IPersonsService personsService;

        public PersonsController(IPersonsService personsService)
        {
            this.personsService = personsService;
        }

        [HttpGet]
        [Route("{personId}")]
        public async Task<PersonModel> GetAsync([FromUri] int personId)
        {
            return await personsService.GetAsync(personId);
        }
    }
}
