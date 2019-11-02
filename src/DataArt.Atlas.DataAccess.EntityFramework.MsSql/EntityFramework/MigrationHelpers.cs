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
using System;

namespace DataArt.Atlas.EntityFramework.MsSql.EntityFramework
{
    public static class MigrationHelpers
    {
        public static string DropDefaultConstraint(string tableName, string columnName)
        {
            var nameVariableName = $"@name_{Guid.NewGuid().ToString().Replace("-", "_")}";

            return $@"DECLARE {nameVariableName} sysname
                      SELECT {nameVariableName} = dc.name
                      FROM sys.columns c
                      JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
                      WHERE c.object_id = OBJECT_ID('{tableName}')
                      AND c.name = '{columnName}'

                      IF {nameVariableName} IS NOT NULL
                      EXECUTE ('ALTER TABLE {tableName} DROP CONSTRAINT ' + {nameVariableName})";
        }
    }
}
