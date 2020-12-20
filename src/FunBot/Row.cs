using System;
using System.Collections.Generic;

namespace FunBot
{
    public sealed class Row
    {
        private readonly IList<object> cells;

        public Row(params object[] cells)
            : this(cells as IList<object>)
        {
        }

        public Row(IList<object> cells)
        {
            this.cells = cells;
        }

        public Location Find(string name)
        {
            var equality = StringComparer.InvariantCultureIgnoreCase;
            for (var i = 0; i < cells.Count; i++)
            {
                var value = cells[i];
                if (equality.Equals(name, value?.ToString()))
                {
                    return new Location(i);
                }
            }

            return Location.NotFound;
        }

        public bool Has(Location location)
        {
            if (!location.Found)
                return false;

            var index = location.Index;
            if (0 <= index && index < cells.Count)
                return !string.IsNullOrWhiteSpace(cells[index]?.ToString());

            return false;
        }

        public string Get(Location location) => cells[location.Index].ToString().Trim();

        public string? TryGet(Location location) => Has(location)
            ? Get(location)
            : null;
    }
}
