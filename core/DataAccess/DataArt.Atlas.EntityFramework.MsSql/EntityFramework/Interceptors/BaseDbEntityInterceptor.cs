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

#if NET452
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace DataArt.Atlas.EntityFramework.MsSql.EntityFramework.Interceptors
{
    public abstract class BaseDbEntityInterceptor : IDbInterceptor
    {
        private static readonly EntityState[] TrackedStates = { EntityState.Added, EntityState.Modified, EntityState.Deleted };

        public Type TargetType { get; }

        protected BaseDbEntityInterceptor(Type targetType)
        {
            TargetType = targetType;
        }

        public void Before(DbInterceptionContext context)
        {
            foreach (var state in TrackedStates)
            {
                var entries = context.EntriesByState[state];

                foreach (var entry in entries)
                {
                    if (IsTargetEntity(entry, state))
                    {
                        OnBefore(entry, state, context);
                    }
                }
            }
        }

        public void After(DbInterceptionContext context)
        {
            foreach (var state in TrackedStates)
            {
                var entries = context.EntriesByState[state];

                foreach (var entry in entries)
                {
                    if (IsTargetEntity(entry, state))
                    {
                        OnAfter(entry, state, context);
                    }
                }
            }
        }

        protected virtual bool IsTargetEntity(DbEntityEntry item, EntityState state)
        {
            return TargetType.IsInstanceOfType(item.Entity);
        }

        protected virtual void OnBefore(DbEntityEntry item, EntityState state, DbInterceptionContext context)
        {
        }

        protected virtual void OnAfter(DbEntityEntry item, EntityState state, DbInterceptionContext context)
        {
        }
    }
}
#endif