using System;
using System.Linq;
using NUnit.Framework;

using RiakTEF;
using RiakTEF.Models;

namespace Tests
{
    [TestFixture]
    class Schemas
    {
        const string CreateRecord = "CREATE TABLE record (\r\n"
                                  + "    id VARCHAR NOT NULL,\r\n"
                                  + "    when TIMESTAMP NOT NULL,\r\n"
                                  + "\r\n"
                                  + "    PRIMARY KEY (\r\n"
                                  + "        (id, QUANTUM(when, 1, 'd')),\r\n"
                                  + "        id, when DESC\r\n"
                                  + "    )\r\n"
                                  + ");\r\n";

        [Test]
        public void Entity()
        {
            var sut = Schema.Create();

            var e = sut.Entity<Record>().Auto();

            e.Column(x => x.When).Quantum(1, Unit.Days)
                                 .Sort(Order.Desc);

            Assert.AreEqual(1, sut.Entities.Count());
            Assert.AreSame (e, sut.Entities.First());
            Assert.AreEqual(2, e.Columns.Count());
        }

        [Test]
        public void Write()
        {
            var schema = Schema.Create();

            var e = schema.Entity<Record>().Auto();

            e.Key.Partition(x => new { x.Id, x.When });

            e.Column(x => x.When).Quantum(1, Unit.Days)
                                 .Sort   (Order.Desc);

            var sut = new SchemaWriter();

            var query = sut.Write(e);

            Assert.AreEqual(CreateRecord, query);
        }

        class Record
        {
            public Guid     Id    { get; set; }
            public DateTime When  { get; set; }
        }
    }
}
