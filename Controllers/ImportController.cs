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
using System.Text;
using System.Globalization;

namespace SkautApp.Controllers
{
    public class ImportController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImportController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberManager memberManager,
            IMemberService memberService,
            IWebHostEnvironment webHostEnvironment)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _memberService = memberService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> SpustitImport()
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "imports", "clenove.csv");
            
            if (!System.IO.File.Exists(filePath))
                return BadRequest($"Soubor nebyl nalezen. Ujisti se, že soubor existuje zde: {filePath}");

            var lines = System.IO.File.ReadAllLines(filePath, Encoding.UTF8);
            var log = new StringBuilder();
            int vytvoreno = 0;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(';');
                if (parts.Length < 2) continue;

                string celeJmeno = parts[0].Trim();
                string druzinka = parts[1].Trim();

                string username = OdstranDiakritiku(celeJmeno).ToLower().Replace(" ", ".");
                string email = $"{username}@skautapp.cz";

                var existujici = await _memberManager.FindByNameAsync(username);
                if (existujici == null)
                {
                    try
                    {
                        // 1. Vytvoření člena
                        var member = _memberService.CreateMember(username, email, celeJmeno, "Member");
                        _memberService.Save(member);

                        // 2. Nastavení hesla přes MemberManager (aby se správně zahashovalo)
                        var identityUser = await _memberManager.FindByNameAsync(username);
                        if (identityUser != null)
                        {
                            await _memberManager.AddPasswordAsync(identityUser, "Skaut2026!");
                        }

                        // 3. Nastavení družiny
                        member.SetValue("druzina", druzinka);
                        _memberService.Save(member);

                        log.AppendLine($"✅ OK: {celeJmeno} (login: {username})");
                        vytvoreno++;
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine($"❌ CHYBA u {celeJmeno}: {ex.Message}");
                    }
                }
                else
                {
                    log.AppendLine($"ℹ️ PŘESKOČENO: {celeJmeno} už existuje.");
                }
            }

            return Ok($"Import dokončen. Vytvořeno {vytvoreno} členů.\n\nDetailní výpis:\n{log.ToString()}");
        }

        private string OdstranDiakritiku(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}