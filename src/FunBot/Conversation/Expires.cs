using System;

namespace FunBot.Conversation
{
    public static class Expires
    {
        public static DateTime Never => new DateTime(5000, 1, 1, 00, 00, 00, DateTimeKind.Utc);
    }
}