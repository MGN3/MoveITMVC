using System;

namespace MoveITMVC.Models {
	public class OrderProduct {
		public Guid OrderId { get; set; }
		public Order Order { get; set; }

		public int ProductId { get; set; }
		public Product Product { get; set; }

		public int Quantity { get; set; }
	}
}