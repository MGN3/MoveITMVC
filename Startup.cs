using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoveITMVC.Models;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Configuration;

namespace MoveITMVC {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {
			var jwtPassword = Configuration.GetSection("Secrets").GetSection("JWT").Value;

			services.AddDbContext<MoveITDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("cnMoveIT")));

			services.AddCors(options => {
				options.AddDefaultPolicy(builder => {
					builder.AllowAnyOrigin()
						   .AllowAnyMethod() // Any HTTP Method, change to make it only availeable for HTTPS
						   .AllowAnyHeader();
				});
			});

			// Configuración del middleware de autenticación JWT
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options => {
					options.TokenValidationParameters = new TokenValidationParameters {
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = "MoveITMVC",
						ValidAudience = "project-frontend-developer",
						IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtPassword)),
						// more parameteres if needed
					};
				});

			// Add other needed services
			services.AddControllers();


			// More configutarions
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MoveITDbContext dbContext) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			} else {
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}

			app.UseCors();
			app.UseHttpsRedirection();

			//Make sure DataBase exists before routing config
			dbContext.Database.EnsureCreated();

			app.UseRouting();
			app.UseWhen(context =>
				context.Request.Path.StartsWithSegments("/api/Auth"), // MODIFICAR RUTAS A COMPROBAR, añadir un identificador a las que requieran comprobación de JWT?
				builder => {
					builder.UseAuthentication();
				});

			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				// More endpoins
			});

		}

	}
}