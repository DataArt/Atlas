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
using Microsoft.AspNetCore.Mvc;
using SampleService.DataAccess;

namespace SampleService.Areas.V1.Controllers
{
    [Route("api/v1/entity")]
    public class EntityController : ControllerBase
    {
        private readonly SampleEntityRepository repository;

        public EntityController(SampleEntityRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        [Route("{id}")]
        public SampleEntity Get(int id)
        {
            return repository.Find(id);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> Post([FromBody] SampleEntity entity)
        {
            var et = repository.Add(entity);
            await repository.SaveChangesAsync();
            return et.Id;
        }
    }
}
