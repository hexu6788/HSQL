using HSQL.Model;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace HSQL
{
    public interface IDbSQLHelper
    {
        int ExecuteNonQuery(bool isNewConnection, string commandText, params IDbDataParameter[] parameters);
        object ExecuteScalar(string commandText, params IDbDataParameter[] parameters);
        List<dynamic> ExecuteList(string commandText, params IDbDataParameter[] parameters);
        List<T> ExecuteList<T>(string commandText, params IDbDataParameter[] parameters);
        IDbDataParameter[] Convert(List<Parameter> parameters);
    }
}
