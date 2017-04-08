using System;
using Moq;
using NUnit.Framework;
using RiakClient;
using RiakClient.Commands.TS;

using RiakTEF;
using RiakTEF.Models;
using RiakTEF.Profiles;

namespace Tests
{
    [TestFixture]
    class Context
    {
        static MyEntity E => new MyEntity { Id = Guid.NewGuid(), When1 = DateTime.UtcNow };

        [Test]
        public void Added()
        {
            var mock = new Mock<IRiakClient>();

            mock.Setup(x => x.Execute(It.IsAny<Store>()))
                .Returns(new RiakResult());

            using (var sut = new MyContext(mock.Object))
            {
                sut.Entities.Add(E);
            }

            mock.Verify(x => x.Execute(It.IsAny<Store>()), Times.Once);
        }

        [Test]
        public void Removed()
        {
            var mock = new Mock<IRiakClient>();

            mock.Setup(x => x.Execute(It.IsAny<Delete>()))
                .Returns(new RiakResult());

            using (var sut = new MyContext(mock.Object))
            {
                sut.Entities.Remove(E);
            }

            mock.Verify(x => x.Execute(It.IsAny<Delete>()), Times.Once);
        }

        [Test]
        public void Transient()
        {
            var mock = new Mock<IRiakClient>();

            using (var sut = new MyContext(mock.Object))
            {
                var e = E;

                sut.Entities.Add(e);
                sut.Entities.Remove(e);
            }

            mock.Verify(x => x.Execute(It.IsAny<Store>()),  Times.Never);
            mock.Verify(x => x.Execute(It.IsAny<Delete>()), Times.Never);
        }

        class MyContext : DbContext
        {
            public MyContext(IRiakClient client) : base(client) { }

            protected override void Initialize(ISchema schema)
            {
                schema.Register<MyProfile>();
            }

            public DbSet<MyEntity> Entities => Set<MyEntity>();
        }

        class MyProfile : Profile<MyEntity>
        {
            protected override void Register(IEntity<MyEntity> entity)
            {
                entity.Table("MyEntity").Auto();

                entity.Key.Partition(x => new { x.Id, x.When1 })
                          .Local    (x => x.Field1);

                entity.Column(x => x.When1).Quantum(1, Unit.Days)
                                           .Sort   (Order.Desc);

                entity.Ignore(x => x.Field5);
            }
        }

        class MyEntity
        {
            public Guid      Id     { get; set; }
            public DateTime  When1  { get; set; }
            public DateTime? When2  { get; set; }
            public long      Field1 { get; set; }
            public long?     Field2 { get; set; }
            public int       Field3 { get; set; }
            public int?      Field4 { get; set; }
            public string    Field5 { get; set; }
        }
    }
}
