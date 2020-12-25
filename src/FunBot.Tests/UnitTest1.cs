using NUnit.Framework;
using System;
using System.Linq;
using FunBot.Collections;

namespace FunBot.Tests
{
    public class Tests
    {
        [Test]
        public void Abc()
        {
            var items = new[]
            {
                new{ Id = 12, Name = "a"},
                new{ Id = 13, Name = "b"},
                new{ Id = 14, Name = "c"}
            };
            var newItems = new[]
            {
                new{ Id = 13, Name = "b"},
                new{ Id = 14, Name = "c"},
                new{ Id = 15, Name = "d"}
            };

            var @join = Full.Join(
                items.By(item => item.Id),
                newItems.By(item => item.Id)
            );

            Console.WriteLine(
                string.Join(
                    Environment.NewLine,
                    @join
                )
            );
        }
    }
}