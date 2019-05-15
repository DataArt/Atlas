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
#if NETSTANDARD2_0
using System;
using AutoMapper;
using DataArt.Atlas.Infrastructure.AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Core.Shell
{
    public static class AutoMapperExtensions
    {
        public static void ConfigureAutoMapper(
            this IServiceCollection services,
            Action<IMapperConfigurationExpression> configure)
        {
            Mapper.Reset();
            Mapper.Initialize(configure);
            Mapper.Configuration.CompileMappings();

            services.AddSingleton(opt => (MapperConfiguration)Mapper.Configuration);
            services.AddScoped(opt => opt
                .GetService<MapperConfiguration>()
                .CreateMapper(opt.GetRequiredService<IServiceScope>().ServiceProvider.GetService));

            var projectionConfiguration = new ProjectionMapperConfiguration(cfg =>
            {
                cfg.AllowNullDestinationValues = false;
                configure(cfg);
            });
            projectionConfiguration.CompileMappings();
            services.AddSingleton<IProjectionConfigurationProvider>(opt => projectionConfiguration);
        }
    }
}
#endif