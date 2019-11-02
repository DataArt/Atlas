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

#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataArt.Atlas.EntityFramework.MsSql.EntityFramework.Interceptors
{
    public class HistoricalEntityInterceptor<THistoryTrackedEntity, THistoricalEntity> : BaseDbEntityInterceptor
        where THistoryTrackedEntity : class, IHistoryTrackedEntity
        where THistoricalEntity : class, IHistoricalEntity
    {
        private readonly string[] syncedTimestampPropertyNames;

        public HistoricalEntityInterceptor(params Expression<Func<THistoryTrackedEntity, DateTimeOffset>>[] syncedTimestampPropertySelectors) : base(typeof(THistoryTrackedEntity))
        {
            var propertyNames = new List<string>();

            foreach (var selector in syncedTimestampPropertySelectors)
            {
                var expression = (MemberExpression)selector.Body;
                propertyNames.Add(expression.Member.Name);
            }

            syncedTimestampPropertyNames = propertyNames.ToArray();
        }

        protected override void OnBefore(EntityEntry entry, EntityState state, DbInterceptionContext context)
        {
            var moment = DateTimeOffset.UtcNow;
            var entity = (THistoryTrackedEntity)entry.Entity;

            switch (state)
            {
                case EntityState.Added:
                    entity.ValidFrom = moment;
                    entity.ValidTo = DateTimeOffset.MaxValue;
                    break;

                case EntityState.Modified:
                    AddHistoricalEntity(context.DbContext, entry.OriginalValues, moment);
                    entity.ValidFrom = moment;
                    break;

                case EntityState.Deleted:
                    AddHistoricalEntity(context.DbContext, entry.OriginalValues, moment);
                    break;
            }

            foreach (var propertyName in syncedTimestampPropertyNames)
            {
                SyncTimestampProperty(entry, state, propertyName, moment);
            }
        }

        private static void AddHistoricalEntity(DbContext dbContext, PropertyValues originalValues, DateTimeOffset moment)
        {
            var original = Mapper.Map<THistoricalEntity>(originalValues.ToObject());
            original.ValidTo = moment;
            dbContext.Set<THistoricalEntity>().Add(original);
        }

        private static void SyncTimestampProperty(EntityEntry entry, EntityState state, string propertyName, DateTimeOffset moment)
        {
            switch (state)
            {
                case EntityState.Added:
                    if (IsPropertySet(entry, propertyName))
                    {
                        entry.CurrentValues[propertyName] = moment;
                    }

                    break;

                case EntityState.Modified:
                    if (IsPropertyModified(entry, propertyName))
                    {
                        entry.CurrentValues[propertyName] = moment;
                    }

                    break;
            }
        }

        private static bool IsPropertySet(EntityEntry entry, string propertyName)
        {
            return (DateTimeOffset)entry.CurrentValues[propertyName] != default(DateTimeOffset);
        }

        private static bool IsPropertyModified(EntityEntry entry, string propertyName)
        {
            return (DateTimeOffset)entry.OriginalValues[propertyName] != (DateTimeOffset)entry.CurrentValues[propertyName];
        }
    }
}
#endif