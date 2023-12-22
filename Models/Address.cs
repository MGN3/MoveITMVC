namespace MoveITMVC.Models {
	public class Address {
		public Guid AddressId { get; set; }
		public string Street { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public bool IsShippingAddress { get; set; } = true;

		// User relation
		public Guid UserId { get; set; }
		//The utility of this attribute might be for "navigation" through the data.
		public User User { get; set; } //Add '?', the database can't store this object.


		//Dictionary to set a country code based on user county input
		private static readonly Dictionary<string, string> CountryCodes = new Dictionary<string, string>
	   {
			{ "Estados Unidos", "US" },
			{ "United States", "US" },
			{ "USA", "US" },
			{ "Canadá", "CA" },
			{ "Canada", "CA" },
			{ "España", "ES" },
			{ "Spain", "ES" },
			{ "Alemania", "DE" },
			{ "Germany", "DE" },
			{ "Francia", "FR" },
			{ "France", "FR" },
			{ "Italia", "IT" },
			{ "Italy", "IT" },
			//Add other languages for the same country
		};
		//Create empty constructor for EF
		public Address() {
		}

		public Address(Guid _addressId, string _street, string _city, string _region, string _postalCode, string _country, bool _isShippingAddress, Guid _userId, User _user) {
			AddressId = _addressId;
			Street = _street;
			City = _city;
			Region = _region;
			PostalCode = _postalCode;
			Country = _country;
			IsShippingAddress = _isShippingAddress;
			UserId = _userId;
			User = _user;
		}

		//Constructor without the User object, state the attribute as nullable with '?' ?
		public Address(Guid _addressId, string _street, string _city, string _region, string _postalCode, string _country, bool _isShippingAddress, Guid _userId) {
			AddressId = _addressId;
			Street = _street;
			City = _city;
			Region = _region;
			PostalCode = _postalCode;
			Country = _country;
			IsShippingAddress = _isShippingAddress;
			UserId = _userId;
		}

		// Method to set the ISO-3166 country code that matches the country input string in the CountryCodes dictionary
		private string GetCountryCode(string countryName) {
			string countryCode;
			countryName = countryName.ToLower().Trim(); // Clean the data before trying to find a match.

			if (!CountryCodes.TryGetValue(countryName, out countryCode)) { //The data structure dictionary has this "weird" method with an out keyword
				return countryCode;
			} else {
				//In case the country is not found, rethink if "XX" is the best thing that can be return.
				return "XX";
			}
		}
		// Validating country postal/zip code.
		public bool ValidatePostalCode() {
			bool isMatch;

			if (string.IsNullOrEmpty(Country) || string.IsNullOrEmpty(PostalCode)) {
				return false; //In case the inputs are empty
			}

			//Get the ISO-3166 code of the country(US, ES...)
			string countryCode = GetCountryCode(Country);

			// Depending on the country code, check if the postal code matches the format.
			// Is regex the best way to check this????
			switch (countryCode) {
				case "US":
				isMatch = System.Text.RegularExpressions.Regex.IsMatch(PostalCode, @"^\d{5}(?:[-\s]\d{4})?$"); // 12345-1234
				break;
				case "CA":
				isMatch = System.Text.RegularExpressions.Regex.IsMatch(PostalCode, @"^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$"); // M5V 3L9
				break;
				//For many EU countries the format is the same:
				case "ES": 
				case "DE": 
				case "FR":
				case "IT": 
				isMatch = System.Text.RegularExpressions.Regex.IsMatch(PostalCode, @"^\d{5}$"); // 45510
				break;
				default:
				isMatch = false;
				break;
			}
			return isMatch;
		}
	}

}