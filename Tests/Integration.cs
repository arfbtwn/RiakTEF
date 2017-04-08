using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiakClient;
using RiakClient.Comms;
using RiakClient.Config;

namespace Tests
{
    using RiakTEF;
    using RiakTEF.Initializers;

    [TestFixture]
    class Integration
    {
        [Test]
        public void _()
        {
            var conf = new RiakClusterConfiguration
            {
                Nodes =
                {
                    new RiakNodeConfiguration { Name = "riak@127.0.0.1", HostAddress = "localhost" }
                }
            };
            var fact = new RiakConnectionFactory();
            var clus = new RiakCluster(conf, fact);

            var client = clus.CreateClient();

            var init = new IfNotLatest(new IfNotExists(new Table.Create()), new Table.Throw());

            DbContext.SetInitializer(init);

            var context = new MyContext(client);

            var set = context.Entities;

            var e1 = new MyEntity();

            set.Add(e1);

            context.Save();

            var e2 = set.First(x => x.Id == e1.Id && x.Stamp == e1.Stamp);

            Assert.IsNotNull(e2);
            Assert.AreEqual(e1.Id,    e2.Id);
            Assert.AreEqual(e1.Stamp, e2.Stamp);
        }

        class MyEntity
        {
            public Guid     Id    { get; set; } = Guid.NewGuid();
            public DateTime Stamp { get; set; } = DateTime.UtcNow;
        }

        class MyContext : DbContext
        {
            public MyContext(IRiakClient client) : base(client) { }

            protected override void Initialize(ISchema schema)
            {
                schema.Entity<MyEntity>().Auto();
                schema.Entity<MyEntity>().Key.Partition(x => new { x.Id, x.Stamp });

                schema.Entity<MyEntity>().Column(x => x.Stamp).Quantum(1, 'd');
            }

            public DbSet<MyEntity> Entities => Set<MyEntity>();
        }
    }
}
