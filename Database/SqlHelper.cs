using System;
using System.Data;
using System.Data.SqlClient;

namespace Code.Database
{
    /// <summary>
    /// Interface for the SQL Helper
    /// </summary>
    public interface ISqlHelper
    {
        /// <summary>
        /// Executes a non-query SQL command such as an INSERT or UPDATE command
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The optional parameters.</param>
        void ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] parameters);

        /// <summary>
        /// Executes the SQL command and returns the result from the first column and first row.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        object ExecuteScalar(CommandType commandType, string commandText, params SqlParameter[] parameters);

        /// <summary>
        /// Executes the SQL command and returns the result as a DataTable
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        DataTable ExecuteDataTable(CommandType commandType, string commandText, params SqlParameter[] parameters);
    }

    public class SqlHelper : ISqlHelper
    {
        private readonly string connectionString;

        public SqlHelper(string connectionString) : this(connectionString, typeof(SqlHelper).Name)
        {
        }

        public SqlHelper(string connectionString, string applicationName)
        {
            this.connectionString = connectionString;
            if (!connectionString.Contains("Application Name"))
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                builder.ApplicationName = applicationName;
                this.connectionString = builder.ConnectionString;
            }
        }

        /// <summary>
        /// Creates, initializes, and returns a <see cref="SqlCommand"/> instance.
        /// </summary>
        /// <param name="connection">The <see cref="SqlConnection"/> the <see cref="SqlCommand"/> should be executed on.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandText">The name of the stored procedure to execute.</param>
        /// <param name="parameters">The parameters of the stored procedure.</param>
        /// <returns>An initialized <see cref="SqlCommand"/> instance.</returns>
        private SqlCommand CreateCommand(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            if (connection != null && connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (parameters != null)
            {
                // Append each parameter to the command
                foreach (SqlParameter parameter in parameters)
                {
                    parameter.Value = CheckValue(parameter.Value);
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }


        /// <summary>
        /// Executes the stored procedure with the specified parameters.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandText">The stored procedure to execute.</param>
        /// <param name="parameters">The parameters of the stored procedure.</param>
        public void ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = CreateCommand(connection, commandType, commandText, parameters))
                {
                    command.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Executes the stored procedure with the specified parameters, and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandText">The stored procedure to execute.</param>
        /// <param name="parameters">The parameters of the stored procedure.</param>
        /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
        public object ExecuteScalar(CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = CreateCommand(connection, commandType, commandText, parameters))
                {
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Executes the stored procedure and returns the result as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandText">The stored procedure to execute.</param>
        /// <param name="parameters">The parameters of the stored procedure.</param>
        /// <returns>A <see cref="DataTable"/> containing the results of the stored procedure execution.</returns>
        public DataTable ExecuteDataTable(CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = CreateCommand(connection, commandType, commandText, parameters))
                {
                    return CreateDataTable(command);
                }
            }
        }

        private static object CheckValue(object value)
        {
            return value ?? DBNull.Value;
        }

        private static DataTable CreateDataTable(SqlCommand command)
        {
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
            {
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                return dataTable;
            }
        }
    }
}