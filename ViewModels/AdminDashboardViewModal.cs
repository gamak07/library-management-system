namespace LibraryApp.ViewModels;

using LibraryApp.Models;

public class AdminDashboardViewModel
{
    public int TotalBooks { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveBorrows { get; set; }
    public int ReturnedBooks { get; set; }
    public List<ApplicationUser> RecentMembers { get; set; } = [];
    public List<BorrowRecord> RecentBorrowRecords { get; set; } = [];
}