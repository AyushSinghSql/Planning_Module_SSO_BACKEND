using Microsoft.EntityFrameworkCore;

public class SqlHelper
{
    private readonly DbContext _context;

    public SqlHelper(DbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
    {
        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.CommandType = System.Data.CommandType.Text;

        if (command.Connection.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        // Add parameters if any
        if (parameters != null && parameters.Length > 0)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
        }

        var result = await command.ExecuteScalarAsync();
        return (result == null || result == DBNull.Value) ? default : (T)Convert.ChangeType(result, typeof(T));
    }
}
