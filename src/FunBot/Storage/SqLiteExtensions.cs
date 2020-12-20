using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FunBot.Storage
{
    public static class SqLiteExtensions
    {
        private static SQLiteCommand Using(
            this SQLiteCommand command,
            params (string Name, Value Value)[] parameters)
        {
            foreach (var (name, value) in parameters)
            {
                command.Parameters.AddWithValue(name, value.Content);
            }

            return command;
        }

        public static IReadOnlyList<T> Read<T>(
            this SQLiteConnection connection,
            string query,
            Func<SqLiteRow, T> projection)
        {
            using var command = new SQLiteCommand(query, connection);
            return Read(projection, command);
        }

        public static IReadOnlyList<T> Read<T>(
            this SQLiteConnection connection,
            string query,
            Func<SqLiteRow, T> projection,
            params (string Name, Value Value)[] parameters)
        {
            using var command = new SQLiteCommand(query, connection);
            return Read(projection, command.Using(parameters));
        }

        private static IReadOnlyList<T> Read<T>(Func<SqLiteRow, T> projection, SQLiteCommand command)
        {
            var reader = command.ExecuteReader();
            var result = new List<T>();
            while (reader.Read())
            {
                var row = new SqLiteRow(reader);
                result.Add(projection(row));
            }

            return result;
        }

        public static void Execute(this SQLiteConnection connection, string query)
        {
            using var command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();
        }

        public static void Execute(
            this SQLiteConnection connection,
            string query,
            params (string Name, Value Value)[] parameters)
        {
            using var command = new SQLiteCommand(query, connection);
            command.Using(parameters).ExecuteNonQuery();
        }

        public static void Execute(
            this SQLiteTransaction transaction,
            string query,
            params (string Name, Value Value)[] parameters)
        {
            using var command = new SQLiteCommand(query, transaction.Connection, transaction);
            command.Using(parameters).ExecuteNonQuery();
        }

        public static IReadOnlyList<T> Read<T>(
            this SQLiteTransaction transaction,
            string query,
            Func<SqLiteRow, T> projection)
        {
            using var command = new SQLiteCommand(query, transaction.Connection, transaction);
            return Read(projection, command);
        }

        public static IReadOnlyList<T> Read<T>(
            this SQLiteTransaction transaction,
            string query,
            Func<SqLiteRow, T> projection,
            params (string Name, Value Value)[] parameters)
        {
            using var command = new SQLiteCommand(query, transaction.Connection, transaction);
            return Read(projection, command.Using(parameters));
        }
    }
}