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

using System.Data.SqlClient;
using System.Linq;

#if NET452
using System.Data.Entity.Infrastructure;
#endif

#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore;
#endif

namespace DataArt.Atlas.EntityFramework.MsSql.Extensions
{
    public static class DbUpdateExceptionExtension
    {
        private static readonly int[] KeyViolationSqlErrorNumbers =
        {
            547, // Constraint check violation concurrency exception (547)
            2601, // Unique index concurrency exception (2601)
            2627 // Unique constraint concurrency exception (2627)
        };

        public static bool IsKeyViolation(this DbUpdateException exception)
        {
            // bug in code analyzer
#pragma warning disable SA1119 // Statement must not use unnecessary parenthesis
            if (!(exception.InnerException?.InnerException is SqlException sqlException))
#pragma warning restore SA1119 // Statement must not use unnecessary parenthesis
            {
                return false;
            }

            return KeyViolationSqlErrorNumbers.Contains(sqlException.Number);
        }
    }
}
