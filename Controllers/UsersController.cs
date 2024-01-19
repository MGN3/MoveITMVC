using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoveITMVC.Models;
using Newtonsoft.Json.Linq;

namespace MoveITMVC.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase {
		private readonly IConfiguration _configuration;
		private readonly MoveITDbContext _context;
		//private readonly UserManager<User> _userManager;  //Using Identity nuget implied adding new tables and migration of the db so I discarded that approach.

		public UsersController(IConfiguration configuration, MoveITDbContext context) {
			_configuration = configuration;
			_context = context;
		}

		// GET: api/Users
		[HttpGet]
		public async Task<ActionResult<IEnumerable<User>>> GetUsers() {
			return await _context.Users.ToListAsync();
		}

		// GET: api/Users/Auth/5
		[HttpGet("Auth/{id}")]
		[Authorize]
		public async Task<ActionResult<User>> GetUser(Guid id) {
			//This user contains all the attributes. It includes the navigating attributes like addressess, orders and shoppingCart
			var user = await _context.Users.FindAsync(id);

			//This line aimed to create the user without navigating attributes. Couldn't achieve it. However, it might be important in other scenarios.
			//This line alongside other strategies might ichive it.
			//var user1 = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);

			if (user == null) {
				return NotFound();
			}

			//For this endpoint we need only some specific attributes of the given user.
			User myAccountUser = new User(user.Name, user.Email, user.Nickname, user.Gender, user.ProfilePictureUrl, user.PhoneNumber);

			return myAccountUser;
		}

		// PUT: 
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPut("Auth/Update/{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateUser(Guid id) {
			try {
				var existingUser = await _context.Users.FindAsync(id);

				if (existingUser == null) {
					return NotFound();
				}

				using (var reader = new StreamReader(Request.Body)) {
					var requestBody = await reader.ReadToEndAsync();

					// Deserialize JSON
					using JsonDocument document = JsonDocument.Parse(requestBody);

					UpdatePropertyIfExists(document, "name", value => existingUser.Name = value);
					UpdatePropertyIfExists(document, "email", value => existingUser.Email = value);
					UpdatePropertyIfExists(document, "nickname", value => existingUser.Nickname = value);

					// Extra logic for gender Enum
					UpdatePropertyIfExists(document, "gender", value => {
						if (Enum.TryParse<Gender>(value, out var gender)) {
							existingUser.Gender = gender;
						}
					});

					UpdatePropertyIfExists(document, "profilePictureUrl", value => existingUser.ProfilePictureUrl = value);
					UpdatePropertyIfExists(document, "phoneNumber", value => existingUser.PhoneNumber = value);

					await _context.SaveChangesAsync();
				}

				return NoContent();

			} catch (JsonException) {
				return BadRequest("Invalid JSON format");
			} catch (DbUpdateConcurrencyException) {
				return StatusCode(500, "Concurrency issue occurred");
			} catch (Exception ex) {
				return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
			}
		}

		private void UpdatePropertyIfExists(JsonDocument document, string propertyName, Action<string> updateAction) {
			if (document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(property.GetString())) {
				updateAction(property.GetString());
			}
		}


		// DELETE: api/Users/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(Guid id) {
			var user = await _context.Users.FindAsync(id);
			if (user == null) {
				return NotFound();
			}

			_context.Users.Remove(user);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		/*
		///////////////////// AUXILIAR METHODS /////////////////////
		*/
		private bool UserExists(Guid id) {
			return _context.Users.Any(e => e.UserId == id);
		}

		private bool EmailExists(string email) {
			return _context.Users.Any(e => email == e.Email);
		}

		// Getting Id from JWT
		private Guid ExtractUserIdFromToken(string jwt) {
			//var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

			//Add more logic when validating Token.
			// Validate and deocede token to get claims(user info inside token)
			var handler = new JwtSecurityTokenHandler();
			var tokenSesion = handler.ReadJwtToken(jwt);

			// Get data from token claims: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{emailaddress} or {name} or {nameidentifier}
			var userId = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value; // Stored as nameidentifier

			//var userEmail = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;  // Stored as emailaddress
			//var userName = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value; // Stored as name

			return new Guid(userId);
		}
		/*
		///////////////////// AUXILIAR METHODS /////////////////////
		*/

		[HttpPost("Register")]
		public async Task<IActionResult> Register(User newUser) {
			try {
				// Is email availeable?
				var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);


				if (existingUser != null) {
					return BadRequest("Email not availeable");
				}

				// Encypting user password
				string hashedPassword = HashPassword(newUser.Password);

				// New user with user input and predefined fields.
				User insertUser = new User {
					UserId = Guid.NewGuid(), // random
					Name = newUser.Name, //from input
					Email = newUser.Email, //from input
					Password = hashedPassword, // From input. Encrypted
					Nickname = newUser.Nickname, // From seed
					Gender = newUser.Gender, // From seed
					ProfilePictureUrl = newUser.ProfilePictureUrl, // From seed
					PhoneNumber = newUser.PhoneNumber // From seed
				};

				// INSERT INTO Users table.
				_context.Users.Add(insertUser);
				await _context.SaveChangesAsync();

				return Ok("User registered successfully");
			} catch (Exception ex) {
				// Manage sceptions
				return StatusCode(500, "Error trying to register a new user");
			}
		}

		[HttpPost("Authenticate")]
		public async Task<IActionResult> Authenticate([FromBody] ClientCredentials credentials) {
			string emailClientInput = credentials.Email; //Upper case vs lower case in client js object?? check consistency
			string passwordClientInput = credentials.Password;

			// Checking inputs. Add more logic or use an auxiliar function.
			if (string.IsNullOrEmpty(emailClientInput) || string.IsNullOrEmpty(passwordClientInput)) {
				return BadRequest("Invalid username or password");
			}

			// If input ok, find email.
			User userAuthenticating = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClientInput);

			// User doesn't exist:
			if (userAuthenticating == null) {
				return NotFound("Email not registered");
			}

			// User exists -> Hash input password before comparing it with the hashed stored password for the given Email
			string hashedPasswordEntered = HashPassword(passwordClientInput);

			if (hashedPasswordEntered != userAuthenticating.Password) {
				return Unauthorized("Wrong password");
			}

			// If comparison is ok, generate a new token with User object.
			var token = JwtGenerator(userAuthenticating);
			//send only as ok(token)? CHECK THIS to make it better looking.
			return Ok(new { Token = token });
		}


		/*
		 ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		 /////////////////     Function 1: Hash password when registeting. Function 2: Generate JWT     /////////////////

		 ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		 */
		// One of the keys here is creating a password that fits inside a NVARCHAR(64bytes).
		private string HashPassword(string password) {
			// SHA256 Object.
			using (SHA256 sha256 = SHA256.Create()) {
				byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

				// hash to hexadecimal string
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < hashedBytes.Length; i++) {
					builder.Append(hashedBytes[i].ToString("x2")); //x2 is key.
				}
				return builder.ToString();
			}
		}

		// JavaScript Web Token generator signed with a KEY, containing claims/info and other parameters like issuer, audience...
		private string JwtGenerator(User user) {
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Secrets").GetSection("JWT").Value));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			//Array of claims to add into the token
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Is ToString really necessary? check
				new Claim(ClaimTypes.Name, user.Name),
				new Claim(ClaimTypes.Email, user.Email)
				//adding many claims should be avoided.
			};

			//check this syntax, JavaScript what are you doing in C# XD..? 
			var token = new JwtSecurityToken(
				issuer: "MoveITMVC",
				audience: "project-forntend-developer",
				claims: claims,
				expires: DateTime.UtcNow.AddHours(2), // Token duration
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		/*
		 ///////////////////////////////////////////////////////////////////////////////////////////////////////////     
	
		 /////////////////     TODO: access database with UserId to retrieve relevant data.        ///////////////// 

		 ///////////////////////////////////////////////////////////////////////////////////////////////////////////   
		 */
		[HttpGet("UserInfo")]
		[Authorize] // [Authorize] to use Startup authentication helper
		public IActionResult GetUserInfo() {
			try {
				var userClaims = HttpContext.User.Claims; // Getting claims

				// Accessing claims to retrieve data inside the JWT
				var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var userEmail = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

				// Query DB

				// SAMPLE: data that is going to be sent.
				var userFromDb = new { UserId = userId, Email = userEmail, OtherInfo = "Some data from DB" };

				// Return data
				return Ok(userFromDb);
			} catch (Exception ex) {
				return StatusCode(500, "Error trying to fetch data");
			}
		}


		[HttpGet("Data")]
		[Authorize]
		public IActionResult GetDataFromUser() {
			try {
				// Get token in a different way, check this line...
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

				//Add more logic when validating Token.
				// Validate and deocede token to get claims(user info inside token)
				var handler = new JwtSecurityTokenHandler();
				var tokenSesion = handler.ReadJwtToken(token);

				// Get data from token claims: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{emailaddress} or {name} or {nameidentifier}
				var userId = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value; // Stored as nameidentifier
				var userEmail = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;  // Stored as emailaddress
				var userName = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value; // Stored as name

				//Instead of data from claims-> QUERY DATABASE once I have the UserId.

				// Return an IActionResult with the required data
				return Ok(new {
					Email = userEmail
				});

			} catch (Exception ex) {
				// Manage error
				return BadRequest("Error with the JWT or DB query");
			}
		}


		//REMAKE this for customized JWT authentication instead of using the [Authorize] notation + Startup config.
		//private ClaimsPrincipal ValidateJwtToken(string token, TokenValidationParameters validationParameters) {
		//	try {
		//		var tokenHandler = new JwtSecurityTokenHandler();
		//		SecurityToken validatedToken;
		//		var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
		//		return principal;
		//	} catch (Exception ex) {
		//		return null;
		//	}
		//}



		///[HttpGet("{id}")]
		//public async Task<IActionResult> GetUserHtml(Guid id) {
		//	var user = await _context.Users.FindAsync(id);

		//	if (user == null) {
		//		return NotFound();
		//	}

		//	string htmlContent = $"<h3>User details:</h3>" +
		//						 $"<p>Name: {user.Name}</p>" +
		//						 $"<p>Nombre: {user.Nickname}</p>";

		//	return Content(htmlContent, "text/html");
		//}


		/*COMO PUEDO TENER VARIOS ENDPOINTS PARA UNA MISMA ACCION?, COMO AÑADIRLE RUTA?=*/
		// POST: api/Users
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		//[HttpPost]
		//public async Task<ActionResult<User>> PostUser(User user) {
		//	_context.Users.Add(user);
		//	await _context.SaveChangesAsync();

		//	return CreatedAtAction("GetUser", new { id = user.UserId }, user);
		//}

		//[HttpPost]
		//public async Task<ActionResult<User>> PostUser2([FromBody] User user) {
		//	string connectionString = _configuration.GetConnectionString("cnMoveIT");

		//	using (SqlConnection connection = new SqlConnection(connectionString)) {
		//		string query = "INSERT INTO Users (UserId, Name, Email, Password, Nickname, Gender, ProfilePictureUrl, PhoneNumber) VALUES (@UserId, @Name, @Email, @Password, @Nickname, @Gender, @ProfilePictureUrl, @PhoneNumber)";

		//		SqlCommand command = new SqlCommand(query, connection);
		//		command.Parameters.AddWithValue("@UserId", Guid.NewGuid());
		//		command.Parameters.AddWithValue("@Name", user.Name);
		//		command.Parameters.AddWithValue("@Email", user.Email);
		//		command.Parameters.AddWithValue("@Password", user.Password);
		//		command.Parameters.AddWithValue("@Nickname", user.Nickname);
		//		command.Parameters.AddWithValue("@Gender", user.Gender);
		//		command.Parameters.AddWithValue("@ProfilePictureUrl", user.ProfilePictureUrl);
		//		command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);

		//		await connection.OpenAsync();
		//		await command.ExecuteNonQueryAsync();
		//	}

		//	return user;
		//}

	}
	// Class helper. Independent file?
	public class ClientCredentials {
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
