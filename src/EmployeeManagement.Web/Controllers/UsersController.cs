using EmployeeManagement.Application.Users.Interfaces;
using EmployeeManagement.Application.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(
        IUserService userService)
    {
        _userService = userService;
    }


    // ========================================================
    // INDEX
    // ========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users =
            await _userService.GetAllAsync();

        return View(users);
    }


    // ========================================================
    // DETAILS
    // ========================================================

    [HttpGet]
    public async Task<IActionResult> Details(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var user =
            await _userService.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        return View(user);
    }


    // ========================================================
    // CREATE
    // ========================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();

        return View(
            new UserCreateModel());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        UserCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(model);
        }

        try
        {
            await _userService.CreateAsync(model);

            TempData["SuccessMessage"] =
                "User created successfully.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadDropdowns();

            return View(model);
        }
    }


    // ========================================================
    // EDIT
    // ========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(
        string id)
    {
        var user =
            await _userService.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        var role =
            user.Roles.FirstOrDefault()
            ?? "Employee";

        var model = new UserEditModel
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = role,
            EmployeeId = user.EmployeeId,
            IsActive = user.IsActive
        };

        await LoadDropdowns();

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        UserEditModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(model);
        }

        try
        {
            var updated =
                await _userService.UpdateAsync(model);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] =
                "User updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadDropdowns();

            return View(model);
        }
    }


    // ========================================================
    // ACTIVATE / DEACTIVATE
    // ========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(
        string id,
        bool isActive)
    {
        if (id == User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            && !isActive)
        {
            TempData["ErrorMessage"] =
                "You cannot deactivate your own account.";

            return RedirectToAction(nameof(Index));
        }

        var result =
            await _userService.SetActiveAsync(
                id,
                isActive);

        if (!result)
            return NotFound();

        TempData["SuccessMessage"] =
            isActive
                ? "User activated successfully."
                : "User deactivated successfully.";

        return RedirectToAction(
            nameof(Index));
    }


    // ========================================================
    // RESET PASSWORD
    // ========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        string id,
        string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["ErrorMessage"] =
                "Password is required.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        if (newPassword.Length < 8)
        {
            TempData["ErrorMessage"] =
                "Password must contain at least 8 characters.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var result =
            await _userService.ResetPasswordAsync(
                id,
                newPassword);

        if (!result)
        {
            TempData["ErrorMessage"] =
                "Unable to reset the password.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        TempData["SuccessMessage"] =
            "Password reset successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }


    // ========================================================
    // DELETE
    // ========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        string id)
    {
        if (id == User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
        {
            TempData["ErrorMessage"] =
                "You cannot delete your own account.";

            return RedirectToAction(nameof(Index));
        }

        var result =
            await _userService.DeleteAsync(id);

        if (!result)
            return NotFound();

        TempData["SuccessMessage"] =
            "User deleted successfully.";

        return RedirectToAction(
            nameof(Index));
    }


    // ========================================================
    // DROPDOWNS
    // ========================================================

    private async Task LoadDropdowns()
    {
        ViewBag.Roles =
            await _userService.GetRolesAsync();

        ViewBag.Employees =
            await _userService.GetEmployeeLookupAsync();
    }
}