using System;
using System.Data.SQLite;

namespace Storage
{
	class DatabaseHandler{
		SQLiteConnection db_connection;

		public void create(){
			SQLiteConnection.CreateFile("MyDatabase.sqlite");
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
			db_connection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
			db_connection.Open();
		}

		public void disconnect(){
			db_connection.Close();
		}

		public int run_command(string sql){
			SQLiteCommand command = new SQLiteCommand(sql, db_connection);
			return command.ExecuteNonQuery();
		}
		
		public void register(Guid id) {
			string s_id = id.ToString();
			string sql = "INSERT INTO account(account_id) VALUES ('"+s_id+"');";
			run_command(sql);
		}

		public Guid get_account_id(){
			string sql = "SELECT account_id FROM account";
			SQLiteCommand command = new SQLiteCommand(sql, db_connection);
			SQLiteDataReader reader = command.ExecuteReader();
			Guid id = Guid.Empty;
			if (reader.HasRows) {
				while (reader.Read()){
					id = Guid.Parse(reader["account_id"].ToString());
				}
			}
			return id;
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
			SQLiteCommand command = new SQLiteCommand(sql, db_connection);
			SQLiteDataReader reader = command.ExecuteReader();
			while (reader.Read()){
				Console.WriteLine("Name: " + reader["user_name"] + "\tID: " + reader["user_id"]);
			}
		}

		public void get_all_messages_from_user(Guid id){
			string s_id = id.ToString();
			string sql = "SELECT * FROM messages WHERE sender_id='"+s_id+"'";
			SQLiteCommand command = new SQLiteCommand(sql, db_connection);
			SQLiteDataReader reader = command.ExecuteReader();
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
