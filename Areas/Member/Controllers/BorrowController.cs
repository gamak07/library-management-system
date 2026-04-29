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
public class BorrowController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BorrowController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Member/Borrow — active borrows
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

        var activeBorrows = await _context.BorrowRecords
            .Include(b => b.Book)
            .Where(b => b.UserId == user.Id && !b.IsReturned)
            .OrderBy(b => b.DueDate)
            .Select(b => new BorrowRecordViewModel
            {
                Id         = b.Id,
                BookTitle  = b.Book.Title,
                BookAuthor = b.Book.Author,
                BorrowedAt = b.BorrowedAt,
                DueDate    = b.DueDate,
                ReturnedAt = b.ReturnedAt,
                IsReturned = b.IsReturned
            })
            .ToListAsync();

        return View(activeBorrows);
    }

    // GET: /Member/Borrow/History
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

        var history = await _context.BorrowRecords
            .Include(b => b.Book)
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.BorrowedAt)
            .Select(b => new BorrowRecordViewModel
            {
                Id         = b.Id,
                BookTitle  = b.Book.Title,
                BookAuthor = b.Book.Author,
                BorrowedAt = b.BorrowedAt,
                DueDate    = b.DueDate,
                ReturnedAt = b.ReturnedAt,
                IsReturned = b.IsReturned
            })
            .ToListAsync();

        return View(history);
    }

    // POST: /Member/Borrow/BorrowBook
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BorrowBook(int bookId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

        // check if member is suspended
        var isSuspended = user.LockoutEnd.HasValue &&
                          user.LockoutEnd > DateTimeOffset.UtcNow;
        if (isSuspended)
        {
            TempData["Error"] = "Your account is suspended. You cannot borrow books.";
            return RedirectToAction("Index", "Books", new { area = "Member" });
        }

        var book = await _context.Books.FindAsync(bookId);
        if (book == null) return NotFound();

        // check if copies are available
        if (book.CopiesAvailable <= 0)
        {
            TempData["Error"] = "No copies available for this book.";
            return RedirectToAction("Details", "Books",
                new { area = "Member", id = bookId });
        }

        // check if member already has this book
        var alreadyBorrowed = await _context.BorrowRecords
            .AnyAsync(b => b.UserId == user.Id &&
                           b.BookId == bookId &&
                           !b.IsReturned);
        if (alreadyBorrowed)
        {
            TempData["Error"] = "You already have this book borrowed.";
            return RedirectToAction("Details", "Books",
                new { area = "Member", id = bookId });
        }

        // create the borrow record
        var borrowRecord = new BorrowRecord
        {
            UserId     = user.Id,
            BookId     = bookId,
            BorrowedAt = DateTime.UtcNow,
            DueDate    = DateTime.UtcNow.AddDays(14), // 2 week loan period
            IsReturned = false,
            CreatedAt  = DateTime.UtcNow
        };

        // reduce available copies by 1
        book.CopiesAvailable--;
        book.UpdatedAt = DateTime.UtcNow;

        _context.BorrowRecords.Add(borrowRecord);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"You have successfully borrowed {book.Title}!";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Member/Borrow/ReturnBook
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnBook(int borrowId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

        // find the borrow record — must belong to this user
        var borrowRecord = await _context.BorrowRecords
            .Include(b => b.Book)
            .FirstOrDefaultAsync(b => b.Id == borrowId &&
                                      b.UserId == user.Id);

        if (borrowRecord == null) return NotFound();

        // mark as returned
        borrowRecord.IsReturned = true;
        borrowRecord.ReturnedAt = DateTime.UtcNow;
        borrowRecord.UpdatedAt  = DateTime.UtcNow;

        // increase available copies by 1
        borrowRecord.Book.CopiesAvailable++;
        borrowRecord.Book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"You have returned {borrowRecord.Book.Title}. Thank you!";
        return RedirectToAction(nameof(Index));
    }
}