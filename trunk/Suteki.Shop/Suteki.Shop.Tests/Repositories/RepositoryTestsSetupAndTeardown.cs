using System;
using NUnit.Framework;
using Suteki.Shop.Maps;
using Suteki.Shop.Tests.Maps;

namespace Suteki.Shop.Tests.Repositories
{
    [SetUpFixture]
    public class RepositoryTestsSetupAndTeardown
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