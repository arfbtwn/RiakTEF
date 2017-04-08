using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;

using RiakTEF;
using RiakTEF.Linq;
using RiakTEF.Models;

namespace Tests
{
    [TestFixture]
    class Queries
    {
        const string Iso8601 = "yyyy-MM-dd HH:mm:ss.fff";

        static readonly DateTime Time  = DateTime.UtcNow;
        static readonly TimeSpan Range = TimeSpan.FromDays(1);
        static readonly Guid     Id    = Guid.NewGuid();

        static readonly string SelectEntity;

        static readonly Expression<Func<Entity, bool>> Filter = x => x.When.InRange(Time, Range) && x.Id == Id;

        static Queries()
        {
            var sb = new StringBuilder("SELECT * FROM entity")
                .AppendLine  ()
                .AppendLine  ("WHERE")
                .AppendFormat($"'{{0:{Iso8601}}}' <= when AND ", Time - Range)
                .AppendFormat($"when < '{{0:{Iso8601}}}' AND ", Time)
                .AppendFormat("id = '{0}'", Id);

            SelectEntity = sb.ToString();
        }

        static Expression Query(Expression<Func<IQueryable<Entity>, IQueryable<Entity>>> expr) => expr;
        static Expression Query(Expression<Func<IQueryable<Entity>, Entity>> expr) => expr;

        static IEntity<Entity> E
        {
            get
            {
                var e = Schema.Create().Entity<Entity>().Auto();

                e.Key.Partition(x => new { x.Id, x.When });

                e.Column(x => x.When).Quantum(1, Unit.Days);

                return e;
            }
        }

        [Test]
        public void Select()
        {
            var sut   = new Parser(E);
            var query = Query(_ => _.Where(Filter));
            var text  = sut.Parse(query);

            Assert.AreEqual(SelectEntity, text);
        }

        [Test]
        public void SkipTake()
        {
            var sut   = new Parser(E);
            var query = Query(_ => _.Where(Filter).Skip(10).Take(5));
            var text  = sut.Parse(query);

            Assert.IsTrue(text.StartsWith(SelectEntity), text);
            Assert.IsTrue(text.EndsWith(" OFFSET 10 LIMIT 5"), text);
        }


        [Test]
        public void OrderBy()
        {
            var sut   = new Parser(E);
            var query = Query(_ => _.Where(Filter)
                                    .OrderBy(x => x.Id)
                                    .ThenByDescending(x => x.When));

            var text  = sut.Parse(query);

            Assert.IsTrue(text.StartsWith(SelectEntity), text);
            Assert.IsTrue(text.EndsWith(" ORDER BY id ASC, when DESC"), text);
        }

        [Test]
        public void Chain()
        {
            var sut   = new Parser(E);
            var query = Query(_ => _.Where(x => x.When.InRange(Time, Range))
                                    .First(x => x.Id == Id));

            var text  = sut.Parse(query);

            Assert.IsTrue(text.StartsWith(SelectEntity), text);
            Assert.IsTrue(text.EndsWith(" LIMIT 1"));
        }

        [Test]
        public void Nulls()
        {
            var sut = new Parser(E);

            var q1 = Query(_ => _.Where(x => x.Id == null));
            var q2 = Query(_ => _.Where(x => x.Id != null));
            var q3 = Query(_ => _.Where(x => null == x.Id));
            var q4 = Query(_ => _.Where(x => null != x.Id));

            var t1 = sut.Parse(q1);
            var t2 = sut.Parse(q2);
            var t3 = sut.Parse(q3);
            var t4 = sut.Parse(q4);

            Assert.IsTrue(t1.EndsWith("id IS NULL"));
            Assert.IsTrue(t2.EndsWith("id IS NOT NULL"));

            Assert.AreEqual(t1, t3);
            Assert.AreEqual(t2, t4);
        }

        class Entity
        {
            public Guid     Id   { get; set; }
            public DateTime When { get; set; }
        }
    }
}
