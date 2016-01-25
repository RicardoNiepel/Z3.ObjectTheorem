namespace Z3.ObjectTheorem.EF.Metamodel
{
    public class ValueField : Field
    {
        public DatabaseGeneratedAttribute DatabaseGeneratedAttribute { get; set; }
        public bool HasIndexAttribute { get; set; }
        public bool HasKeyAttribute { get; set; }
        public bool HasMaxLengthAttribute { get; set; }
        public bool IsPrimaryKey { get; set; }
        public ValueType Type { get; set; }

        public ReferenceType ForeignType { get; set; }
    }
}