using System.ComponentModel.DataAnnotations;

namespace MoveITMVC.Models {
	public class User : IUser {
		public Guid UserId { get; set; }
		public string Name { get; set; }
		[EmailAddress]
		public string Email { get; set; }
		public string Password { get; set; }

		// New attributes
		public string Nickname { get; set; }
		public Gender Gender { get; set; }
		public string ProfilePictureUrl { get; set; }
		public string PhoneNumber { get; set; }

		// Relations*
		public List<Address> Addresses { get; set; }
		public List<Order> Orders { get; set; }
		public ShoppingCart ShoppingCart { get; set; } // Relación 1 a 1

		//Entity framework needs a constructor without parameters
		public User() {
		}
		//Remake the constructor for new properties.
		public User(Guid _userId, string _name, string _email, string _password) {
			UserId = _userId;
			Name = _name;
			Email = _email;
			Password = _password;
		}
	}


	public enum Gender {
		Male,
		Female,
		NonBinary,
		NonSpecified
	}
}
