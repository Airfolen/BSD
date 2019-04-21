using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSD.Comparers
{
    public class EnumerableComparer<T> : IEqualityComparer<IEnumerable<T>> where T : IComparable<T>
    {
        public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == second)
                return true;
            if ((first == null) || (second == null))
                return false;

            return new HashSet<T>(first).SetEquals(second);
        }

        public int GetHashCode(IEnumerable<T> enumerable)
        {
            return enumerable.OrderBy(x => x)
              .Aggregate(17, (current, val) => current * 23 + val.GetHashCode());
        }
    }
}
