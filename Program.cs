using Network;
using System;
using System.Collections.Generic;

namespace Network{
    class MainClient{
        
        public static void Main(String[] args){
            if (args[0] == "client"){
                
                Client c = new Client();
                c.run_client();
                
            } else if (args[0] == "server"){
                Middleman mm = new Middleman();
                mm.listen();
            } else if (args[0] == "dbtest"){
				Storage.DatabaseHandler dbh = new Storage.DatabaseHandler();
				dbh.test();
			}
        }
    }
}