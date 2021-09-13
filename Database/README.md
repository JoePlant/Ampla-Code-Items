# Database #

This code will allow for calls to the database to managed easily.

## Purpose ##

To provide a simple interface to access SQL database

## How to use it ##

Copy the code from [SQLHelper.cs](SqlHelper.cs) to a code item.

## Interface ##

```CSharp

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
```

## Examples ##

```CSharp

 using (ISqlHelper sqlHelper = new SqlHelper(connectionString, "Example Code")
 {
        string sql = "Select Sum([Value]) from dbo.ExampleTable where Code = @code";
        SqlParameter codeParam = new SqlParameter("code", "123");
        return (double)sqlHelper.ExecuteScalar(CommandType.Text, sql, codeParam);
 }

```
