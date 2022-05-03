using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Storage
{
	public class DatabaseHandler{
		string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
		string db_connection_str;
		SqliteConnection db_connection;

		public void create(){
			db_connection_str = "Data Source=" + folder + "data.db";
			db_connection = new SqliteConnection(db_connection_str);
		}
		public void setup(){
			
			string messages = @"CREATE TABLE if not exists messages
			(
				message_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
				message_text CHAR(" + Network.Constants.MESSAGE_SIZE + @") NOT NULL,
				sender_id INT NOT NULL,
				FOREIGN KEY(sender_id) REFERENCES users(user_id)
			);";
			
			string users = @"CREATE TABLE if not exists users
			(
				user_id VARCHAR(40) PRIMARY KEY NOT NULL,
				user_key CHAR(32),
				user_name VARCHAR(20)
			);";

			string account = @"CREATE TABLE if not exists account
			(
			 	account_id VARCHAR(40) PRIMARY KEY NOT NULL,
				password_hash BINARY(256),
				key CHAR(32),
				iv CHAR(16)
			);";
			run_command(messages);
			run_command(users);
			run_command(account);
		}

		public void connect(){
			db_connection_str = "Data Source=" + folder + "/data.db";
			db_connection = new SqliteConnection(db_connection_str);
			db_connection.Open();
		}

		public void disconnect(){
			db_connection.Close();
		}

		public int run_command(string sql){
			SqliteCommand command = db_connection.CreateCommand();
			command.CommandText = sql;
			return command.ExecuteNonQuery();
		}

		public SqliteDataReader run_reader(string sql){
			SqliteCommand command = db_connection.CreateCommand();
			command.CommandText = sql;
			SqliteDataReader reader = command.ExecuteReader();
			return reader;
		}
		
		public void register(Guid id, byte[] key, byte[] iv) {
			string s_id = id.ToString();
			string s_key = Convert.ToBase64String(key);
			string s_iv = Convert.ToBase64String(iv);
			string sql = "INSERT INTO account(account_id, key, iv) VALUES ('"+s_id+"','"+s_key+"','"+s_iv+"');";
			run_command(sql);
		}

		public Guid get_account_id(){
			string sql = "SELECT account_id FROM account";
			SqliteDataReader reader = run_reader(sql);
			Guid id = Guid.Empty;
			if (reader.HasRows) {
				while (reader.Read()){
					id = Guid.Parse(reader["account_id"].ToString());
				}
			}
			return id;
		}

		public byte[] get_user_key(Guid id)
		{
			string sql = "SELECT user_key FROM users WHERE user_id='" + id.ToString() + "'";
			byte[] key = new byte[32];
			SqliteDataReader reader = run_reader(sql);
			if (reader.HasRows) {
				while (reader.Read())
				{
					key = Convert.FromBase64String(reader["user_key"].ToString());
				}
			}
			return key;
		}
		public byte[] get_key(){
			string sql = "SELECT key FROM account";
			SqliteDataReader reader = run_reader(sql);
			string s_key;
			byte[] key = new byte[32];
			if (reader.HasRows) {
				while (reader.Read()){
					s_key = reader["key"].ToString();
					key = Convert.FromBase64String(s_key);
				}
			}
			return key;
		}
		public byte[] get_iv(){
			string sql = "SELECT iv FROM account";
			SqliteDataReader reader = run_reader(sql);
			string s_iv;
			byte[] iv = new byte[16];
			if (reader.HasRows) {
				while (reader.Read()){
					s_iv = reader["iv"].ToString();
					iv = Convert.FromBase64String(s_iv);
				}
			}
			return iv;
		}
		public bool user_exists(Guid id){
			string s_id = id.ToString();
			string sql = "SELECT COUNT(*) FROM users WHERE user_id = '"+s_id+"'";
			SqliteDataReader reader = run_reader(sql);
			while (reader.Read()){
				if (reader["COUNT(*)"].ToString() != "0") {
					return true;
				}
			}
			return false;
		}
		public void add_user(string username, Guid id, byte[] key){
			string s_key = Convert.ToBase64String(key);
			string s_id = id.ToString();
			string sql = "INSERT INTO users(user_id, user_name, user_key) VALUES ('"+s_id+"', '"+username+"', '"+s_key+"')";
			run_command(sql);
		}

		public void add_message(string message, Guid id){
			string s_id = id.ToString();
			string sql = "INSERT INTO messages(message_text, sender_id) VALUES ('"+message+"', '"+s_id+"')";
			run_command(sql);
		}
		public List<Network.User.UserTransferable> get_all_users(){
            List<Network.User.UserTransferable> users = new List<Network.User.UserTransferable>();
            string sql = "select * from users";
			SqliteDataReader reader = run_reader(sql);
			while (reader.Read()){
				string name = reader["user_name"].ToString();
				Guid id = Guid.Parse(reader["user_id"].ToString());
				Console.WriteLine("Name: " + name + "\tID: " + id.ToString());
                users.Add(new Network.User.UserTransferable(name, id, new byte[32], new byte[16]));
            }
            return users;
        }

		public void print_all_messages(byte[] key, byte[] iv){
			string sql = "SELECT message_text, sender_id FROM messages;";
			SqliteDataReader reader = run_reader(sql);
			cryptography.CryptoHelper c = new cryptography.CryptoHelper();
			while (reader.Read()){
				string msg_str = reader["message_text"].ToString();
				byte[] msg = Convert.FromBase64String(msg_str);
				string text = c.decrypt(msg, key, iv);
				Console.WriteLine("{0} : {1}", reader["sender_id"], text);
			}
		}
		public List<Network.Message> get_all_messages_from_user(Guid id){
			string s_id = id.ToString();
			string sql = "SELECT * FROM messages WHERE sender_id='"+s_id+"'";
			SqliteCommand command = db_connection.CreateCommand();
			command.CommandText = sql;
			SqliteDataReader reader = command.ExecuteReader();
			List<Network.Message> messages = new List<Network.Message>();
			while (reader.Read()){
				Console.WriteLine(id + " : " + reader["message_text"]);
				byte[] msg = Convert.FromBase64String(reader["message_text"].ToString());
				messages.Add(new Network.Message(msg, Guid.Empty, Guid.Parse(reader["sender_id"].ToString())));
			}
			return messages;
		}
		
		public bool check_if_setup(){
			string sql = "SELECT COUNT(object_id) FROM sys.tables WHERE name = 'users';";
			SqliteDataReader reader = run_reader(sql);
			while (reader.Read()){
				if (reader["COUNT(object_id)"].ToString() != "0" ) {
					return true;
				}
			}
			return false;
		}

		public void test(){
			create();
			connect();
			setup();
			//add_user("Tom");
			//add_user("Steve");
			//add_message("Hello, World!", 1);
			//add_message("Im Steve", 2);
			Console.WriteLine("========Users========");
			get_all_users();
			Console.WriteLine("========Texts========");
			//get_all_messages_from_user(1);
			//get_all_messages_from_user(2);
			disconnect();
		}
	}
}
