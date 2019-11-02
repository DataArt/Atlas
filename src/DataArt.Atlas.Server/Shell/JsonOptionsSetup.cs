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
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace DataArt.Atlas.Core.Shell
{
    public class JsonOptionsSetup : IConfigureOptions<MvcJsonOptions>
    {
        public static JsonSerializerSettings Settings =>
            new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new DefaultContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() }
                }
            };

        public void Configure(MvcJsonOptions options)
        {
            // copy all needed properties from serializerSettings to options.SerializerSettings like:
            options.SerializerSettings.Formatting = Settings.Formatting;
            options.SerializerSettings.TypeNameHandling = Settings.TypeNameHandling;
            options.SerializerSettings.ContractResolver = Settings.ContractResolver;
            options.SerializerSettings.Converters = Settings.Converters;
        }
    }
}
