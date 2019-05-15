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
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace DataArt.Atlas.Tests.Common
{
    public static class AutoMapperConfigValidator
    {
        public static void Validate(Action<IMapperConfigurationExpression> configure)
        {
            var configuration = new MapperConfiguration(c =>
            {
                configure(c);
                c.Advanced.Validator(EnumsDefaultValidator);
            });

            var mapper = configuration.CreateMapper();

            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        private static void EnumsDefaultValidator(ValidationContext context)
        {
            var sourceType = context.Types.SourceType;
            var destinationType = context.Types.DestinationType;

            if (!sourceType.IsEnum || !destinationType.IsEnum)
            {
                return;
            }

            var memberList = context.TypeMap?.ConfiguredMemberList ?? context.PropertyMap.TypeMap.ConfiguredMemberList;

            IList<string> errors;

            switch (memberList)
            {
                case MemberList.Destination:
                    errors = ValidateEnumsMapping(destinationType, sourceType);
                    break;
                case MemberList.Source:
                    errors = ValidateEnumsMapping(sourceType, destinationType);
                    break;
                default:
                    errors = new List<string>();
                    break;
            }

            if (errors.Any())
            {
                throw new AutoMapperConfigurationException($"Mapping from enum {context.Types.SourceType} to {context.Types.DestinationType}: {string.Join(", ", errors)}");
            }
        }

        private static IList<string> ValidateEnumsMapping(Type mappedEnumType, Type sourceEnumType)
        {
            IList<string> errors = new List<string>();

            var mappedValues = mappedEnumType.GetEnumValues();
            var sourceNames = sourceEnumType.GetEnumNames();

            foreach (var mappedValue in mappedValues)
            {
                var mappedName = mappedEnumType.GetEnumName(mappedValue);
                var sourceName = sourceEnumType.GetEnumName(mappedValue);

                var isValueMapped = sourceName != null;
                var isNameMapped = sourceNames.Contains(mappedName);

                if (!isValueMapped)
                {
                    errors.Add($"value that corresponds to '{mappedValue}' is not mapped");
                }
                else if (!isNameMapped || sourceName != mappedName)
                {
                    errors.Add($"name '{mappedName}' is incorrectly mapped to '{sourceName}'");
                }
            }

            return errors;
        }
    }
}
