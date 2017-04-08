using System;
using NUnit.Framework;

using RiakTEF;

namespace Tests
{
    [TestFixture]
    class Sequencing
    {
        [Test]
        public void Key()
        {
            var e = Schema.Create().Entity<MyEntity>().Auto();

            e.Key.Partition(x => new { x.Id, x.When1 })
                 .Local    (x => x.Field1);

            var col = Default.Serializers;

            var sut = col.Get(e);

            var o = new MyEntity { Id = Guid.NewGuid() };

            var k = sut.Key(o);

            Assert.AreEqual(3, k.Cells.Count);
        }

        [Test]
        public void RoundTrip()
        {
            var e = Schema.Create().Entity<MyEntity>().Auto();

            var col = Default.Serializers;

            var sut = col.Get(e);

            var o1 = new MyEntity { Id = Guid.NewGuid() };

            var r = sut.Serialize(o1);

            Assert.AreEqual(8, r.Cells.Count);

            var o2 = (MyEntity) sut.Deserialize(r);

            Assert.AreNotSame(o1, o2);

            Assert.AreEqual(o1.Id, o2.Id);
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
