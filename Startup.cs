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

			// Middleware configuration for JWT
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
			// More configs
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

			//KEY code. The authentication applies only WHEN:
			//Check again if the condition really applies even if the endoint in the controller doesn't include [Authorize]
			app.UseWhen(context =>
				context.Request.Path.StartsWithSegments("/Auth"), //This applies after basic route api/controller.
				builder => {
					app.UseAuthentication();
					app.UseAuthorization();
				});

			//Why both are required? Which one is the JWT validation?
			app.UseAuthentication(); //This line was not present originally, so I couldn't properly authenticate
			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				// More endpoins
			});

		}

	}
}