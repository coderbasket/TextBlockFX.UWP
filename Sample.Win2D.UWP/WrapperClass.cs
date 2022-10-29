using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextBlockFX.Win2D.UWP;

namespace Sample.Win2D.UWP
{
    public class ComboWrapper<T>
    {
        public string Name { get; }

        public T Value { get; }

        public ComboWrapper(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }

    public class BuiltInEffect
    {
        public string Name { get; }

        public ITextEffectBase Effect { get; }

        public BuiltInEffect(string name, ITextEffectBase effect)
        {
            Name = name;
            Effect = effect;
        }
    }
}
