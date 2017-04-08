namespace RiakTEF.Models
{
    public enum Order { Asc, Desc }

    public enum Nulls { First, Last }

    public struct Sort
    {
        public Sort(Order order, Nulls? nulls)
        {
            Order = order;
            Nulls = nulls;
        }

        public Sort(Order order) : this(order, null) { }

        public Order  Order { get; }
        public Nulls? Nulls { get; }

        string _Order => Order.ToString().ToUpper();
        string _Nulls => Nulls.ToString().ToUpper();

        public override string ToString()
        {
            return Nulls.HasValue ? _Order + " " + _Nulls : _Order;
        }
    }
}