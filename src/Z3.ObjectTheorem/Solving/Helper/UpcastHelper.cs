using Microsoft.Z3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z3.ObjectTheorem.Solving.Helper
{
    public static class UpcastHelper
    {
        public static Expr Upcast(Context context, Expr objectForMemberAccess, Sort memberDeclaredObjectSort)
        {
            FuncDecl upcast = ((DatatypeSort)memberDeclaredObjectSort).Constructors
                .SingleOrDefault(c => c.Domain[0] == objectForMemberAccess.Sort);

            objectForMemberAccess = context.MkApp(upcast, objectForMemberAccess);
            return objectForMemberAccess;
        }
    }
}
