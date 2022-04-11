using System;

namespace Network{
	class User{
		private string name;
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		private Guid id;
		public Guid Id{
			get { return id; }
			set { id = value; }
		}
		public User(string _name){
			id = Guid.NewGuid();
			Name = _name;
		}

	
	}
}
