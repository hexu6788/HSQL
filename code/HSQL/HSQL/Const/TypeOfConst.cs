using HSQL.Attribute;
using System;
using System.Collections.Generic;

namespace HSQL.Const
{
    internal class TypeOfConst
    {
        internal static Type String = typeof(string);
        internal static Type Int = typeof(int);
        internal static Type UInt = typeof(uint);
        internal static Type Long = typeof(long);
        internal static Type Float = typeof(float);
        internal static Type Double = typeof(double);
        internal static Type Decimal = typeof(decimal);
        internal static Type DateTime = typeof(DateTime);
        internal static Type ByteArray = typeof(byte[]);

        internal static Type ListString = typeof(List<string>);
        internal static Type ListInt = typeof(List<int>);
        internal static Type ListLong = typeof(List<long>);
        internal static Type ListFloat = typeof(List<float>);
        internal static Type ListDouble = typeof(List<double>);
        internal static Type ListDecimal = typeof(List<decimal>);

        internal static Type TableAttribute = typeof(TableAttribute);
        internal static Type ColumnAttribute = typeof(ColumnAttribute);
    }
}
