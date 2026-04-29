namespace LibraryApp.ViewModels;

using LibraryApp.Models;

public class MemberListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalBorrows { get; set; }
    public int ActiveBorrows { get; set; }
    public bool IsSuspended { get; set; }
}