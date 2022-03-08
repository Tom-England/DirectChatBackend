namespace Network{
    class Middleman{

        Listener listener;
        List<Message> message_stack = new List<Message>();
        public Middleman(){
            listener = new Listener();
        }

        public void test_msg(String test){
            
        }
        public void listen(){
            listener.create_server();
            bool running = true;
            while (running) {
                message_stack.Add(listener.get_message());
                
            }

            foreach (Message m in message_stack){
                Console.WriteLine(m.text);
            }
        }
    }
}