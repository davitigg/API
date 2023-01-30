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
        public User SelectUser(SqlCommand cmd)
        {
            cmd.Connection = cnn;
            adapter.SelectCommand = cmd;
            cnn.Open();
            var reader = adapter.SelectCommand.ExecuteReader();
            reader.Read();
            User user = new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4));
            cnn.Close();
            return user;
        }
        public List<Item> SelectItems()
        {
            SqlCommand cmd = new("SELECT * FROM items");
            cmd.Connection = cnn;
            adapter.SelectCommand = cmd;
            cnn.Open();
            var reader = adapter.SelectCommand.ExecuteReader();
            List<Item> items = new();
            while (reader.Read())
            {
                items.Add(new Item(reader.GetInt32(0), reader.GetString(1),
                    reader.GetInt32(2), reader.GetInt32(3)));
            }
            cnn.Close();
            return items;
        }
        public Item SelectItem(int itemId)
        {
            SqlCommand cmd = new("SELECT * FROM items Where items.id=@itemId");
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Connection = cnn;
            adapter.SelectCommand = cmd;
            cnn.Open();
            var reader = adapter.SelectCommand.ExecuteReader();
            reader.Read();
            var item = new Item(reader.GetInt32(0), reader.GetString(1),
                reader.GetInt32(2), reader.GetInt32(3));
            cnn.Close();
            return item;
        }
        public List<CartItem> SelectCartItems(int userId)
        {
            SqlCommand cmd = new("SELECT cart.id, cart.itemId, items.item_name, items.price, cart.quantity " +
                "FROM cart " +
                "INNER JOIN items ON cart.itemId=items.id " +
                "WHERE cart.userId=@userId " +
                "ORDER BY cart.id;");
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Connection = cnn;
            adapter.SelectCommand = cmd;
            cnn.Open();
            var reader = adapter.SelectCommand.ExecuteReader();
            List<CartItem> items = new();
            while (reader.Read())
            {
                items.Add(new CartItem(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), reader.GetInt32(4)));
            }
            cnn.Close();
            return items;
        }
        public CartItem SelectCartItem(int userId, int itemId)
        {
            SqlCommand cmd = new("SELECT cart.id, cart.itemId, items.item_name, items.price, cart.quantity " +
                "FROM cart " +
                "INNER JOIN items ON cart.itemId=items.id " +
                "WHERE cart.userId=@userId AND cart.itemId=@itemId;");
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Connection = cnn;
            adapter.SelectCommand = cmd;
            cnn.Open();
            try
            {
                var reader = adapter.SelectCommand.ExecuteReader();
                reader.Read();
                CartItem item = new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), reader.GetInt32(4));
                return item;
            }
            catch (Exception)
            {
                return null;
            }
            finally { cnn.Close(); }
        }
        public int InsertCartItem(int userId, int itemId, int quantity)
        {
            SqlCommand cmd = new("INSERT INTO users (email, password, first_name, last_name) Output Inserted.id " +
                       "VALUES (N'test', N'test', N'test', N'test')");
            cmd.Connection = cnn;
            cnn.Open();
            var returnValue = cmd.ExecuteNonQuery();
            cnn.Close();
            return returnValue;
        }
    }
}
