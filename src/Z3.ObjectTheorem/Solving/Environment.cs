using Microsoft.Z3;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Z3.ObjectTheorem.Solving
{
    internal class Environment
    {
        public Environment()
        {
            Instances = new Dictionary<string, InstanceInfo>();
            Types = new Dictionary<Type, Sort>();
            Members = new Dictionary<MemberInfo, FuncDecl>();
        }

        public IDictionary<string, InstanceInfo> Instances { get; private set; }

        public IDictionary<MemberInfo, FuncDecl> Members { get; private set; }

        public EnumSort PossibleStringValues { get; set; }

        public IDictionary<Type, Sort> Types { get; private set; }
    }
}