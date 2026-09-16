using Microsoft.Data.Sqlite;
using Umbraco.Cms.Core.Services;
using System.Collections.Generic;
using System.Linq; // Důležité pro žebříček družin

namespace SkautApp.Services
{
    // Pomocná třída pro data v žebříčku
    public class ZebricekItem
    {
        public string Jmeno { get; set; } = string.Empty;
        public string Druzina { get; set; } = string.Empty;
        public decimal Body { get; set; }
    }

    public class ModryZivotService
    {
        private readonly string _connectionString;
        private readonly IMemberService _memberService;

        public ModryZivotService(IMemberService memberService)
        {
            _memberService = memberService;
            var dataDirectory = AppContext.GetData("DataDirectory")?.ToString()
                ?? Path.Combine(AppContext.BaseDirectory, "umbraco", "Data");
            _connectionString = $"Data Source={Path.Combine(dataDirectory, "ModryZivot.db")};Cache=Shared;";
            InitDatabase();
        }

        private void InitDatabase()
        {
            using (var db = new SqliteConnection(_connectionString))
            {
                db.Open();
                var sql = @"CREATE TABLE IF NOT EXISTS ModryZivot (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            MemberId INTEGER,
                            VyzvaId TEXT,
                            Datum TEXT,
                            Splneno INTEGER,
                            UNIQUE(MemberId, VyzvaId, Datum)
                        )";
                var command = new SqliteCommand(sql, db);
                command.ExecuteNonQuery();
            }
        }

        // Metoda pro zápis (už máš hotovou)
        public void ZapisVyzvu(int memberId, string vyzvaId, DateTime datum, bool splneno)
        {
            using (var db = new SqliteConnection(_connectionString))
            {
                db.Open();
                var sql = @"INSERT INTO ModryZivot (MemberId, VyzvaId, Datum, Splneno) 
                            VALUES (@mid, @vid, @dat, @spl)
                            ON CONFLICT(MemberId, VyzvaId, Datum) 
                            DO UPDATE SET Splneno = @spl";

                var command = new SqliteCommand(sql, db);
                command.Parameters.AddWithValue("@mid", memberId);
                command.Parameters.AddWithValue("@vid", vyzvaId);
                command.Parameters.AddWithValue("@dat", datum.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@spl", splneno ? 1 : 0);
                command.ExecuteNonQuery();
            }
        }

        // Metoda pro kontrolu (už máš hotovou)
        public bool JeSplneno(int memberId, string vyzvaId, DateTime datum)
        {
            using (var db = new SqliteConnection(_connectionString))
            {
                db.Open();
                var sql = "SELECT Splneno FROM ModryZivot WHERE MemberId = @mid AND VyzvaId = @vid AND Datum = @dat";
                var command = new SqliteCommand(sql, db);
                command.Parameters.AddWithValue("@mid", memberId);
                command.Parameters.AddWithValue("@vid", vyzvaId);
                command.Parameters.AddWithValue("@dat", datum.ToString("yyyy-MM-dd"));

                var result = command.ExecuteScalar();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }

        // Metoda pro získání historie splněných úkolů pro heatmapu
        public Dictionary<DateTime, int> GetHistoriePocetSplnenych(int memberId, int pocetDni)
        {
            var historie = new Dictionary<DateTime, int>();
            using (var db = new SqliteConnection(_connectionString))
            {
                db.Open();
                var startDate = DateTime.Today.AddDays(-pocetDni + 1);
                var sql = @"SELECT Datum, COUNT(Id) 
                            FROM ModryZivot 
                            WHERE MemberId = @mid AND Splneno = 1 AND Datum >= @startDate 
                            GROUP BY Datum 
                            ORDER BY Datum ASC";

                var command = new SqliteCommand(sql, db);
                command.Parameters.AddWithValue("@mid", memberId);
                command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dateString = reader.GetString(0);
                        var count = reader.GetInt32(1);
                        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                        {
                            historie[date] = count;
                        }
                    }
                }
            }
            return historie;
        }

        // --- NOVÉ METODY PRO ŽEBŘÍČKY ---

        // Žebříček jednotlivců
        public List<ZebricekItem> GetZebricekJednotlivcu()
        {
            var list = new List<ZebricekItem>();
            using (var db = new SqliteConnection(_connectionString))
            {
                db.Open();
                // Spočítáme řádky, kde Splneno = 1 pro každého uživatele
                var sql = @"SELECT MemberId, COUNT(*) as Pocet 
                            FROM ModryZivot 
                            WHERE Splneno = 1 
                            GROUP BY MemberId 
                            ORDER BY Pocet DESC";

                var command = new SqliteCommand(sql, db);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var memberId = reader.GetInt32(0);
                        var body = reader.GetInt32(1);

                        // Vytáhneme info o členovi z Umbraca
                        var member = _memberService.GetById(memberId);
                        if (member != null)
                        {
                            list.Add(new ZebricekItem
                            {
                                Jmeno = member.Name ?? string.Empty,
                                Druzina = member.GetValue<string>("druzina") ?? "Bez družiny",
                                Body = body
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Žebříček družin (průměr splněných výzev na člena družiny)
        public List<ZebricekItem> GetZebricekDruzin()
        {
            var jednotlivci = GetZebricekJednotlivcu();
            var pocetClenuDruziny = _memberService
                .GetAllMembers()
                .GroupBy(member => member.GetValue<string>("druzina") ?? "Bez družiny")
                .ToDictionary(group => group.Key, group => group.Count());

            return jednotlivci
                .GroupBy(x => x.Druzina)
                .Select(g => new ZebricekItem
                {
                    Druzina = g.Key,
                    Body = pocetClenuDruziny.TryGetValue(g.Key, out var pocetClenu) && pocetClenu > 0
                        ? g.Sum(x => x.Body) / pocetClenu
                        : 0
                })
                .OrderByDescending(x => x.Body)
                .ToList();
        }
    }
}