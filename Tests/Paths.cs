using System;
using System.Linq.Expressions;
using NUnit.Framework;

using RiakTEF.Low;

namespace Tests
{
    [TestFixture]
    public class Paths
    {
        static Expression Path<T>(Expression<Func<T, object>> expr) => expr;

        [Test]
        public void Fields()
        {
            var e    = new Entity();
            var path = Path<Entity>(x => x.Field);
            var sut  = new Property(path);

            var f1 = sut.Get(e);

            Assert.AreEqual(0, f1);

            Assert.DoesNotThrow(() => sut.Set(e, 1));

            var f2 = sut.Get(e);

            Assert.AreEqual(1, f2);
        }

        [Test]
        public void SubFields()
        {
            var e    = new Entity();
            var path = Path<Entity>(x => x.Sub.Field);
            var sut  = new Property(path);

            Assert.IsNull(e.Sub);

            var f1 = sut.Get(e);

            Assert.IsNull(f1);

            Assert.DoesNotThrow(() => sut.Set(e, 1));

            var f2 = sut.Get(e);

            Assert.AreEqual(1, f2);
        }

        class Entity
        {
            public int       Field { get; set; }
            public SubEntity Sub   { get; set; }
        }

        class SubEntity
        {
            public int Field { get; set; }
        }
    }
}
