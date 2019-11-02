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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DataArt.Atlas.EntityFramework.MsSql.EntityFramework.Interceptors;
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace DataArt.Atlas.EntityFramework.MsSql.EntityFramework
{
    public class BaseDbContext<TContext> : DbContext
        where TContext : DbContext
    {
        private static bool isAnyFilterInitilized;

        private string IdPropertyName { get; } = typeof(IEntity<>).GetProperties().First().Name;

        protected BaseDbContext(DbContextOptions<TContext> options)
            : base(options)
        {
        }

        public override int SaveChanges()
        {
            var interceptionContext = GetInterceptionContext();
            var interceptors = DbInterceptorsProvider.Get().ToList();

            interceptors.ForEach(i => i.Before(interceptionContext));
            var result = base.SaveChanges();
            interceptors.ForEach(i => i.After(interceptionContext));

            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var interceptionContext = GetInterceptionContext();
            var interceptors = DbInterceptorsProvider.Get().ToList();

            interceptors.ForEach(i => i.Before(interceptionContext));
            var result = await base.SaveChangesAsync(cancellationToken);
            interceptors.ForEach(i => i.After(interceptionContext));

            return result;
        }

        protected static void InitilizeGlobalFilters<T>(Expression<Func<T, bool>> filterExpression)
            where T : class
        {
            QueryFilterManager.Filter<T>(p => p.Where(filterExpression));

            isAnyFilterInitilized = true;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseLazyLoadingProxies(false);
            if (isAnyFilterInitilized)
            {
                QueryFilterManager.InitilizeGlobalFilter(this);
            }

            optionsBuilder.UseLoggerFactory(AtlasLogging.LoggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
                .Where(p => p.Relational().ColumnType == null))
            {
                if (property.ClrType == typeof(decimal))
                {
                    property.Relational().ColumnType = "decimal(30, 8)";
                }

                if (property.ClrType == typeof(DateTime))
                {
                    property.Relational().ColumnType = "datetime2";
                }
            }

            foreach (var type in modelBuilder.Model.GetEntityTypes()
                .Where(t => t.ClrType.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>))))
            {
                var property = type.GetProperties().First(p => p.Name == IdPropertyName);
                var attr = property.PropertyInfo.GetCustomAttribute<ColumnAttribute>();

                if (!string.IsNullOrEmpty(attr?.Name))
                {
                    property.Relational().ColumnName = attr.Name;
                }
                else
                {
                    property.Relational().ColumnName = type.ClrType.Name + IdPropertyName;
                }
            }

            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
            {
                var attr = property.PropertyInfo.GetCustomAttribute<SqlColumnTypeAttribute>();
                if (!string.IsNullOrEmpty(attr?.ColumnType))
                {
                    property.Relational().ColumnType = attr.ColumnType;
                }
            }
        }

        private DbInterceptionContext GetInterceptionContext()
        {
            var entries = ChangeTracker.Entries().ToList();
            var entriesByState = entries.ToLookup(row => row.State);

            return new DbInterceptionContext
            {
                DbContext = this,
                Entries = entries,
                EntriesByState = entriesByState,
            };
        }
    }
}
#endif