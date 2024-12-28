using Microsoft.EntityFrameworkCore;
using System;

namespace MoveITMVC.Models {
	public class MoveITDbContext : DbContext {
		public DbSet<User> Users { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<Address> Addresses { get; set; }
		public DbSet<ShoppingCart> ShoppingCarts { get; set; }
		public DbSet<OrderProduct> OrderProducts { get; set; }
		public DbSet<ShoppingCartProduct> ShoppingCartProducts { get; set; }

		public MoveITDbContext(DbContextOptions<MoveITDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			// Products
			modelBuilder.Entity<Product>(products => {
				products.ToTable("Products");
				products.HasKey(p => p.ProductId);
				products.Property(p => p.Category).IsRequired().HasMaxLength(200);
				products.Property(p => p.Name).HasMaxLength(500);
				products.Property(p => p.Price).IsRequired();
				products.Property(p => p.UrlImg).IsRequired();
				products.Property(p => p.Description).IsRequired();
			});

			// Users
			modelBuilder.Entity<User>(users => {
				users.ToTable("Users");
				users.HasKey(p => p.UserId);
				users.Property(p => p.Name).IsRequired().HasMaxLength(200);
				users.Property(p => p.Email).IsRequired().HasMaxLength(254);
				users.HasIndex(p => p.Email).IsUnique();
				users.Property(p => p.Password).IsRequired().HasMaxLength(64);

				// User Address 1 - n
				users.HasMany(u => u.Addresses)
					.WithOne(a => a.User)
					.HasForeignKey(a => a.UserId)
					.IsRequired(false)
					.OnDelete(DeleteBehavior.Cascade);
				

				// User Order 1-n
				users.HasMany(u => u.Orders)
					.WithOne(o => o.User)
					.HasForeignKey(o => o.UserId)
					.IsRequired(false)
					.OnDelete(DeleteBehavior.Cascade);

				// User ShoppingCart 1-1
				users.HasOne(u => u.ShoppingCart)
					.WithOne(sc => sc.User)
					.IsRequired(false)
					.HasForeignKey<ShoppingCart>(sc => sc.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// Addresses
			modelBuilder.Entity<Address>(addresses => {
				addresses.ToTable("Addresses");
				addresses.HasKey(a => a.AddressId);
				addresses.Property(a => a.Street).IsRequired().HasMaxLength(200);
				addresses.Property(a => a.City).IsRequired().HasMaxLength(100);
				addresses.Property(a => a.Region).HasMaxLength(100);
				addresses.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
				addresses.Property(a => a.Country).IsRequired().HasMaxLength(100);
				addresses.Property(a => a.IsShippingAddress).IsRequired();

				// Adress User n-1
				addresses.HasOne(a => a.User)
					.WithMany(u => u.Addresses)
					.HasForeignKey(a => a.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// Orders
			modelBuilder.Entity<Order>(orders => {
				orders.ToTable("Orders");
				orders.HasKey(o => o.OrderId);
				orders.Property(o => o.OrderDate).IsRequired();
				orders.Property(o => o.Status).IsRequired();
				orders.Property(o => o.TotalPrice).IsRequired();

				// Order Product relation through the intermediate table OrderProducts
				orders.HasMany(o => o.OrderProducts)
					  .WithOne(op => op.Order)
					  .HasForeignKey(op => op.OrderId);

				// Order User n-1
				modelBuilder.Entity<Order>()
					.HasOne(o => o.User)
					.WithMany(u => u.Orders)
					.HasForeignKey(o => o.UserId)
					.OnDelete(DeleteBehavior.Restrict); // .Restrict instead of Cascade
														//ERROR AT THE END->

				// Order ShippingAddress 1-1
				orders.HasOne(o => o.ShippingAddress)
					.WithMany()
					.HasForeignKey(o => o.ShippingAddressId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// ShoppingCart
			modelBuilder.Entity<ShoppingCart>(shoppingCart => {
				shoppingCart.ToTable("ShoppingCarts");
				shoppingCart.HasKey(sc => sc.ShoppingCartId);
				shoppingCart.Property(sc => sc.Created).IsRequired();
				shoppingCart.Property(sc => sc.TotalPrice).IsRequired();

				// ShoppingCart User 1-1
				shoppingCart.HasOne(sc => sc.User)
					.WithOne(u => u.ShoppingCart)
					.HasForeignKey<ShoppingCart>(sc => sc.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// The OrderProduct Id is composed of OrderId and ProductId
			modelBuilder.Entity<OrderProduct>(orderProducts => {
				orderProducts.ToTable("OrderProducts");
				orderProducts.HasKey(op => new { op.OrderId, op.ProductId });
			});
			//The ShoppingCartProduct Id is composed of ShoppingCartId and ProductId
			modelBuilder.Entity<ShoppingCartProduct>(shoppingCartProducts => {
				shoppingCartProducts.ToTable("ShoppingCartProducts");
				shoppingCartProducts.HasKey(scp => new { scp.ShoppingCartId, scp.ProductId });
			});
		}
	}
}
/* Microsoft.AspNetCore.Hosting.Diagnostics[6]
				  Application startup exception
				  Microsoft.Data.SqlClient.SqlException (0x80131904): Si especifica la restricción FOREIGN KEY 'FK_Orders_Users_UserId' en la tabla 'Orders', podrían producirse ciclos o múltiples rutas en cascada. Especifique ON DELETE NO ACTION o UPDATE NO ACTION, o bien modifique otras restricciones FOREIGN KEY.
				  No se pudo crear la restricción o el índice. Vea los errores anteriores.
					 at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
					 at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
					 at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
					 at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)
					 at Microsoft.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean isAsync, Int32 timeout, Boolean asyncWrite)
					 at Microsoft.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, Boolean sendToPipe, Int32 timeout, Boolean& usedCache, Boolean asyncWrite, Boolean inRetry, String methodName)
					 at Microsoft.Data.SqlClient.SqlCommand.ExecuteNonQuery()
					 at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteNonQuery(RelationalCommandParameterObject parameterObject)
					 at Microsoft.EntityFrameworkCore.Migrations.MigrationCommand.ExecuteNonQuery(IRelationalConnection connection, IReadOnlyDictionary`2 parameterValues)
					 at Microsoft.EntityFrameworkCore.Migrations.Internal.MigrationCommandExecutor.ExecuteNonQuery(IEnumerable`1 migrationCommands, IRelationalConnection connection)
					 at Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator.CreateTables()
					 at Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator.EnsureCreated()
					 at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.EnsureCreated()
					 at MoveITMVC.Startup.Configure(IApplicationBuilder app, IWebHostEnvironment env, MoveITDbContext dbContext) in E:\MGN\source\repos\MoveITMVC\Startup.cs:line 52
					 at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
					 at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)
					 at System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
					 at Microsoft.AspNetCore.Hosting.ConfigureBuilder.Invoke(Object instance, IApplicationBuilder builder)
					 at Microsoft.AspNetCore.Hosting.GenericWebHostService.StartAsync(CancellationToken cancellationToken)
				  ClientConnectionId:915cce16-7432-4c3c-8996-cbf2f6979b6b
				  Error Number:1785,State:0,Class:16*/
