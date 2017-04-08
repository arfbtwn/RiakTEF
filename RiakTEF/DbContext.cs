using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using RiakClient;
using RiakClient.Commands;
using RiakClient.Commands.TS;

using RiakTEF.Low;
using RiakTEF.Serialization;

namespace RiakTEF
{
    using TableMap = ConcurrentDictionary<IEntity, ITable>;

    /// <summary>
    /// Exception throwing access to some common client commands
    /// </summary>
    public interface IDatabase
    {
        IRiakClient Client { get; }

        RiakResult Query  (Query command);
        RiakResult Store  (Store command);
        RiakResult Execute(IRiakCommand command);

        Task<RiakResult> QueryAsync  (Query command);
        Task<RiakResult> StoreAsync  (Store command);
        Task<RiakResult> ExecuteAsync(IRiakCommand command);
    }

    [Flags]
    public enum State
    {
        Added     = 1 << 0,
        Updated   = 1 << 1,
        Deleted   = 1 << 2,
        Transient = Added | Deleted
    }

    /// <summary>
    /// A component that records actions we can defer
    /// for batching purposes
    /// </summary>
    public interface IUnit : IEnumerable<Tuple<State, object>>
    {
        State this[object key] { get; set; }

        void Flush();
    }

    /// <summary>
    /// A collection of serializers
    /// </summary>
    public interface ISerializers : IEnumerable<IColumnSerializer>
    {
        void Add(IColumnSerializer serializer);

        IRowSerializer Get(IEntity entity);
    }

    /// <summary>
    /// An object that initializes the database and aligns
    /// the object model
    /// </summary>
    public interface IInitializer
    {
        void Initialize(DbContext context);
    }

    /// <summary>
    /// Base class for data context access
    /// </summary>
    public abstract class DbContext : IDisposable
    {
        static IInitializer _Initializer = Default.Initializer;
        static ISerializers _Serializers = Default.Serializers;

        public static void SetInitializer(IInitializer initializer) => _Initializer = initializer;
        public static void SetSerializers(ISerializers serializers) => _Serializers = serializers;

        bool _initialized;

        public ISchema      Schema      { get; }
        public IDatabase    Database    { get; }
        public TableMap     Tables      { get; } = new TableMap();

        public IInitializer Initializer { get; set; } = _Initializer;
        public ISerializers Serializers { get; set; } = _Serializers;
        public IUnit        Unit        { get; set; } = new _Unit();


        protected DbContext(IRiakClient client)
        {
            Database = new Database(client);
            Schema   = RiakTEF.Schema.Create();
        }

        void Initialize()
        {
            if (_initialized) return;

            lock(this)
            {
                if (_initialized) return;

                Initialize(Schema);

                Initializer?.Initialize(this);

                _initialized = true;
            }
        }

        protected abstract void Initialize(ISchema schema);

        Batcher Batch()
        {
            var commands = new Batcher(Tables, Serializers);

            if (null == Unit)
            {
                return commands;
            }

            foreach (var pair in Unit)
            {
                commands.Add(pair.Item1, pair.Item2);
            }

            Unit.Flush();

            return commands;
        }

        public virtual void Save()
        {
            ThrowIfDisposed();

            var batch = Batch();

            foreach (var cmd in batch.Store)
            {
                Database.Store(cmd);
            }

            foreach (var cmd in batch.Delete)
            {
                Database.Execute(cmd);
            }
        }

        public virtual async Task SaveAsync()
        {
            ThrowIfDisposed();

            var batch = Batch();

            foreach (var cmd in batch.Delete)
            {
                await Database.ExecuteAsync(cmd).ConfigureAwait(false);
            }

            foreach (var cmd in batch.Store)
            {
                await Database.StoreAsync(cmd).ConfigureAwait(false);
            }
        }

        protected virtual DbSet<T> Set<T>(ITable table)
        {
            return new DbSet<T>(table, Unit);
        }

        ITable _Table(IEntity entity)
        {
            var serial = Serializers.Get(entity);
            return new Table(Database, entity, serial);
        }

        public DbSet<T> Set<T>()
        {
            ThrowIfDisposed();

            Initialize();

            var type   = typeof(T);
            var entity = Schema.Entities.Single(type);
            var table  = Tables.GetOrAdd(entity, _Table);

            return Set<T>(table);
        }

        bool _disposed;

        ~DbContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)  return;
            if (!disposing) return;

            Save();

            _disposed = true;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
        }
    }
}
