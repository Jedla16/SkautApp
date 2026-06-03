using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using SkautApp.Models;
using SkautApp.Services;
using Microsoft.Extensions.DependencyInjection; // Tohle je důležité pro AddScoped

namespace SkautApp
{
    public class ModryZivotComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Tady registrujeme naši službu, aby o ní Controller věděl
            builder.Services.AddScoped<ModryZivotService>();

            // Tady říkáme Umbracu, aby při startu zkontrolovalo databázi
            builder.Services.AddSingleton<ModryZivotMigrationPlan>();
        }
    }

    public class ModryZivotMigrationPlan : MigrationPlan
    {
        public ModryZivotMigrationPlan() : base("ModryZivot")
        {
            From(string.Empty).To<AddModryZivotTable>("first-migration");
        }
    }

    public class AddModryZivotTable : MigrationBase
    {
        public AddModryZivotTable(IMigrationContext context) : base(context) { }

        protected override void Migrate()
        {
            if (!TableExists("ModryZivotZapis"))
            {
                Create.Table<ModryZivotZapis>().Do();
            }
        }
    }
}