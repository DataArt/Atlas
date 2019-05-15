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

using DataArt.Atlas.EntityFramework.MsSql.Extensions;
using Polly;

#if NET452
using System.Data.Entity.Infrastructure;
#endif

#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore;
#endif

namespace DataArt.Atlas.EntityFramework.MsSql.Helpers
{
    public static class PollyHelper
    {
        public static PolicyBuilder HandleConcurrencyAndKeysDuplication()
        {
            return Policy
                .Handle<DbUpdateConcurrencyException>()
                .Or<DbUpdateException>(e => e.IsKeyViolation());
        }

        public static PolicyBuilder HandleKeysDuplication()
        {
            return Policy.Handle<DbUpdateException>(e => e.IsKeyViolation());
        }
    }
}
