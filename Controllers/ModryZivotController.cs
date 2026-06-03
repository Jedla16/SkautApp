using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Security;
using SkautApp.Services;

namespace SkautApp.Controllers
{
    public class ModryZivotController : SurfaceController
    {
        private readonly ModryZivotService _modryZivotService;
        private readonly IMemberManager _memberManager;
        private readonly IMemberSignInManager _memberSignInManager;

        public ModryZivotController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            ModryZivotService modryZivotService,
            IMemberManager memberManager,
            IMemberSignInManager memberSignInManager) // Přidáno pro správu přihlášení
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _modryZivotService = modryZivotService;
            _memberManager = memberManager;
            _memberSignInManager = memberSignInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Zapis(string vyzvaId, bool splneno)
        {
            // 1. Zjistíme, kdo je přihlášený
            var member = await _memberManager.GetCurrentMemberAsync();
            if (member == null) return Unauthorized();

            // 2. Převedeme ID na int (Umbraco Member ID)
            if (int.TryParse(member.Id, out int memberId))
            {
                // 3. Zápis k dnešnímu datu
                _modryZivotService.ZapisVyzvu(memberId, vyzvaId, DateTime.Today, splneno);

                return Ok(new { success = true, message = "Zapsáno!" });
            }

            return BadRequest("Chyba při identifikaci člena.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Odhlasit()
        {
            // Správné odhlášení přes SignInManager
            await _memberSignInManager.SignOutAsync();

            // Přesměrování na úvodní stránku (login)
            return Redirect("/");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ZmenaHesla(string oldPassword, string newPassword, string confirmNewPassword)
        {
            var member = await _memberManager.GetCurrentMemberAsync();
            if (member == null)
            {
                // Member not found or not logged in, redirect to login page
                return Redirect("/"); // Redirect to the site root (which should be the login page)
            }

            // Input validation
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                TempData["ErrorMessage"] = "Všechna pole musí být vyplněna.";
                return Redirect("/zmena-hesla");
            }

            if (newPassword != confirmNewPassword)
            {
                TempData["ErrorMessage"] = "Nové heslo a potvrzení hesla se neshodují.";
                return Redirect("/zmena-hesla");
            }

            if (newPassword.Length < 10) // Example: enforce minimum password length
            {
                TempData["ErrorMessage"] = "Nové heslo musí mít alespoň 10 znaků.";
                return Redirect("/zmena-hesla");
            }

            var changePasswordResult = await _memberManager.ChangePasswordAsync(member, oldPassword, newPassword);

            if (changePasswordResult.Succeeded)
            {
                // Re-sign in the user after password change to update security stamp if needed
                await _memberSignInManager.SignInAsync(member, isPersistent: false);
                TempData["SuccessMessage"] = "Heslo bylo úspěšně změněno.";
            }
            else
            {
                var errors = string.Join(" ", changePasswordResult.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"Chyba při změně hesla: {errors}";
            }

            return Redirect("/zmena-hesla");
        }
    }
}