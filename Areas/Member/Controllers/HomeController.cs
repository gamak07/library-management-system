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
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // get the currently logged in user
        // User is a ClaimsPrincipal — built into Controller
        // GetUserAsync reads the identity claims and finds the user in the database
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

        // fetch active borrows with book data
        var activeBorrows = await _context.BorrowRecords
            .Include(b => b.Book)
            .Where(b => b.UserId == user.Id && !b.IsReturned)
            .OrderBy(b => b.DueDate)
            .Select(b => new ActiveBorrowViewModel
            {
                Id         = b.Id,
                BookTitle  = b.Book.Title,
                BookAuthor = b.Book.Author,
                BorrowedAt = b.BorrowedAt,
                DueDate    = b.DueDate
            })
            .ToListAsync();

        var viewModel = new MemberDashboardViewModel
        {
            FirstName           = user.FirstName,
            LastName            = user.LastName,
            TotalBooksInLibrary = await _context.Books.CountAsync(),
            CurrentlyBorrowed   = await _context.BorrowRecords
                .CountAsync(b => b.UserId == user.Id && !b.IsReturned),
            ReturnedBooks       = await _context.BorrowRecords
                .CountAsync(b => b.UserId == user.Id && b.IsReturned),
            OverdueBooks        = await _context.BorrowRecords
                .CountAsync(b => b.UserId == user.Id &&
                                 !b.IsReturned &&
                                 b.DueDate < DateTime.UtcNow),
            ActiveBorrows       = activeBorrows
        };

        return View(viewModel);
    }
}