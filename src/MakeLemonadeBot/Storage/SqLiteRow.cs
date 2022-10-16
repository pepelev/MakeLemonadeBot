using System;
using System.Data.SQLite;
using System.Globalization;

namespace MakeLemonadeBot.Storage
{
    public sealed class SqLiteRow
    {
        private readonly SQLiteDataReader reader;

        public SqLiteRow(SQLiteDataReader reader)
        {
            this.reader = reader;
        }

        public string String(string column) => reader[column] switch
        {
            string @string => @string,
            int @int => @int.ToString(CultureInfo.InvariantCulture),
            long @long => @long.ToString(CultureInfo.InvariantCulture),
            var any => throw new InvalidOperationException($"Unexpected {any?.GetType()} {any}")
        };

        public string? MaybeString(string column) => reader[column] switch
        {
            string @string => @string,
            null => null,
            DBNull _ => null,
            { } any => throw new InvalidOperationException($"Unexpected {any.GetType()} {any}")
        };

        public int Int(string column) => (int) reader[column];

        public long Long(string column) => reader[column] switch
        {
            int @int => @int,
            long @long => @long,
            { } any => throw new InvalidOperationException($"Unexpected {any.GetType()} {any}"),
            null => throw new InvalidOperationException("Unexpected null")
        };

        public int? MaybeInt(string column) => reader[column] switch
        {
            int @int => @int,
            null => null,
            DBNull _ => null,
            { } any => throw new InvalidOperationException($"Unexpected {any.GetType()} {any}")
        };
    }
}