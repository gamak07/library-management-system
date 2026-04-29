namespace LibraryApp.ViewModels;

public class MemberDashboardViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int TotalBooksInLibrary { get; set; }
    public int CurrentlyBorrowed { get; set; }
    public int ReturnedBooks { get; set; }
    public int OverdueBooks { get; set; }
    public List<ActiveBorrowViewModel> ActiveBorrows { get; set; } = [];
}

public class ActiveBorrowViewModel
{
    public int Id { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsOverdue => DateTime.UtcNow > DueDate;
}