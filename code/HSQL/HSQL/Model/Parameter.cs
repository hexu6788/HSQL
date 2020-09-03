using System.Data;

namespace HSQL.Model
{
    public class Parameter
    {
        public Parameter(string parameterName, object value)
        {
            ParameterName = parameterName;
            Value = value;
        }

        public object Value { get; set; }
        public string ParameterName { get; set; }

        public byte Precision { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public byte Scale { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int Size { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public DbType DbType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ParameterDirection Direction { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool IsNullable => throw new System.NotImplementedException();

        public string SourceColumn { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public DataRowVersion SourceVersion { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
