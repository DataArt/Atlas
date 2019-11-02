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
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using DataArt.Atlas.CallContext.Idempotency;
using Serilog;
using Z.EntityFramework.Plus;

namespace DataArt.Atlas.DataAccess.EntityFramework.MsSql.EntityFramework.Messaging
{
    public abstract class IdempotentMessageConsumerDbContext<TContext> : BaseDbContext<TContext>
        where TContext : DbContext
    {
        private DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();

        public override int SaveChanges()
        {
            if (EnsureConsumerMessageAsync().GetAwaiter().GetResult())
            {
                var result = base.SaveChanges();
                MessagingContext.IsConsumed = true;
                return result;
            }

            return 0;
        }

        public override async Task<int> SaveChangesAsync()
        {
            if (await EnsureConsumerMessageAsync())
            {
                var result = await base.SaveChangesAsync();
                MessagingContext.IsConsumed = true;
                return result;
            }

            return 0;
        }

        public Task EraseAsync(int daysAgo)
        {
            var moment = DateTimeOffset.UtcNow.AddDays(-daysAgo);
            return ConsumedMessages.Where(m => m.ConsumedAt < moment).DeleteAsync();
        }

        protected IdempotentMessageConsumerDbContext()
        {
        }

        protected IdempotentMessageConsumerDbContext(string connectionString) : base(connectionString)
        {
        }

        protected IdempotentMessageConsumerDbContext(DbConnection dbConnection) : base(dbConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ConsumedMessage>();
        }

        private async Task<bool> EnsureConsumerMessageAsync()
        {
            if (!MessagingContext.MessageId.HasValue)
            {
                return true;
            }

            if (!MessagingContext.ProvideIdempotency.HasValue)
            {
                throw new InvalidOperationException($"{nameof(MessagingContext)}.{nameof(MessagingContext.ProvideIdempotency)} should be specified explicitly");
            }

            if (!MessagingContext.ProvideIdempotency.Value)
            {
                return true;
            }

            if (MessagingContext.IsConsumed)
            {
                throw new InvalidOperationException("Idempotency could be provided in a single transaction only");
            }

            var messageId = MessagingContext.MessageId.Value;

            var message = await ConsumedMessages.FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                ConsumedMessages.Add(new ConsumedMessage { Id = messageId });
                return true;
            }

            Log.Warning("Attempt to consume already consumed message: {@Message}", message);

            return false;
        }
    }
}
#endif