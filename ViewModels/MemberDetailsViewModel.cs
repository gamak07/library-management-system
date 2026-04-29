namespace LibraryApp.ViewModels;

using LibraryApp.Models;

public class MemberDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsSuspended { get; set; }
    public List<BorrowRecordViewModel> BorrowHistory { get; set; } = [];
}

public class BorrowRecordViewModel
{
    public int Id { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public bool IsReturned { get; set; }
    public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;
}