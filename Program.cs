using Dapper;
using System.Data.SqlClient;
using System.Globalization;

namespace MKLImport
{
    class Program
    {

        //TODO - import all times for MKL
        //TODO - edit PP script to add times that are not found in the MKL dumps (this is where you need weird logic)
        private static string[] contents = File.ReadAllText(@"C:\Users\arvok\mklimport\config.txt").Split("\r\n");
        private static string _connectionString = contents[0];
        private static DateTime minDate = DateTime.Parse("2008-04-09");

        static async Task Main(string[] args)
        {
            string folderPath = @"C:\Users\arvok\mklimport\profiles";

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

        static Player MapPlayer(MarioKartData data)
        {
            return new Player
                {
                    Name = data.Name,
                    Country = Enum.Parse<Country>(data.Country.ToUpper())
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
                    Track = Array.IndexOf(tracks.ToArray(), tracks.First(t => t == timeEntry.Track)),
                    Glitch = timeEntry.Glitch,
                    Flap = false,
                    RunTime = timeEntry.Time,
                    Link = timeEntry.Video,
                    Ghost = timeEntry.Ghost
                });
            }
            return times;
        }

        static async Task<int> PushPlayer(Player player)
        {
            string sqlQuery = "INSERT INTO Players (Name, Country)" +
            "VALUES (@Name, @Country); SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sqlQuery, player);
        }

        private static async void PushTime(Time time)
        {
            if(time.Date < minDate) {
                time.Date = null;
            }

            string sqlQuery = "INSERT INTO Times (PlayerId, Date, Track, Glitch, Flap, RunTime, Link, Ghost, Obsoleted)" +
            "VALUES (@PlayerId, @Date, @Track, @Glitch, @Flap, @RunTime, @Link, @Ghost, 0)";


            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sqlQuery, time);
        }
    }

    public class TimeEntry
    {
        public string Date { get; set; }
        public string Track { get; set; }
        public int Time { get; set; }
        public bool Glitch { get; set; }
        public string Video { get; set; }
        public string Ghost { get; set; }
    }

    public class MarioKartData
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public List<TimeEntry> Times { get; set; }
    }
}