namespace MoveITMVC.Models {
	public class ShoppingCartProduct {
		public Guid ShoppingCartId { get; set; }
		public ShoppingCart ShoppingCart { get; set; }

		public int ProductId { get; set; }
		public Product Product { get; set; }

		public int Quantity { get; set; }
	}
}