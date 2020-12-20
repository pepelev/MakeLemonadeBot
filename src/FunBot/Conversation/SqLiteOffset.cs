using System.Data.SQLite;
using System.Linq;
using FunBot.Storage;

namespace FunBot.Conversation
{
    public sealed class SqLiteOffset : Offset
    {
        private readonly SQLiteConnection connection;
        private readonly string token;

        public SqLiteOffset(string token, SQLiteConnection connection)
        {
            this.token = token;
            this.connection = connection;
        }

        public override int Get()
        {
            return connection.Read(@"
                SELECT value FROM offsets
                WHERE token = :token",
                row => row.MaybeInt("value"),
                ("token", token)
            ).SingleOrDefault() ?? 0;
        }

        public override Offset Put(int value)
        {
            connection.Execute(@"
                REPLACE INTO offsets (token, value)
                VALUES (:token, :value)",
                ("token", token),
                ("value", value)
            );
            return new SqLiteOffset(token, connection);
        }
    }
}