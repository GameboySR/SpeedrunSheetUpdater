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
        public string? level { get; set; } = "empty";
        public string? timing { get; set; } = "empty";
    }

    public class LeaderboardEntry
    {
        public int? place { get; set; } = null;
        public Run? run { get; set; } = null;
    }

    public class Run
    {
        public Video? videos { get; set; } = null;
        public string? comment { get; set; } = "empty";
        public Status? status { get; set; } = null;
        public List<Player>? players { get; set; } = null;
        public string? date { get; set; } = "empty";
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
        public string? uri { get; set; } = "empty";
    }

    public class Status
    {
        public string? status { get; set; } = "empty";
        public string? examiner { get; set; } = "empty";

        [JsonProperty(PropertyName = "verify-date")]
        public string? verify_date { get; set; } = "empty";
    }

    public class Player
    {
        public string? uri { get; set; } = "empty";
    }

    public class RunSystem
    {
        public string? platform { get; set; } = "empty";
        public bool emulated { get; set; } = false;
        public string? region { get; set; } = "empty";
    }

    public class Time
    {
        public string? primary { get; set; } = "empty";
        public string? realtime { get; set; } = "empty";
        public string? realtime_noloads { get; set; } = "empty";
        public string? ingame { get; set; } = "empty";
    }

    public class Platform
    {
        public PlatformData? data { get; set; } = null;
    }

    public class PlatformData
    {
        public string? name { get; set; } = "empty";
    }

    public class FetchedPlayer
    {
        public PlayerData? data { get; set; } = null;
    }

    public class PlayerData
    {
        public string? weblink { get; set; } = "empty";
    }

    public class Region
    {
        public RegionData? data { get; set; } = null;
    }

    public class RegionData
    {
        public string? name { get; set; } = "empty";
    }

    public class Level
    {
        public LevelData? data { get; set; } = null;
    }

    public class LevelData
    {
        public string? name { get; set; } = "empty";
    }
}
