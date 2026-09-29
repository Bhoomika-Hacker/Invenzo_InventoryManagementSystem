// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using InventoryManagementSystem.Data;

namespace InventoryManagementSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;


        // =========================
        // CONSTRUCTOR
        // =========================

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }


        // =========================
        // INPUT
        // =========================

        [BindProperty]
        public InputModel Input { get; set; } = default!;


        public string? ReturnUrl { get; set; }


        public IList<AuthenticationScheme>? ExternalLogins { get; set; }


        // =========================
        // INPUT MODEL
        // =========================

        public class InputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(
                ErrorMessage = "Please enter a valid email address")]
            [RegularExpression(
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                ErrorMessage =
                    "Please enter a valid email address, e.g. abc@gmail.com")]
            [Display(Name = "Email")]
            public string Email { get; set; } = default!;


            [Required(ErrorMessage = "Password is required")]
            [StringLength(
                100,
                ErrorMessage =
                    "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = default!;


            [Required(ErrorMessage = "Confirm password is required")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare(
                "Password",
                ErrorMessage =
                    "The password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }
        }


        // =========================
        // GET REGISTER PAGE
        // =========================

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins =
                (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();
        }


        // =========================
        // POST REGISTER
        // =========================

        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");


            ExternalLogins =
                (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();


            // =========================
            // VALIDATION
            // =========================

            if (ModelState.IsValid)
            {
                // =========================
                // CREATE USER
                // =========================

                var user = CreateUser();


                // =========================
                // SET USERNAME
                // =========================

                await _userStore.SetUserNameAsync(
                    user,
                    Input.Email,
                    CancellationToken.None);


                // =========================
                // SET EMAIL
                // =========================

                await _emailStore.SetEmailAsync(
                    user,
                    Input.Email,
                    CancellationToken.None);


                // =========================
                // CREATE ACCOUNT
                // =========================

                var result =
                    await _userManager.CreateAsync(
                        user,
                        Input.Password);


                // =========================
                // ACCOUNT CREATED
                // =========================

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "User created a new account with password.");


                    // =========================
                    // ASSIGN SELLER ROLE
                    // =========================

                    var roleResult =
                        await _userManager.AddToRoleAsync(
                            user,
                            "Seller");


                    // =========================
                    // CHECK ROLE RESULT
                    // =========================

                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                error.Description);
                        }

                        return Page();
                    }


                    // =========================
                    // DO NOT AUTO LOGIN
                    // =========================
                    //
                    // User should go to Login page
                    // and login manually.
                    //

                    return RedirectToPage("./Login");
                }


                // =========================
                // USER CREATION ERRORS
                // =========================

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }
            }


            // =========================
            // RETURN REGISTER PAGE
            // =========================

            return Page();
        }


        // =========================
        // CREATE APPLICATION USER
        // =========================

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
                    $"Ensure that '{nameof(ApplicationUser)}' is not an " +
                    $"abstract class and has a parameterless constructor, " +
                    $"or alternatively override the register page in " +
                    $"/Areas/Identity/Pages/Account/Register.cshtml");
            }
        }


        // =========================
        // GET EMAIL STORE
        // =========================

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
}