using Microsoft.Z3;

namespace Z3.ObjectTheorem.Solving
{
    internal class InstanceInfo
    {
        public InstanceInfo(Expr enumConstant, object objectInstance)
        {
            EnumConstant = enumConstant;
            ObjectInstance = objectInstance;
        }

        public Expr EnumConstant { get; private set; }

        public object ObjectInstance { get; private set; }
    }
}