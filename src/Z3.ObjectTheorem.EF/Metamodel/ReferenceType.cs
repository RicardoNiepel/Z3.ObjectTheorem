using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z3.ObjectTheorem.EF.Metamodel
{
    public class ReferenceType : Type
    {
        public bool IsEntity { get; set; }

        public bool HasComplexTypeAttribute { get; set; }

        public bool HasSingleFields { get; set; }
        public bool HasCollectionFields { get; set; }

        public IEnumerable<SingleField> SingleFields { get; set; }

        public IEnumerable<CollectionField> CollectionFields { get; set; }

        public IEnumerable<ValueField> ValueFields { get; set; }
    }
}
