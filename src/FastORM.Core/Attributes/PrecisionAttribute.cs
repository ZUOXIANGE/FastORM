using System;

namespace FastORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrecisionAttribute : Attribute
    {
        public int Precision { get; }
        public int Scale { get; }

        public PrecisionAttribute(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }
    }
}
