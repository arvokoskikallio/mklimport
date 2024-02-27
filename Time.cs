using System.ComponentModel.DataAnnotations;
namespace PPImport
{
    public class Time
    {

        public Time() {

        }

        public Time(DateTime date, int playerId, int track, bool glitch, bool flap, int runTime, string link, string ghost) {
            Date = date;
            PlayerId = playerId;
            Track = track;
            Glitch = glitch;
            Flap = flap;
            RunTime = runTime;
            Link = link;
            Ghost = ghost;
        }

        public int Id { get; set; }

        public DateTime? Date { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required]
        public int Track { get; set; }

        [Required]
        public bool Glitch { get; set; }

        [Required]
        public bool Flap { get; set; }

        [Required]
        public int RunTime { get; set; }

        public string? Link { get; set; }

        public string? Ghost { get; set; }
    }
}