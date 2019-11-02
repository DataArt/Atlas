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

#if NET452
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DataArt.Atlas.DataAccess.EntityFramework.MsSql.EntityFramework.Interceptors;
using Z.EntityFramework.Plus;

namespace DataArt.Atlas.DataAccess.EntityFramework.MsSql.EntityFramework
{
    public class BaseDbContext<TContext> : DbContext
        where TContext : DbContext
    {
        // ReSharper disable once StaticMemberInGenericType
        private static bool isAnyFilterInitilized;

        private string IdPropertyName { get; } = typeof(IEntity<>).GetProperties().First().Name;

        static BaseDbContext()
        {
#if !DEBUG
            Database.SetInitializer(new NullDatabaseInitializer<TContext>());
#else
            Database.SetInitializer(new CreateDatabaseIfNotExists<TContext>());
#endif
        }

        protected BaseDbContext() : base("name=LocalConnection")
        {
            Initialize();
        }

        protected BaseDbContext(string connectionString) : base(connectionString)
        {
            Initialize();
        }

        protected BaseDbContext(DbConnection dbConnection) : base(dbConnection, true)
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

        public override async Task<int> SaveChangesAsync()
        {
            var interceptionContext = GetInterceptionContext();
            var interceptors = DbInterceptorsProvider.Get().ToList();

            interceptors.ForEach(i => i.Before(interceptionContext));
            var result = await SaveChangesAsync(CancellationToken.None);
            interceptors.ForEach(i => i.After(interceptionContext));

            return result;
        }

        protected static void InitilizeGlobalFilters<T>(Expression<Func<T, bool>> filterExpression)
            where T : class
        {
            QueryFilterManager.Filter<T>(p => p.Where(filterExpression));

            isAnyFilterInitilized = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
            modelBuilder.Properties<decimal>().Configure(c => c.HasPrecision(30, 8));

            modelBuilder.Types().Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
                .Configure(SetIdPropertyName);

            if (Database.Connection is SqlConnection)
            {
                modelBuilder.Conventions.Add<SqlColumnTypeAttributeConvention>();
            }
        }

        private void Initialize()
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;

            if (isAnyFilterInitilized)
            {
                QueryFilterManager.InitilizeGlobalFilter(this);
            }
        }

        private void SetIdPropertyName(ConventionTypeConfiguration entityTypeConfiguration)
        {
            var propertyConfiguration = entityTypeConfiguration.Property(IdPropertyName);
            var propertyInfo = propertyConfiguration.ClrPropertyInfo;
            var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();

            if (!string.IsNullOrEmpty(columnAttribute?.Name))
            {
                propertyConfiguration.HasColumnName(columnAttribute.Name);
            }
            else
            {
                propertyConfiguration.HasColumnName(entityTypeConfiguration.ClrType.Name + IdPropertyName);
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