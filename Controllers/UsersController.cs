using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoveITMVC.Models;

namespace MoveITMVC.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase {
		private readonly IConfiguration _configuration;
		private readonly MoveITDbContext _context;
		//private readonly UserManager<User> _userManager;  //Using Identity nuget implied adding new tables and migration of the db.

		public UsersController(IConfiguration configuration, MoveITDbContext context) {
			_configuration = configuration;
			_context = context;
		}

		// GET: api/Users
		[HttpGet]
		public async Task<ActionResult<IEnumerable<User>>> GetUsers() {
			return await _context.Users.ToListAsync();
		}

		// GET: api/Users/5
		[HttpGet("{id}")]
		public async Task<ActionResult<User>> GetUser(Guid id) {
			var user = await _context.Users.FindAsync(id);

			if (user == null) {
				return NotFound();
			}

			return user;
		}

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

		// PUT: api/Users/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPut("{id}")]
		public async Task<IActionResult> PutUser(Guid id, User user) {
			if (id != user.UserId) {
				return BadRequest();
			}

			_context.Entry(user).State = EntityState.Modified;

			try {
				await _context.SaveChangesAsync();
			} catch (DbUpdateConcurrencyException) {
				if (!UserExists(id)) {
					return NotFound();
				} else {
					throw;
				}
			}

			return NoContent();
		}

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

		private bool UserExists(Guid id) {
			return _context.Users.Any(e => e.UserId == id);
		}


		[HttpPost("Register")]
		public async Task<IActionResult> Register(User newUser) {
			try {
				// Verificar si el usuario ya existe en la base de datos
				var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);

				if (existingUser != null) {
					return BadRequest("El usuario ya está registrado");
				}

				// Hashear la contraseña del nuevo usuario
				string hashedPassword = HashPassword(newUser.Password);

				// Crear un nuevo objeto User para ser insertado en la base de datos
				User insertUser = new User {
					UserId = Guid.NewGuid(),
					Name = newUser.Name,
					Email = newUser.Email,
					Password = hashedPassword, // Contraseña cifrada
					Nickname = newUser.Nickname,
					Gender = newUser.Gender,
					ProfilePictureUrl = newUser.ProfilePictureUrl,
					PhoneNumber = newUser.PhoneNumber
				};

				// Agregar el nuevo usuario a la base de datos
				_context.Users.Add(insertUser);
				await _context.SaveChangesAsync();

				return Ok("Usuario registrado exitosamente");
			} catch (Exception ex) {
				// Manejar cualquier excepción que pueda ocurrir durante el registro
				return StatusCode(500, "Error al registrar el usuario");
			}
		}

		[HttpPost("Authenticate")]
		public async Task<IActionResult> Authenticate([FromBody] LoginRequestModel loginRequest) {
			string email = loginRequest.Email;
			string password = loginRequest.Password;

			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
				return BadRequest("Invalid username or password");
			}

			User userAuthenticating = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

			if (userAuthenticating == null) {
				return NotFound("Email not registered");
			}

			string hashedPasswordEntered = HashPassword(password);

			if (hashedPasswordEntered != userAuthenticating.Password) {
				return Unauthorized("Wrong password");
			}

			// generate and send JWT
			var token = JwtGenerator(userAuthenticating);
			//send only as ok(token)?
			return Ok(new { Token = token });
		}

		[HttpGet("UserInfo")]
		[Authorize] // Añade [Authorize] para requerir autenticación mediante JWT
		public IActionResult GetUserInfo() {
			try {
				var userClaims = HttpContext.User.Claims; // Obtén las claims del usuario autenticado

				// Aquí puedes acceder a las claims del usuario, por ejemplo:
				var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var userEmail = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

				// Con la información obtenida del token, puedes realizar consultas en la base de datos, por ejemplo:
				// TODO: Realizar consulta en la base de datos usando userId o userEmail para obtener más información del usuario

				// Supongamos que obtienes datos del usuario de la base de datos
				var userFromDb = new { UserId = userId, Email = userEmail, OtherInfo = "Some data from DB" };

				// Devuelve los datos del usuario
				return Ok(userFromDb);
			} catch (Exception ex) {
				// Manejo de errores
				return StatusCode(500, "Error interno del servidor");
			}
		}


		[HttpGet("datos")]
		[Authorize] // Asegura que solo los usuarios autenticados puedan acceder a este endpoint
		public IActionResult ObtenerDatosUsuario() {
			try {
				// Obtener el token del encabezado de autorización
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

				// Validar y decodificar el token para obtener las claims (información del usuario)
				var handler = new JwtSecurityTokenHandler();
				var tokenSesion = handler.ReadJwtToken(token);

				// Obtener los datos del usuario desde las claims del token
				var userId = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
				var userEmail = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;
				var userName = tokenSesion.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
				// Aquí puedes obtener más datos del usuario desde las claims según lo que esté incluido en el token

				// Devolver los datos del usuario
				return Ok(new {
					UserId = userId,
					Email = userEmail,
					// Otros datos del usuario que quieras devolver
				});
			} catch (Exception ex) {
				// Manejar cualquier excepción que pueda ocurrir durante el proceso
				return BadRequest("Error al obtener los datos del usuario");
			}
		}


		private string HashPassword(string password) {
			// SHA256 for pass
			using (SHA256 sha256 = SHA256.Create()) {
				byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

				// hash to hexadecimal string
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < hashedBytes.Length; i++) {
					builder.Append(hashedBytes[i].ToString("x2"));
				}

				return builder.ToString();
			}
		}

		private string JwtGenerator(User user) {
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Secrets").GetSection("JWT").Value));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			//Array of claims to add into the token
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new Claim(ClaimTypes.Name, user.Name),
				new Claim(ClaimTypes.Email, user.Email)
				//adding many claims should be avoided.
			};
			var token = new JwtSecurityToken(
				issuer: "MoveITMVC",
				audience: "project-forntend-developer",
				claims: claims,
				expires: DateTime.UtcNow.AddHours(1), // Token duration
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}


		private ClaimsPrincipal ValidateJwtToken(string token, TokenValidationParameters validationParameters) {
			try {
				var tokenHandler = new JwtSecurityTokenHandler();
				SecurityToken validatedToken;
				var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
				return principal;
			} catch (Exception ex) {
				// Manejar cualquier excepción que pueda ocurrir durante la validación del token
				// Por ejemplo, token inválido, expirado, firma incorrecta, etc.
				return null;
			}
		}

	}
	public class LoginRequestModel {
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
