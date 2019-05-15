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

[assembly: DataArt.Atlas.Infrastructure.ExplicitTypeUsage(typeof(Newtonsoft.Json.ConstructorHandling))]

namespace DataArt.Atlas.Infrastructure
{
    /// <summary>
    /// For situations when we explicitly need to reference type.
    /// For example: type usage from external library to be sure third party
    /// library will be copied no matter how output folders are configured.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ExplicitTypeUsageAttribute : Attribute
    {
        // ReSharper disable once UnusedParameter.Local
        public ExplicitTypeUsageAttribute(Type externalType)
        {
        }
    }
}