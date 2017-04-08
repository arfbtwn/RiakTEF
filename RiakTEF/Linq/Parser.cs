using System.Linq.Expressions;

using RiakTEF.Visitors;

namespace RiakTEF.Linq
{
    class Parser
    {
        readonly string   _table;
        readonly IColumns _columns;

        public Parser(string table, IColumns columns)
        {
            _table   = table;
            _columns = columns;
        }

        public Parser(ITable table)   : this(table.Name, table.Entity.Columns) { }
        public Parser(IEntity schema) : this(schema.Table, schema.Columns) { }

        protected virtual Expression Syntax(Expression expression)
        {
            return new Syntax().Visit(expression);
        }

        protected virtual Expression Evaluate(Expression expression)
        {
            return Eval.Local(expression);
        }

        protected virtual Expression Validate(Expression expression)
        {
            return new Validator().Visit(expression);
        }

        public string Parse(Expression expression)
        {
            var tree = expression;

            tree = Evaluate(tree);
            tree = Syntax  (tree);
            tree = Evaluate(tree);
            tree = Validate(tree);

            return new Generator(_table, _columns).Generate(tree);
        }
    }
}