﻿using System.Collections;
using MySql.Data.MySqlClient;

namespace TriggerMe_Bot
{
    class SqlClient
    {
        MySqlConnection _client;

        public SqlClient()
        {
            string connectionString = $"Server={Properties.Resources.sql_url};" +
                $"Database={Properties.Resources.sql_db};" +
                $"Uid={Properties.Resources.sql_name};" +
                $"Pwd={Properties.Resources.sql_pw};";

            _client = new MySqlConnection(connectionString);

        }

        public ArrayList PutCommand(string pCommand)
        {
            ArrayList output = new ArrayList();
            MySqlCommand command = _client.CreateCommand();
            command.CommandText = pCommand;
            MySqlDataReader reader;

            if (_client.State == System.Data.ConnectionState.Open)
                _client.Close();
            
            _client.Open();
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                string row = "";
                for (int i = 0; i < reader.FieldCount; i++)
                    row += reader.GetValue(i).ToString() + ";";
                output.Add(row);
            }
            _client.Close();

            return output;
        }
    }
}
