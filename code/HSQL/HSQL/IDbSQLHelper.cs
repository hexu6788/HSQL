using HSQL.Model;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace HSQL
{
    public interface IDbSQLHelper
    {
        int ExecuteNonQuery(bool isNewConnection, bool consolePrintSql, string commandText, params IDbDataParameter[] parameters);
        object ExecuteScalar(bool consolePrintSql, string commandText, params IDbDataParameter[] parameters);
        List<dynamic> ExecuteList(bool consolePrintSql, string commandText, params IDbDataParameter[] parameters);
        List<T> ExecuteList<T>(bool consolePrintSql, string commandText, params IDbDataParameter[] parameters);
        IDbDataParameter[] Convert(List<Parameter> parameters);
    }
}
