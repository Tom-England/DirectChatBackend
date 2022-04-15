using System;
using Microsoft.Data.Sqlite;

namespace Storage
{
	class DatabaseHandler{
		string db_connection_str = "Data Source=data.db";
		SqliteConnection db_connection;

		public void create(){
			db_connection = new SqliteConnection(db_connection_str);
		}
		public void setup(){
			
			string messages = @"CREATE TABLE messages
			(
				message_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
				message_text CHAR(" + Network.Constants.MESSAGE_SIZE + @") NOT NULL,
				sender_id INT NOT NULL,
				FOREIGN KEY(sender_id) REFERENCES users(user_id)
			);";
			
			string users = @"CREATE TABLE users
			(
				user_id VARCHAR(40) PRIMARY KEY NOT NULL,
				user_name VARCHAR(20)
			);";

			string account = @"CREATE TABLE account
			(
			 	account_id VARCHAR(40) PRIMARY KEY NOT NULL,
				password_hash CHAR(20)
			);";
			run_command(messages);
			run_command(users);
			run_command(account);
		}

		public void connect(){
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
		
		public void register(Guid id) {
			string s_id = id.ToString();
			string sql = "INSERT INTO account(account_id) VALUES ('"+s_id+"');";
			run_command(sql);
		}

		public Guid get_account_id(){
			string sql = "SELECT account_id FROM account";
			SqliteCommand command = db_connection.CreateCommand();
			command.CommandText = sql;
			SqliteDataReader reader = command.ExecuteReader();
			Guid id = Guid.Empty;
			if (reader.HasRows) {
				while (reader.Read()){
					id = Guid.Parse(reader["account_id"].ToString());
				}
			}
			return id;
		}
		public bool user_exists(Guid id){
			string s_id = id.ToString();
			string sql = "SELECT COUNT(*) FROM users WHERE user_id = '"+s_id+"'";
			SqliteCommand command = db_connection.CreateCommand();
			command.CommandText = sql;
			SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read()){
				if (reader["COUNT(*)"].ToString() != "0") {
					return true;
				}
			}
			return false;
		}
		public void add_user(string username, Guid id){
			string s_id = id.ToString();
			string sql = "INSERT INTO users(user_id, user_name) VALUES ('"+s_id+"', '"+username+"')";
			run_command(sql);
		}

		public void add_message(string message, Guid id){
			string s_id = id.ToString();
			string sql = "INSERT INTO messages(message_text, sender_id) VALUES ('"+message+"', '"+s_id+"')";
			run_command(sql);
		}

		public void get_all_users(){

			string sql = "select * from users";
			SqliteCommand command = db_connection.CreateCommand();
			command.CommandText = sql;
			SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read()){
				Console.WriteLine("Name: " + reader["user_name"] + "\tID: " + reader["user_id"]);
			}
		}

		public void get_all_messages_from_user(Guid id){
			string s_id = id.ToString();
			string sql = "SELECT * FROM messages WHERE sender_id='"+s_id+"'";
			SqliteCommand command = db_connection.CreateCommand();
			command.CommandText = sql;
			SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read()){
				Console.WriteLine(id + " : " + reader["message_text"]);
			}
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
