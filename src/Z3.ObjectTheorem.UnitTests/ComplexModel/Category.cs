using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z3.ObjectTheorem.UnitTests.ComplexModel
{
    public class Category
    {
        public IEnumerable<Product> Products { get; set; }

        public bool IsHighlighted { get; set; }
    }
}
