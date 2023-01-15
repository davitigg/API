using API.Models;
using System.Data.SqlClient;

namespace API.Services
{
    public class DBService
    {
        SqlConnection cnn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=testDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        SqlDataAdapter adapter = new SqlDataAdapter();

        public int Insert(SqlCommand cmd)
        {
            cmd.Connection = cnn;
            adapter.InsertCommand = cmd;
            cnn.Open();
            var returnValue = adapter.InsertCommand.ExecuteNonQuery();
            cnn.Close();
            return returnValue;
        }
        public bool IsRowSelected(SqlCommand cmd)
        {
            cmd.Connection = cnn;
            adapter.SelectCommand = cmd;
            cnn.Open();
            var reader = adapter.SelectCommand.ExecuteReader();
            var rows = reader.HasRows;
            cnn.Close();
            return rows;
        }
        public UserModel SelectUser(SqlCommand cmd)
        {
            cmd.Connection = cnn;
            adapter.SelectCommand = cmd;
            cnn.Open();
            var reader = adapter.SelectCommand.ExecuteReader();
            reader.Read();
            UserModel user = new UserModel(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5));
            cnn.Close();
            return user;
        }
        public int Update(SqlCommand cmd)
        {
            cmd.Connection = cnn;
            adapter.UpdateCommand = cmd;
            cnn.Open();
            var rowsAffected = adapter.UpdateCommand.ExecuteNonQuery();
            cnn.Close();
            return rowsAffected;
        }
        public int Delete(SqlCommand cmd)
        {
            cmd.Connection = cnn;
            adapter.DeleteCommand = cmd;
            cnn.Open();
            var rowsAffected = adapter.DeleteCommand.ExecuteNonQuery();
            cnn.Close();
            return rowsAffected;
        }

    }
}
