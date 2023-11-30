namespace MoveITMVC.Models {
	public interface IUser {
		Guid UserId { get; set; }
		string Name { get; set; }
		string Email { get; set; }
		string Password { get; set; }
	}
}
