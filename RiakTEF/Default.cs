using System;

using RiakTEF.Low;

namespace RiakTEF
{
    static class Default
    {
        public static readonly IInitializer Initializer;
        public static readonly ISerializers Serializers;

        public static readonly Grapher Grapher;
        public static readonly Mapper  Mapper;

        static Default()
        {
            Initializer = null;

            Serializers = new Serializers
            {
                new Serializers.Direct(),
                new Serializers.Guid(),
                new Serializers.TimeSpan()
            };

            Grapher = new Grapher
            {
                typeof(string),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(Guid),
                typeof(TimeSpan)
            };

            Mapper = new Mapper();
        }
    }
}
