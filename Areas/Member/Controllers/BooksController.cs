using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Areas.Member.Controllers;

[Area("Member")]
[Authorize(Roles = "Member")]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BooksController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

        // start with all books
        var query = _context.Books.AsQueryable();

        // apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // case insensitive search on title or author
            query = query.Where(b =>
                b.Title.Contains(search) ||
                b.Author.Contains(search));
        }

        var books = await query.ToListAsync();

        // find which books this user has already borrowed and not returned
        var activeBorrowedBookIds = await _context.BorrowRecords
            .Where(b => b.UserId == user.Id && !b.IsReturned)
            .Select(b => b.BookId)
            .ToListAsync();

        // project into ViewModel
        var viewModel = books.Select(b => new BookBrowseViewModel
        {
            Id              = b.Id,
            Title           = b.Title,
            Author          = b.Author,
            Genre           = b.Genre,
            ISBN            = b.ISBN,
            Description     = b.Description,
            CoverImageUrl   = b.CoverImageUrl,
            CopiesAvailable = b.CopiesAvailable,
            TotalCopies     = b.TotalCopies,
            AlreadyBorrowed = activeBorrowedBookIds.Contains(b.Id)
        }).ToList();

        ViewBag.Search = search;
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        // check if this user already has this book borrowed
        var alreadyBorrowed = await _context.BorrowRecords
            .AnyAsync(b => b.UserId == user.Id &&
                           b.BookId == id &&
                           !b.IsReturned);

        var viewModel = new BookBrowseViewModel
        {
            Id              = book.Id,
            Title           = book.Title,
            Author          = book.Author,
            Genre           = book.Genre,
            ISBN            = book.ISBN,
            Description     = book.Description,
            CoverImageUrl   = book.CoverImageUrl,
            CopiesAvailable = book.CopiesAvailable,
            TotalCopies     = book.TotalCopies,
            AlreadyBorrowed = alreadyBorrowed
        };

        return View(viewModel);
    }
}