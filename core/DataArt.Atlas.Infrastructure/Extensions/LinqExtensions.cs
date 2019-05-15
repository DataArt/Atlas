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
using System.Linq;

namespace DataArt.Atlas.Infrastructure.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TResult> FullOuterJoinDisctinct<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TResult> resultSelector)
        {
            EnsureCollections(ref left, ref right);

            var leftLookup = left.ToLookup(leftKeySelector);
            var rightLookup = right.ToLookup(rightKeySelector);

            var keys = leftLookup.Select(l => l.Key).Union(rightLookup.Select(r => r.Key));

            return keys.Select(key => resultSelector(leftLookup[key].FirstOrDefault(), rightLookup[key].FirstOrDefault()));
        }

        public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TResult> resultSelector)
        {
            EnsureCollections(ref left, ref right);

            return left
                .GroupJoin(right, leftKeySelector, rightKeySelector, (l, rights) => new { l, rights = rights.DefaultIfEmpty() })
                .SelectMany(g => g.rights, (g, r) => resultSelector(g.l, r));
        }

        private static void EnsureCollections<TLeft, TRight>(ref IEnumerable<TLeft> left, ref IEnumerable<TRight> right)
        {
            left = left ?? new TLeft[0];
            right = right ?? new TRight[0];
        }
    }
}
