using System;
using System.Collections.Generic;

namespace MoveITMVC.Models {
	public class Order {
		public Guid OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public OrderStatus Status { get; set; }
		public decimal TotalPrice { get; set; }

		// Relación con la tabla intermedia OrderProduct
		public List<OrderProduct> OrderProducts { get; set; }

		//Shipping Address relation
		public Guid ShippingAddressId { get; set; }
		public Address ShippingAddress { get; set; }

		// User relation
		public Guid UserId { get; set; }
		public User User { get; set; }

		//Create empty constructor
	}

	public enum OrderStatus {
		Pending,
		Processing,
		Shipped,
		Delivered,
		Cancelled
	}
}