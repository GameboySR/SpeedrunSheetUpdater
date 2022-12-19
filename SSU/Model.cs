using Newtonsoft.Json;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace IL_Loader
{
    public class LeaderboardData
    {
        public Data? data { get; set; } = null;

    }

    public class Data
    {
        public List<LeaderboardEntry>? runs { get; set; } = null;
        public string? level { get; set; } = null;
        public string? timing { get; set; } = null;
    }

    public class LeaderboardEntry
    {
        public int? place { get; set; } = null;
        public Run? run { get; set; } = null;
    }

    public class Run
    {
        public Video? videos { get; set; } = null;
        public string? comment { get; set; } = null;
        public Status? status { get; set; } = null;
        public List<Player>? players { get; set; } = null;
        public string? date { get; set; } = null;
        public RunSystem? system { get; set; } = null;
        public Time? times { get; set; } = null;
        public Dictionary<string, string>? values { get; set; } = null;
    }

    public class Video
    {
        public List<Link>? links { get; set; } = null;
    }

    public class Link
    {
        public string? uri { get; set; } = null;
    }

    public class Status
    {
        public string? status { get; set; } = null;
        public string? examiner { get; set; } = null;

        [JsonProperty(PropertyName = "verify-date")]
        public string? verify_date { get; set; } = null;
    }

    public class Player
    {
        public string? uri { get; set; } = null;
    }

    public class RunSystem
    {
        public string? platform { get; set; } = null;
        public bool emulated { get; set; } = false;
        public string? region { get; set; } = null;
    }

    public class Time
    {
        public string? primary { get; set; } = null;
        public string? realtime { get; set; } = null;
        public string? realtime_noloads { get; set; } = null;
        public string? ingame { get; set; } = null;
    }

    public class Platform
    {
        public PlatformData? data { get; set; } = null;
    }

    public class PlatformData
    {
        public string? name { get; set; } = null;
    }

    public class FetchedPlayer
    {
        public PlayerData? data { get; set; } = null;
    }

    public class PlayerData
    {
        public string? weblink { get; set; } = null;
    }

    public class Region
    {
        public RegionData? data { get; set; } = null;
    }

    public class RegionData
    {
        public string? name { get; set; } = null;
    }

    public class Level
    {
        public LevelData? data { get; set; } = null;
    }

    public class LevelData
    {
        public string? name { get; set; } = null;
    }
}
