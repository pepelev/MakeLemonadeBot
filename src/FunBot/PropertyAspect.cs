using System;
using System.Collections.Generic;

namespace FunBot
{
    public sealed class PropertyAspect<Item, Property> : Aspect<Item>
    {
        private readonly string name;
        private readonly Func<Item, Property> access;
        private readonly IEqualityComparer<Property> equality;

        public PropertyAspect(string name, Func<Item, Property> access)
            : this(name, access, EqualityComparer<Property>.Default)
        {
        }

        public PropertyAspect(string name, Func<Item, Property> access, IEqualityComparer<Property> equality)
        {
            this.name = name;
            this.access = access;
            this.equality = equality;
        }

        public override bool Changed(Item old, Item @new) => !equality.Equals(
            access(old),
            access(@new)
        );

        public override string Print(Item old, Item @new) => $"{name} changed from {access(old)} to {access(@new)}";
    }
}