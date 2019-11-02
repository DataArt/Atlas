using System.Collections.Generic;
using System.Linq;

namespace DataArt.Atlas.Core.Extensions
{
    public static class QueueExtensions
    {
        public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> list)
        {
            if (list != null)
            {
                foreach (var item in list.Where(x => x != null))
                {
                    queue.Enqueue(item);
                }
            }
        }
    }
}