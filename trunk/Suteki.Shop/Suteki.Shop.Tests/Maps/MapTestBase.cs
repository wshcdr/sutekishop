using System;
using System.Data;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Suteki.Shop.Maps;
using Suteki.Shop.Repositories;

namespace Suteki.Shop.Tests.Maps
{
    public static class InMemoryDatabaseManager
    {
        private static Configuration configuration;
        private static IDbConnection connection;
        private static ISessionFactory sessionFactory;
        static readonly object sessionFactoryLock = new object();

        /// <summary>
        /// Fixture setup and teardown doesn't seem to work in some build environments, so 
        /// fall back to lazy initialisation.
        /// </summary>
        private static ISessionFactory SessionFactory
        {
            get
            {
                if(sessionFactory == null)
                {
                    lock (sessionFactoryLock)
                    {
                        if (sessionFactory == null)
                        {
                            Start(typeof(ProductMap).Assembly);
                        }
                    }
                }
                return sessionFactory;
            }
        }

        public static void Start(Assembly mapAssembly)
        {
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            sessionFactory = GetSessionFactory();
            connection = OpenConnection();
            BuildSchema(OpenSession());
        }

        public static void Stop()
        {
            sessionFactory.Dispose();
            connection.Dispose();
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession(connection);
        }

        private static ISessionFactory GetSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(FluentNHibernateConfigurationBuilder.ConfigureMappings)
                .ExposeConfiguration(c => configuration = c)
                .BuildSessionFactory();
        }

        private static void BuildSchema(ISession session)
        {
            var export = new SchemaExport(configuration);
            export.Execute(true, true, false, session.Connection, null);
        }

        private static IDbConnection OpenConnection()
        {
            var sqLiteConnection = new System.Data.SQLite.SQLiteConnection("Data Source=:memory:;Version=3;New=True");
            sqLiteConnection.Open();
            return sqLiteConnection;
        }
    }

    public abstract class MapTestBase
    {
        protected ISession OpenSession()
        {
            return InMemoryDatabaseManager.OpenSession();
        }

        protected void InSession(Action<ISession> action)
        {
            using (var session = InMemoryDatabaseManager.OpenSession())
            using(var transaction = session.BeginTransaction())
            {
                action(session);
                transaction.Commit();
            }
        }
    }
}