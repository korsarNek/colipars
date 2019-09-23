using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Test
{
    class Wrapper : IEquatable<Wrapper>
    {
        public int number;

        public bool Equals(Wrapper other)
        {
            if (other == null)
                return false;

            return other.number == number;
        }

        public override bool Equals(object obj)
        {
            if (obj is Wrapper wrapper)
                return Equals(wrapper);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return number;
        }

        public override string ToString()
        {
            return number.ToString();
        }
    }

    class WrapperConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string text)
            {
                return new Wrapper() { number = int.Parse(text) };
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
