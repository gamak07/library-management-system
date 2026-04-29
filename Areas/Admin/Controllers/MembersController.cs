using LibraryApp.Models;
using LibraryApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;

namespace LibraryApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MembersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MembersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Admin/Members
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // fetch all members in the Member role
        var members = await _userManager.GetUsersInRoleAsync("Member");

        // project each member into MemberListViewModel
        var viewModel = new List<MemberListViewModel>();

        foreach (var member in members)
        {
            // count their borrow records
            var totalBorrows = await _context.BorrowRecords
                .CountAsync(b => b.UserId == member.Id);

            var activeBorrows = await _context.BorrowRecords
                .CountAsync(b => b.UserId == member.Id && !b.IsReturned);

            viewModel.Add(new MemberListViewModel
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email ?? string.Empty,
                CreatedAt = member.CreatedAt,
                TotalBorrows = totalBorrows,
                ActiveBorrows = activeBorrows,

                // suspended if LockoutEnd exists and is in the future
                IsSuspended = member.LockoutEnd.HasValue &&
                              member.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        return View(viewModel.OrderByDescending(m => m.CreatedAt).ToList());
    }

    // GET: /Admin/Members/Details/id
    [HttpGet]
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null) return NotFound();

        var member = await _userManager.FindByIdAsync(id);
        if (member == null) return NotFound();

        // eager loading — fetch borrow records WITH their related Book data
        // .Include(b => b.Book) loads the Book navigation property
        // without Include, b.Book would be null
        var borrowRecords = await _context.BorrowRecords
            .Include(b => b.Book)
            .Where(b => b.UserId == id)
            .OrderByDescending(b => b.BorrowedAt)
            .ToListAsync();

        var viewModel = new MemberDetailsViewModel
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Email = member.Email ?? string.Empty,
            CreatedAt = member.CreatedAt,
            IsSuspended = member.LockoutEnd.HasValue &&
                          member.LockoutEnd > DateTimeOffset.UtcNow,

            // project each BorrowRecord into BorrowRecordViewModel
            BorrowHistory = borrowRecords.Select(b => new BorrowRecordViewModel
            {
                Id = b.Id,
                BookTitle = b.Book.Title,
                BookAuthor = b.Book.Author,
                BorrowedAt = b.BorrowedAt,
                DueDate = b.DueDate,
                ReturnedAt = b.ReturnedAt,
                IsReturned = b.IsReturned
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: /Admin/Members/Suspend/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(string id)
    {
        var member = await _userManager.FindByIdAsync(id);
        if (member == null) return NotFound();

        // LockoutEnd set far in the future = permanently suspended
        // LockoutEnabled must be true for lockout to work
        member.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        member.LockoutEnabled = true;

        await _userManager.UpdateAsync(member);
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/Members/Reinstate/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reinstate(string id)
    {
        var member = await _userManager.FindByIdAsync(id);
        if (member == null) return NotFound();

        // clear the lockout — account is active again
        member.LockoutEnd = null;

        await _userManager.UpdateAsync(member);
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/Members/Delete/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var member = await _userManager.FindByIdAsync(id);
        if (member == null) return NotFound();

        // delete all their borrow records first
        // otherwise foreign key constraint will block deletion
        var borrowRecords = await _context.BorrowRecords
            .Where(b => b.UserId == id)
            .ToListAsync();

        _context.BorrowRecords.RemoveRange(borrowRecords);
        await _context.SaveChangesAsync();

        // now delete the user
        await _userManager.DeleteAsync(member);
        return RedirectToAction(nameof(Index));
    }
}