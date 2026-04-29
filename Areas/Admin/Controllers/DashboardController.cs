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
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var members = await _userManager.GetUsersInRoleAsync("Member");
        var recentMembers = members
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .ToList();

        var allMembers = await _userManager.GetUsersInRoleAsync("Member");
        return View(new AdminDashboardViewModel
        {
            TotalBooks = await _context.Books.CountAsync(),
            TotalMembers = allMembers.Count,
            ActiveBorrows = 0,
            ReturnedBooks = 0,
            RecentMembers = recentMembers
        });
    }
}