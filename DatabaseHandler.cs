using System;
using System.Data.SQLite;

namespace Storage
{
	class DatabaseHandler{
		SQLiteConnection db_connection;
		public void setup(){
			SQLiteConnection.CreateFile("MyDatabase.sqlite");
			string setup = "CREATE TABLE messages";
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