using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using IdentityServer4.Events;
using IdentityServer4.Services;

using Codidact.Authentication.Domain.Entities;

namespace Codidact.Authentication.Web.Pages
{
    [BindProperties]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEventService _events;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          ILogger<LoginModel> logger,
                          UserManager<ApplicationUser> userManager,
                          IEventService events)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _events = events;
        }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberLogin { get; set; } = false;

        public string ReturnUrl { get; set; } = "/index";

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Email, Password, RememberLogin, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(Email);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName));

                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                }
            }

            return Page();
        }
    }
}
