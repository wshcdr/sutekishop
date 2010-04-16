using System;
using NUnit.Framework;
using Suteki.Shop.Maps;

namespace Suteki.Shop.Tests.Maps
{
    [SetUpFixture]
    public class MapsTestsSetupAndTeardown
    {
        [SetUp]
        public void RunBeforeAnyMapTests()
        {
            InMemoryDatabaseManager.Start(typeof(PostZoneMap).Assembly);
            Console.WriteLine("Opening in memory database connection");
        }

        [TearDown]
        public void RunAfterAnyMapTests()
        {
            InMemoryDatabaseManager.Stop();
            Console.WriteLine("Closing in memory database connection");
        }
    }
}