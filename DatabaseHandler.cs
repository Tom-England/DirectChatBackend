using System;
using System.Data.SQLite;

namespace Storage
{
	class DatabaseHandler{
		SQLiteConnection db_connection;
		public void setup(){
			SQLiteConnection.CreateFile("MyDatabase.sqlite");
			string setup = @"CREATE TABLE messages
			(
				message_id INT PRIMARY KEY NOT NULL,
				message_text CHAR(" + Network.Constants.MESSAGE_SIZE + @") NOT NULL,
				sender_id INT NOT NULL,
				FOREIGN KEY(sender_id) REFERENCES users(user_id)
			)
			
			CREATE TABLE users
			(
				user_id INT PRIMARY KEY NOT NULL,
				user_name VARCHAR(20)
			)

			CREATE TABLE password
			(
				password_hash CHAR(20);
			)
			
			)";
		}

		public void connect(){
			db_connection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
			db_connection.Open();
		}

		public void disconnect(){
			db_connection.Close();
		}
	}
}