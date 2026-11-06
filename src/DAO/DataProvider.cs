using System.Data;
using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace DAO
{
    public class DataProvider
    {
        // Singleton pattern
        private static DataProvider instance;
        public static DataProvider Instance
        {
            get
            {
                if (instance == null)
                    instance = new DataProvider();
                return instance;
            }
            private set { instance = value; }
        }

        private DataProvider() { }

        // 🔹 Kết nối từ App.config
        public static string connectionSTR
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["QUANLICHTL"].ConnectionString;
            }
        }


        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable data = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionSTR))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện truy vấn: " + ex.Message);
            }

            return data;
        }

        // Execute stored procedure and return DataTable
        public DataTable ExecuteStoredProcedure(string spName, SqlParameter[] parameters = null)
        {
            DataTable data = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionSTR))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện stored procedure: " + ex.Message);
            }

            return data;
        }

        // Execute stored procedure non-query (INSERT/UPDATE/DELETE)
        public int ExecuteNonQueryStoredProcedure(string spName, SqlParameter[] parameters = null)
        {
            int result = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionSTR))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        result = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện stored procedure: " + ex.Message);
            }

            return result;
        }

        // Execute stored procedure scalar (return single value)
        public object ExecuteScalarStoredProcedure(string spName, SqlParameter[] parameters = null)
        {
            object result = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionSTR))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        result = command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện stored procedure: " + ex.Message);
            }

            return result;
        }


        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            int result = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionSTR))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        result = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện truy vấn: " + ex.Message);
            }

            return result;
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            object result = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionSTR))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        result = command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện truy vấn: " + ex.Message);
            }

            return result;
        }


        public static DataTable SelectData(string sql, CommandType type, SqlParameter[] parameters = null)
        {
            DataTable result = new DataTable();

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(connectionSTR))
                {
                    sqlcon.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, sqlcon))
                    {
                        cmd.CommandType = type;
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi SelectData: " + ex.Message);
            }

            return result;
        }

        public DataTable ExecuteQuery(string query, object[] parameters)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            foreach (var obj in parameters)
                if (obj is SqlParameter p)
                    sqlParams.Add(p);

            return ExecuteQuery(query, sqlParams.ToArray());
        }

        public static DataSet SelectMultiData(string sql)
        {
            DataSet result = new DataSet();

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(connectionSTR))
                {
                    sqlcon.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(sql, sqlcon))
                    {
                        da.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi SelectMultiData: " + ex.Message);
            }

            return result;
        }


    }
}