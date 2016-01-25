using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z3.ObjectTheorem.EF.Metamodel
{
    public class SingleField : ReferenceField
    {
        public string ForeignKeyAttribute { get; set; }
    }
}
