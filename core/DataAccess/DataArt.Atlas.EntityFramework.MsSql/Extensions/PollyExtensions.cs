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
using System.Linq;
using Polly;
using Polly.Retry;

#if NET452
using System.Data.Entity.Infrastructure;
#endif

#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore;
#endif

namespace DataArt.Atlas.EntityFramework.MsSql.Extensions
{
    public static class PollyExtensions
    {
        public static RetryPolicy RetryForeverWithEntityReload(this PolicyBuilder policyBuilder, Action<Exception, Context> onRetry)
        {
            return policyBuilder.RetryForever((exception, context) =>
            {
                ReloadEntryFromException(exception);
                onRetry(exception, context);
            });
        }

        public static RetryPolicy RetryForeverWithEntityReload(this PolicyBuilder policyBuilder, Action<Exception> onRetry)
        {
            return policyBuilder.RetryForever(exception =>
            {
                ReloadEntryFromException(exception);
                onRetry(exception);
            });
        }

        public static AsyncRetryPolicy RetryForeverWithEntityReloadAsync(this PolicyBuilder policyBuilder, Action<Exception, Context> onRetry)
        {
            return policyBuilder.RetryForeverAsync((exception, context) =>
            {
                ReloadEntryFromException(exception);
                onRetry(exception, context);
            });
        }

        public static AsyncRetryPolicy RetryForeverWithEntityReloadAsync(this PolicyBuilder policyBuilder, Action<Exception> onRetry)
        {
            return policyBuilder.RetryForeverAsync(exception =>
            {
                ReloadEntryFromException(exception);
                onRetry(exception);
            });
        }

        private static void ReloadEntryFromException(Exception exception)
        {
            var entry = ((DbUpdateException)exception).Entries.Single();
            entry.Reload();
        }
    }
}
