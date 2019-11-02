using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataArt.Atlas.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static bool In<T>(this T obj, IEnumerable<T> values, IComparer<T> comparer = null)
        {
            values.ThrowIfNull(nameof(values));

            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            comparer = comparer ?? Comparer<T>.Default;
            return values.Any(x => comparer.Compare(obj, x) == 0);
        }

        public static Task<T> AsTask<T>(this T obj)
        {
            return Task.FromResult(obj);
        }

        public static IEnumerable<T> Yield<T>(this T obj)
        {
            if (obj == null)
            {
                yield break;
            }

            yield return obj;
        }

        public static IList<T> AsSingleItemList<T>(this T obj)
        {
            return new List<T> { obj };
        }
    }
}