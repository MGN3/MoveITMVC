using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoveITMVC.Models;
using System.Globalization;

namespace MoveITMVC {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {
			services.AddDbContext<MoveITDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("cnMoveIT")));

			services.AddCors(options => {
				options.AddDefaultPolicy(builder => {
					builder.AllowAnyOrigin()
						   .AllowAnyMethod() // cualquier método HTTP
						   .AllowAnyHeader();
				});
			});

			// Otros servicios que puedas necesitar
			services.AddControllers();
			// ...

			// Otras configuraciones
			// ...
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

			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			// Asegurar la creación de la base de datos antes de configurar el enrutamiento
			dbContext.Database.EnsureCreated();

			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				// Otros endpoints
			});
		}

	}
}