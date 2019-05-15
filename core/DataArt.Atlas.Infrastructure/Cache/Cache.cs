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
using System.Collections;

namespace DataArt.Atlas.Infrastructure.Cache
{
    public sealed class Cache<T> : ICache<T>, IDisposable
    {
        private readonly System.Runtime.Caching.MemoryCache cache;

        public Cache()
        {
            cache = new System.Runtime.Caching.MemoryCache(Guid.NewGuid().ToString());
        }

        public T this[string key] => Get(key);

        public void Set(string key, T value, TimeSpan timeToLive)
        {
            cache.Set(key, value, DateTimeOffset.Now.Add(timeToLive));
        }

        public void Set(string key, T value)
        {
            cache.Set(key, value, null);
        }

        public T Get(string key)
        {
            return (T)cache.Get(key);
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        public void Dispose()
        {
            cache?.Dispose();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)cache).GetEnumerator();
        }
    }
}
#endif