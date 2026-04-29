using LibraryApp.Data;
using LibraryApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BorrowRecordsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BorrowRecordsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? filter)
    {
        // fetch all borrow records with related Book and User data
        var query = _context.BorrowRecords
            .Include(b => b.Book)
            .Include(b => b.User)
            .AsQueryable();

        // apply filter based on query string parameter
        query = filter switch
        {
            "active"   => query.Where(b => !b.IsReturned &&
                          b.DueDate >= DateTime.UtcNow),
            "returned" => query.Where(b => b.IsReturned),
            "overdue"  => query.Where(b => !b.IsReturned &&
                          b.DueDate < DateTime.UtcNow),
            _          => query // default — no filter, show all
        };

        // project into ViewModel
        var records = await query
            .OrderByDescending(b => b.BorrowedAt)
            .Select(b => new BorrowRecordListViewModel
            {
                Id           = b.Id,
                MemberName   = b.User.FirstName + " " + b.User.LastName,
                MemberEmail  = b.User.Email ?? string.Empty,
                BookTitle    = b.Book.Title,
                BookAuthor   = b.Book.Author,
                BorrowedAt   = b.BorrowedAt,
                DueDate      = b.DueDate,
                ReturnedAt   = b.ReturnedAt,
                IsReturned   = b.IsReturned
            })
            .ToListAsync();

        // pass filter and counts to view via ViewBag
        ViewBag.CurrentFilter  = filter ?? "all";
        ViewBag.TotalCount     = await _context.BorrowRecords.CountAsync();
        ViewBag.ActiveCount    = await _context.BorrowRecords
            .CountAsync(b => !b.IsReturned && b.DueDate >= DateTime.UtcNow);
        ViewBag.ReturnedCount  = await _context.BorrowRecords
            .CountAsync(b => b.IsReturned);
        ViewBag.OverdueCount   = await _context.BorrowRecords
            .CountAsync(b => !b.IsReturned && b.DueDate < DateTime.UtcNow);

        return View(records);
    }
}