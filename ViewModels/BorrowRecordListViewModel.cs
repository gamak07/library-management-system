namespace LibraryApp.ViewModels;

public class BorrowRecordListViewModel
{
    public int Id { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public bool IsReturned { get; set; }
    public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;
}