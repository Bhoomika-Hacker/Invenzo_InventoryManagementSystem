// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using InventoryManagementSystem.Data;

namespace InventoryManagementSystem.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<ExternalLoginModel> _logger;


    // =========================================
    // CONSTRUCTOR
    // =========================================

    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<ExternalLoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _logger = logger;
    }


    // =========================================
    // INPUT
    // =========================================

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ProviderDisplayName { get; set; }

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
    }


    // =========================================
    // GET
    // =========================================

    public IActionResult OnGet()
    {
        return RedirectToPage("./Login");
    }


    // =========================================
    // POST - START GOOGLE LOGIN
    // =========================================

    public IActionResult OnPost(
        string provider,
        string? returnUrl = null)
    {
        var redirectUrl = Url.Page(
            "./ExternalLogin",
            pageHandler: "Callback",
            values: new
            {
                returnUrl
            });

        var properties =
            _signInManager.ConfigureExternalAuthenticationProperties(
                provider,
                redirectUrl!);

        return new ChallengeResult(
            provider,
            properties);
    }


    // =========================================
    // GOOGLE CALLBACK
    // =========================================

    public async Task<IActionResult> OnGetCallbackAsync(
        string? returnUrl = null,
        string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");


        // =========================================
        // GOOGLE ERROR
        // =========================================

        if (remoteError != null)
        {
            ErrorMessage =
                $"Error from external provider: {remoteError}";

            return RedirectToPage(
                "./Login",
                new
                {
                    ReturnUrl = returnUrl
                });
        }


        // =========================================
        // GET GOOGLE LOGIN INFO
        // =========================================

        var info =
            await _signInManager
                .GetExternalLoginInfoAsync();


        if (info == null)
        {
            ErrorMessage =
                "Error loading external login information.";

            return RedirectToPage(
                "./Login",
                new
                {
                    ReturnUrl = returnUrl
                });
        }


        // =========================================
        // CHECK EXISTING GOOGLE USER
        // =========================================

        var user =
            await _userManager.FindByLoginAsync(
                info.LoginProvider,
                info.ProviderKey);


        // =========================================
        // EXISTING GOOGLE USER
        // =========================================

        if (user != null)
        {
            // -----------------------------------------
            // Make sure Seller role exists
            // -----------------------------------------

            if (!await _userManager.IsInRoleAsync(
                user,
                "Seller"))
            {
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "Seller");


                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        _logger.LogError(
                            "Failed to add Seller role: {Error}",
                            error.Description);
                    }

                    ErrorMessage =
                        "Unable to assign Seller role.";

                    return RedirectToPage(
                        "./Login",
                        new
                        {
                            ReturnUrl = returnUrl
                        });
                }
            }


            // -----------------------------------------
            // EXISTING GOOGLE USER LOGIN
            // -----------------------------------------

            var result =
                await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: false,
                    bypassTwoFactor: true);


            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "{Name} logged in with {LoginProvider} provider.",
                    info.Principal.Identity?.Name,
                    info.LoginProvider);


                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }


            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
        }


        // =========================================
        // NEW GOOGLE USER
        // =========================================

        ReturnUrl = returnUrl;

        ProviderDisplayName =
            info.ProviderDisplayName;


        if (info.Principal.HasClaim(
            c => c.Type == ClaimTypes.Email))
        {
            Input = new InputModel
            {
                Email =
                    info.Principal.FindFirstValue(
                        ClaimTypes.Email)!
            };
        }


        return Page();
    }


    // =========================================
    // POST CONFIRMATION
    // CREATE NEW GOOGLE USER
    // =========================================

    public async Task<IActionResult> OnPostConfirmationAsync(
        string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");


        // =========================================
        // GET GOOGLE LOGIN INFO
        // =========================================

        var info =
            await _signInManager
                .GetExternalLoginInfoAsync();


        if (info == null)
        {
            ErrorMessage =
                "Error loading external login information during confirmation.";

            return RedirectToPage(
                "./Login",
                new
                {
                    ReturnUrl = returnUrl
                });
        }


        // =========================================
        // VALIDATE
        // =========================================

        if (ModelState.IsValid)
        {
            // -----------------------------------------
            // CREATE USER
            // -----------------------------------------

            var user = CreateUser();


            // -----------------------------------------
            // SET USERNAME
            // -----------------------------------------

            await _userStore.SetUserNameAsync(
                user,
                Input.Email,
                CancellationToken.None);


            // -----------------------------------------
            // SET EMAIL
            // -----------------------------------------

            await _emailStore.SetEmailAsync(
                user,
                Input.Email,
                CancellationToken.None);


            // -----------------------------------------
            // CREATE USER IN DATABASE
            // -----------------------------------------

            var result =
                await _userManager.CreateAsync(user);


            if (result.Succeeded)
            {
                // -----------------------------------------
                // ADD GOOGLE LOGIN
                // -----------------------------------------

                result =
                    await _userManager.AddLoginAsync(
                        user,
                        info);


                if (result.Succeeded)
                {
                    // =====================================
                    // ASSIGN SELLER ROLE
                    // =====================================

                    var roleResult =
                        await _userManager.AddToRoleAsync(
                            user,
                            "Seller");


                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                error.Description);
                        }


                        ProviderDisplayName =
                            info.ProviderDisplayName;

                        ReturnUrl = returnUrl;

                        return Page();
                    }


                    // =====================================
                    // ACCOUNT CREATED
                    // =====================================

                    _logger.LogInformation(
                        "User created an account using {Name} provider.",
                        info.LoginProvider);


                    // =====================================
                    // DO NOT AUTO LOGIN
                    // =====================================
                    //
                    // Google account is registered.
                    // User must now login manually.
                    //

                    return RedirectToPage("./Login");
                }
            }


            // =========================================
            // USER CREATION ERRORS
            // =========================================

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }
        }


        // =========================================
        // RETURN TO CONFIRMATION PAGE
        // =========================================

        ProviderDisplayName =
            info.ProviderDisplayName;

        ReturnUrl = returnUrl;

        return Page();
    }


    // =========================================
    // CREATE USER
    // =========================================

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException(
                $"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class " +
                $"and has a parameterless constructor, or alternatively " +
                $"override the external login page in " +
                $"/Areas/Identity/Pages/Account/ExternalLogin.cshtml");
        }
    }


    // =========================================
    // EMAIL STORE
    // =========================================

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException(
                "The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}