using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiakTEF.Initializers
{
    public class Delegate : IInitializer
    {
        public Delegate() { }
        public Delegate(IInitializer inner)
        {
            Inner = inner;
        }

        public IInitializer Inner { get; set; }

        public virtual void Initialize(DbContext context)
        {
            Inner?.Initialize(context);
        }
    }
}
