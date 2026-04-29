namespace LibraryApp.ViewModels;

public class BookBrowseViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public int CopiesAvailable { get; set; }
    public int TotalCopies { get; set; }
    public bool IsAvailable => CopiesAvailable > 0;
    public bool AlreadyBorrowed { get; set; }
}