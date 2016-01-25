using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z3.ObjectTheorem.EF.Metamodel
{
    public abstract class Field
    {
        public string Name { get; set; }

        public ReferenceType Owner { get; set; }

        public bool HasNotMappedAttribute { get; set; }
    }
}
