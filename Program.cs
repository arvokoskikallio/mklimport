using Dapper;
using System.Data.SqlClient;
using System.Globalization;

namespace PPImport
{
    class Program
    {
        private static string[] contents = File.ReadAllText(@"C:\Users\Arvo Koskikallio\ppimport\config.txt").Split("\r\n");
        private static string _connectionString = contents[0];
        private static DateTime minDate = DateTime.Parse("2008-04-09");

        static async Task Main(string[] args)
        {
            string folderPath = @"C:\Users\Arvo Koskikallio\mklimport\profiles";

            string[] files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                List<MarioKartData> marioKartData = new List<MarioKartData>();

                using (StreamReader r = new StreamReader(file))
                {
                    string json = r.ReadToEnd();
                    marioKartData.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<MarioKartData>(json));
                }

                foreach (var data in marioKartData)
                {
                    // Map to List<Player> and List<Time>
                    var player = MapPlayer(data);
                    var playerId = await PushPlayer(player);
                    List<Time> times = MapTimes(data);

                    foreach (var time in times)
                    {
                        time.PlayerId = playerId;
                        PushTime(time);
                    }
                }
            }
        }

        static Country GetCountryByLongName(string fullName)
        {
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo region = new RegionInfo(ci.Name);
                if (region.EnglishName == fullName)
                {
                    return Enum.Parse<Country>(region.TwoLetterISORegionName);
                }
            }

            if(fullName == "UK") {
                return Country.GB;
            }

            if(fullName == "USA") {
                return Country.US;
            }

            if(fullName == "Ivory Coast") {
                return Country.CI;
            }

            if(fullName == "Dominican Rep") {
                return Country.DO;
            }

            if(fullName == "UAE") {
                return Country.AE;
            }

            if(fullName == "Bosnia") {
                return Country.BA;
            }

            if(fullName == "South Korea") {
                return Country.KR;
            }


            return Country.Unknown;
        }

        static Player MapPlayer(MarioKartData data)
        {
            return new Player
                {
                    Name = data.Info.Name,
                    Country = GetCountryByLongName(data.Info.Country),
                    Town = data.Info.Town,
                    OtherInfo = data.Info.OtherInfo.Replace(" (", ""),
                    PPProofStatus = data.Info.ProofStatus
                };
        }

        static List<Time> MapTimes(MarioKartData data)
        {
            List<string> tracks = new List<string>
            {
                "Luigi Circuit",
                "Moo Moo Meadows",
                "Mushroom Gorge",
                "Toad's Factory",
                "Mario Circuit",
                "Coconut Mall",
                "DK's Snowboard Cross",
                "Wario's Gold Mine",
                "Daisy Circuit",
                "Koopa Cape",
                "Maple Treeway",
                "Grumble Volcano",
                "Dry Dry Ruins",
                "Moonview Highway",
                "Bowser's Castle",
                "Rainbow Road",
                "GCN Peach Beach",
                "DS Yoshi Falls",
                "SNES Ghost Valley 2",
                "N64 Mario Raceway",
                "N64 Sherbet Land",
                "GBA Shy Guy Beach",
                "DS Delfino Square",
                "GCN Waluigi Stadium",
                "DS Desert Hills",
                "GBA Bowser Castle 3",
                "N64 DK's Jungle Parkway",
                "GCN Mario Circuit",
                "SNES Mario Circuit 3",
                "DS Peach Gardens",
                "GCN DK Mountain",
                "N64 Bowser's Castle",
            };

            var times = new List<Time>();

            foreach (var timeEntry in data.Times)
            {
                times.Add(new Time
                {
                    Date = DateTime.Parse(timeEntry.Date),
                    Track = Array.IndexOf(tracks.ToArray(), tracks.First(t => t == timeEntry.Track)),
                    Glitch = timeEntry.Glitch,
                    Flap = timeEntry.Flap,
                    RunTime = timeEntry.m*60*1000 + timeEntry.s*1000 + timeEntry.ms,
                    Link = timeEntry.Video
                });
            }
            return times;
        }

        static async Task<int> PushPlayer(Player player)
        {
            string sqlQuery = "INSERT INTO Players (Name, Country, Town, OtherInfo, PPProofStatus)" +
            "VALUES (@Name, @Country, @Town, @OtherInfo, @PPProofStatus); SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sqlQuery, player);
        }

        private static async void PushTime(Time time)
        {
            if(time.Date < minDate) {
                time.Date = null;
            }

            string sqlQuery = "INSERT INTO Times (PlayerId, Date, Track, Glitch, Flap, RunTime, Link, Obsoleted)" +
            "VALUES (@PlayerId, @Date, @Track, @Glitch, @Flap, @RunTime, @Link, 0)";


            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sqlQuery, time);
        }
    }

    public class TimeEntry
    {
        public string Date { get; set; }
        public string Name { get; set; }
        public string Track { get; set; }
        public int m { get; set; }
        public int s { get; set; }
        public int ms { get; set; }
        public bool Flap { get; set; }
        public bool Glitch { get; set; }
        public string Video { get; set; }
    }

    public class PlayerInfo
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public string Town { get; set; }
        public string OtherInfo { get; set; }
        public string ProofStatus { get; set; }
    }

    public class MarioKartData
    {
        public PlayerInfo Info { get; set; }
        public List<TimeEntry> Times { get; set; }
    }
}