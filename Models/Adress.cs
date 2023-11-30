namespace MoveITMVC.Models {
	public class Address {
		public Guid AddressId { get; set; }
		public string Street { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public bool IsShippingAddress { get; set; }

		// User relation
		public Guid UserId { get; set; }
		public User User { get; set; }

		//Create empty constructor
	}

}