using Microsoft.AspNetCore.Mvc;
using SkautApp.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using System.Globalization;
using System.Text;

namespace SkautApp.Controllers
{
    [Route("prihlaseni")]
    public class PrihlaseniController : Controller
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;

        public PrihlaseniController(IMemberManager memberManager, IMemberService memberService)
        {
            _memberManager = memberManager;
            _memberService = memberService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var identityUser = await _memberManager.GetCurrentMemberAsync();
                if (identityUser != null)
                {
                    var member = _memberService.GetByKey(Guid.Parse(identityUser.Key.ToString()));
                    var nazevDruziny = member?.GetValue<string>("druzina") ?? "";

                    if (!string.IsNullOrEmpty(nazevDruziny))
                    {
                        string stext = nazevDruziny.Normalize(NormalizationForm.FormD);
                        var sb = new StringBuilder();
                        foreach (char c in stext)
                        {
                            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                            {
                                sb.Append(c);
                            }
                        }

                        var urlDruziny = "/" + sb.ToString().Normalize(NormalizationForm.FormC).ToLower().Trim().Replace(" ", "-");
                        return Redirect(urlDruziny);
                    }
                }
            }

            return View("~/Views/Prihlaseni.cshtml");
        }
    }
}