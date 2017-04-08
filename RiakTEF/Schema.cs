using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RiakClient.Commands.TS;

using RiakTEF.Models;

namespace RiakTEF
{
    using IEntities = IEnumerable<IEntity>;
    using IPaths    = IEnumerable<Path>;

    public static class Schema
    {
        public static ISchema Create() => new _Schema();
    }

    public interface ISchemaWriter
    {
        string Write(ISchema schema);
        string Write(IEntity entity);
    }

    public interface ISchema
    {
        IEntities  Entities { get; }

        IEntity<T> Entity<T>(string table = null);
    }

    public interface IColumns : IReadOnlyCollection<IColumn>
    {
        IColumn this[Path path] { get; }
    }

    public interface IPrimaryKey
    {
        IPaths Local     { get; }
        IPaths Partition { get; }
    }

    public interface IPrimaryKey<T> : IPrimaryKey
    {
        new IPrimaryKey<T> Local<V>    (Expression<Func<T, V>> expr);
        new IPrimaryKey<T> Partition<V>(Expression<Func<T, V>> expr);
    }

    public interface IEntity
    {
        IColumns    Columns { get; }
        IPrimaryKey Key     { get; }

        Type   Type  { get; }
        string Table { get; }

        void Add(IColumn column);
    }

    public interface IEntity<T> : IEntity
    {
        new IPrimaryKey<T> Key { get; }

        new IEntity<T> Table(string name);

        void Ignore<V>(Expression<Func<T, V>> expr);
    }

    public interface IColumn
    {
        string     Name     { get; }
        ColumnType Type     { get; }
        bool       Nullable { get; }

        Sort?      Sort     { get; }
        Quantum?   Quantum  { get; }

        Type       Source   { get; }
        Type       Target   { get; }
        Path       Path     { get; }
    }

    public interface IFluent
    {
        void Name    (string name);
        void Nullable(bool nullable = true);
    }

    public interface ISortable
    {
        void Sort(Sort order);
    }

    public interface ITimestamp
    {
        void Quantum(Quantum quantum);
    }
}
