using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace SkautApp.Controllers
{
    public class SkautLoginController : SurfaceController
    {
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly IMemberManager _memberManager;

        public SkautLoginController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberSignInManager memberSignInManager,
            IMemberManager memberManager)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberSignInManager = memberSignInManager;
            _memberManager = memberManager;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> HandleLogin(LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username))
            {
                TempData["LoginFailure"] = "Chybí přihlašovací údaje.";
                return Redirect("/prihlaseni");
            }

            var result = await _memberSignInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var identityMember = await _memberManager.FindByNameAsync(model.Username);

                if (identityMember != null)
                {
                    var member = _memberManager.AsPublishedMember(identityMember);

                    if (member != null)
                    {
                        var druzinaValue = member.Value<string>("druzina");

                        if (string.IsNullOrEmpty(druzinaValue))
                        {
                            TempData["LoginFailure"] = "V profilu skauta není vybraná družina!";
                            return Redirect("/prihlaseni");
                        }

                        string vybranaDruzina = druzinaValue.Trim();

                        if (vybranaDruzina == "Skauti") return Redirect("/skauti");
                        if (vybranaDruzina == "Vlčata") return Redirect("/vlcata");
                        if (vybranaDruzina == "Benjamínci") return Redirect("/benjaminci");
                        if (vybranaDruzina == "Světlušky") return Redirect("/svetlusky");
                        if (vybranaDruzina == "Skautky") return Redirect("/skautky");
                        if (vybranaDruzina == "Roveři") return Redirect("/roveri");
                        if (vybranaDruzina == "Rangers") return Redirect("/rangers");

                        return Redirect("/");
                    }
                }
            }

            TempData["LoginFailure"] = "Špatné jméno nebo heslo!";
            return Redirect("/prihlaseni");
        }
    }
}