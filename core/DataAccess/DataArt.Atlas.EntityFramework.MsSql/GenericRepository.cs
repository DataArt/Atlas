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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

#if NET452
using System.Data.Entity;
#endif

#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
#endif

namespace DataArt.Atlas.EntityFramework.MsSql
{
    public abstract class GenericRepository<TContext, TEntity, TEntityId>
        where TContext : DbContext
        where TEntity : class, IEntity<TEntityId>
    {
        protected TContext Context { get; }

        protected DbSet<TEntity> DbSet { get; }

        protected GenericRepository(TContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();

#if NET452
            Context.Configuration.AutoDetectChangesEnabled = false;
#endif

#if NETSTANDARD2_0
            Context.ChangeTracker.AutoDetectChangesEnabled = false;
#endif
        }

        public ICollection<TEntity> GetAll()
        {
            return DbSet.ToList();
        }

        public IQueryable<TEntity> GetAllAsQueryable()
        {
            return DbSet.AsQueryable();
        }

        public IQueryable<TEntity> GetAllAsNoFilterQueryable()
        {
            return DbSet.AsNoFilter();
        }

        public ICollection<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Where(predicate).ToList();
        }

        public async Task<ICollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await DbSet.Where(predicate).ToListAsync();
        }

        public async Task<ICollection<TEntity>> FindAsNoFilterAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await DbSet.AsNoFilter().Where(predicate).ToListAsync();
        }

        public async Task<ICollection<TEntity>> FindAsync(IEnumerable<TEntityId> ids)
        {
            return await DbSet.Where(e => ids.Contains(e.Id)).ToListAsync();
        }

        public async Task<ICollection<TEntity>> FindAsNoFilterAsync(IEnumerable<TEntityId> ids)
        {
            return await DbSet.AsNoFilter().Where(e => ids.Contains(e.Id)).ToListAsync();
        }

        public TEntity Find(TEntityId entityId)
        {
            return DbSet.Single(GetByIdPredicate(entityId));
        }

        public Task<TEntity> FindAsync(TEntityId entityId)
        {
            return DbSet.SingleAsync(GetByIdPredicate(entityId));
        }

        public TEntity FindOrDefault(TEntityId entityId)
        {
            return DbSet.SingleOrDefault(GetByIdPredicate(entityId));
        }

        public Task<TEntity> FindOrDefaultAsync(TEntityId entityId)
        {
            return DbSet.SingleOrDefaultAsync(GetByIdPredicate(entityId));
        }

        public virtual IQueryable<TEntity> FindAsQuerable(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Where(predicate).AsQueryable();
        }

        public virtual TEntity Add(TEntity entity)
        {
#if NET452
            return DbSet.Add(entity);
#endif

#if NETSTANDARD2_0
            return DbSet.Add(entity).Entity;
#endif

        }

        public virtual IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
        {
#if NET452
            return DbSet.AddRange(entities);
#endif

#if NETSTANDARD2_0
            return entities.Select(entity => DbSet.Add(entity).Entity);
#endif
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.AnyAsync(predicate);
        }

        public virtual TEntity Delete(TEntity entity)
        {
#if NET452
            return DbSet.Remove(entity);
#endif

#if NETSTANDARD2_0
            return DbSet.Remove(entity).Entity;
#endif
        }

        public virtual IEnumerable<TEntity> DeleteRange(IEnumerable<TEntity> entities)
        {
#if NET452
            return DbSet.RemoveRange(entities);
#endif

#if NETSTANDARD2_0
            return entities.Select(entity => DbSet.Remove(entity).Entity);
#endif
        }

        public Task<int> DeleteQueryAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Where(predicate).DeleteAsync();
        }

        public virtual void Update(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync();
        }

        public virtual int SaveChanges()
        {
            return Context.SaveChanges();
        }

#if NET452
        public DbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return Context.Database.BeginTransaction(isolationLevel);
        }

#endif

#if NETSTANDARD2_0
        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return Context.Database.BeginTransaction(isolationLevel);
        }
#endif

        // this is required so that EF can create correct SQL query
        // .Equals() won't work
        // http://stackoverflow.com/questions/10402029/ef-object-comparison-with-generic-types
        private static Expression<Func<TEntity, bool>> GetByIdPredicate(TEntityId entityId) => e => (object)e.Id == (object)entityId;
    }
}
