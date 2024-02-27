using System.ComponentModel.DataAnnotations;

namespace PPImport
{
    public class Player
    {
        public Player() {
            Name = "";
        }

        public Player(string name, Country country, string town, string otherInfo, string discord, string ppProofStatus) {
            Name = name;
            Country = country;
            Town = town;
            OtherInfo = otherInfo;
            Discord = discord;
            PPProofStatus = ppProofStatus;
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Country Country { get; set; }

        public string? Town { get; set; }

        public string? OtherInfo { get; set; }

        public string? Discord { get; set; }

        public string? PPProofStatus { get; set; }
    }
}