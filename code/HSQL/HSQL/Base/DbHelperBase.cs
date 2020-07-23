using System.Data;

namespace HSQL.Base
{
    public class DbHelperBase
    {
        public int ExecuteNonQuery(IDbConnection connection,string commandText, params IDbDataParameter[] parameters)
        {
            int result = 0;
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                result = command.ExecuteNonQuery();
            }
            return result;
        }
    }
}
