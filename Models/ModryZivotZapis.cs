using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkautApp.Models
{
    [TableName("ModryZivotZapis")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class ModryZivotZapis
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("MemberKey")] // Použijeme GUID člena z Umbraca
        public Guid MemberKey { get; set; }

        [Column("VyzvaId")]
        public int VyzvaId { get; set; }

        [Column("Datum")]
        public DateTime Datum { get; set; }

        [Column("Splneno")]
        public bool Splneno { get; set; }
    }
}