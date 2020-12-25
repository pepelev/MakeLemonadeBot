using System;

namespace FunBot.Communication
{
    public static class Expires
    {
        public static DateTime Never => new DateTime(5000, 1, 1, 00, 00, 00, DateTimeKind.Utc);
    }
}