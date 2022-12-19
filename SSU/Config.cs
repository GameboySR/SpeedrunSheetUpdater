using System.Text;

namespace IL_Loader
{
    /// <summary>
    /// Class which traverses the file, loads configuration
    /// and processes the leaderboards data into a usable form.
    /// It's also disgustingly long but who cares about formatting anyway, haha.
    /// </summary>
    public class Config
    {


        /// <summary>
        ///<para>Dictionary values representation (definitely didn't initially write all of this like a ****** instead of using the map)</para>
        /// 
        ///<para>public string CommentCell { get; set; } = "";           | comment=A1</para>
        ///<para>public string DateCell { get; set; } = "";              | date=A1</para>
        ///<para>public string EmulatorCell { get; set;  } = "";         | emulator=A1</para>
        ///<para>public string InGameTimeCell { get; set; } = "";        | ingametime=A1</para>
        ///<para>public string LockCell { get; set;  } = "";             | lock=A1</para>
        ///<para>public string PlatformCell { get; set; } = "";          | platform=A1</para>
        ///<para>public string PlayerCell { get; set; } = "";            | player=A1</para>
        ///<para>public string PrimaryTimeCell { get; set; } = "";       | primarytime=realtime/realtimeloadless/ingametime</para>
        ///<para>public string RealTimeCell { get; set; } = "";          | realtime=A1</para>
        ///<para>public string RealTimeLoadlessCell { get; set; } = "";  | realtimeloadless=A1</para>
        ///<para>public string RegionCell { get; set; } = "";            | region=A1</para>
        ///<para>public string Sheet { get; set; } = "";                 | spreadsheet=sheet_link - mandatory</para>
        ///<para>public string VideoLinkCell { get; set; } = "";         | videolink=A1</para>
        ///
        ///<para>private string _game = "";                              | game=game_name - mandatory</para>
        ///<para>private bool _emulator = true;                          | emulatorallowed=false</para>
        ///<para>private bool _verifiedOnly = false;                     | verifiedonly=true</para>
        /// </summary>
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
        public List<string> Range = new List<string>();

        private bool _configParsed = false;
        private bool _emulator = true;
        private bool _verifiedOnly = false;
        private readonly string _path;

        private readonly string LEADERBOARD_LINK = @"https://www.speedrun.com/api/v1/leaderboards/";
        private readonly string LEVEL_LINK = @"https://www.speedrun.com/api/v1/levels/";
        private readonly string PLATFORM_LINK = @"https://www.speedrun.com/api/v1/platforms/";
        private readonly string REGION_LINK = @"https://www.speedrun.com/api/v1/regions/";

        private HttpClient _client;

        /// <summary>
        /// Creates an instance of a config parser.
        /// </summary>
        /// <param name="path">Path to the config file.</param>
        public Config(string path, ref HttpClient client)
        {
            _path = path;
            _client = client;
        }

        /// <summary>
        /// Loads the configuration file, parses it and downloads leaderboards data.
        /// It could be written more efficiently, so the config isn't parsed each time but eeeeeeeeeeh
        /// Maybe later
        /// </summary>
        /// <returns>A list of processed leaderboards data.</returns>
        public List<LeaderboardData?> Parse()
        {
            List<LeaderboardData?> result = new List<LeaderboardData?>();

            using (var fileStream = File.OpenRead(_path))
            {
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, true))
                {
                    string? line;

                    // main loop
                    while ((line = reader.ReadLine()) != null)
                    {
                        // ignore comment lines, empty lines and first line
                        if (line == "" || line[0] == '#' || line.StartsWith("[CONFIG]"))
                        {
                            continue;
                        }

                        // check if we're still in the [CONFIG] section
                        if (!_configParsed)
                        {
                            // check if we're done parsing [CONFIG] and if [CONFIG] has game name and sheet link
                            // also set active filters
                            if (line.StartsWith("[LEADERBOARDS]"))
                            {
                                _configParsed = true;

                                // check if necessary parts of config are there
                                if (!Parameters.ContainsKey("game") ||
                                    !Parameters.ContainsKey("spreadsheet") ||
                                    !Parameters.ContainsKey("primarytime"))
                                {
                                    throw new FormatException("Game, Google Sheet or primary time type missing in the [CONFIG] section.");
                                }

                                // set filters
                                if (Parameters.ContainsKey("emulatorallowed") &&
                                    bool.TryParse(Parameters["emulatorallowed"], out bool _) &&
                                    !bool.Parse(Parameters["emulatorallowed"]))
                                {
                                    _emulator = false;
                                }

                                if (Parameters.ContainsKey("verifiedonly") &&
                                    bool.TryParse(Parameters["verifiedonly"], out bool _) &&
                                    bool.Parse(Parameters["verifiedonly"]))
                                {
                                    _verifiedOnly = true;
                                }
                                
                                // get range for Spreadsheet call
                                // first to values are beginning and end of the row range
                                // third value is the primary time cell index within all cells
                                var sorted = Parameters.Values.Where(value => char.IsUpper(value[0]) && char.IsDigit(value[1])).ToList();
                                sorted.Sort();
                                Range.Add(sorted[0]);
                                Range.Add(sorted[sorted.Count - 1]);
                                int index = 0;

                                foreach (var pair in Parameters)
                                {
                                    if (pair.Key == Parameters["primarytime"])
                                    {
                                        Range.Add(index.ToString());
                                        break;
                                    }
                                    index++;
                                }

                                Console.WriteLine("Emulator runs: " + (_emulator ? "enabled." : "disabled."));
                                Console.WriteLine("Verfied-only runs: " + (_verifiedOnly ? "enabled." : "disabled."));
                                Console.WriteLine("Config successfully parsed.\n\n====================================\n");
                                continue;
                            }
                            ParseConfig(line);
                            continue;
                        }

                        var leaderboard = FetchLeaderboard(line);
                        
                        if (leaderboard == null || leaderboard.data == null)
                        {
                            Console.Error.WriteLine("Failed to fetch leaderboard: " + line);
                            Console.WriteLine("Skipping leaderboard…");
                            result.Add(null);
                             continue;
                        }

                        GetFastestRun(leaderboard);
                        leaderboard.data.level = FormatLeaderboardName(leaderboard.data.level);

                        if (leaderboard.data.runs == null)
                        {
                            Console.WriteLine("No elligible runs for " + leaderboard.data.level);
                            result.Add(null);
                            continue;
                        }

                        FormatLeaderboard(leaderboard.data.runs[0]);
                        result.Add(leaderboard);
                        Console.WriteLine(leaderboard.data.level + " successully formatted.");
                    }
                    Console.WriteLine("All elligible leaderboards successfully formatted.\n\n====================================\n");
                }
            }

            return result;
        }

        /// <summary>
        /// Makes 3 attempts at fetching the leaderboard info,
        /// waiting 5 and 10 seconds between each attempt
        /// </summary>
        /// <param name="line">Leaderboard ID</param>
        /// <returns></returns>
        private LeaderboardData? FetchLeaderboard(string line)
        {
            for (int i = 1; i < 3; i++)
            {
                var leaderboard = SendRequest(LEADERBOARD_LINK + BuildLeaderboardRequestParams(line))
                .Content.ReadAsAsync<LeaderboardData>().Result;

                if (leaderboard == null || leaderboard.data == null)
                {
                    Console.Error.WriteLine("Failed to fetch leaderboard: " + line);
                    Console.WriteLine("Retrying in " + 5 * i + " seconds…");
                    Thread.Sleep(5 * i * 1000);
                    continue;
                }

                return leaderboard;
            }

            return null;
        }

        /// <summary>
        /// Sends an HTTP request and returns the response.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private HttpResponseMessage SendRequest(string request)
        {
            var task = _client.GetAsync(request);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Transforms data where needed into a readily readable form.
        /// </summary>
        /// <param name="leaderboard"></param>
        private void FormatLeaderboard(LeaderboardEntry leaderboard)
        {
            if (leaderboard.run == null || leaderboard.run.times == null ||
                leaderboard.run.system == null || leaderboard.run.videos == null)
            {
                return;
            }

            //format times
            if (leaderboard.run.times.realtime != null)
            {
                leaderboard.run.times.realtime = FormatTime(leaderboard.run.times.realtime);
            }

            if (leaderboard.run.times.realtime_noloads != null)
            {
                leaderboard.run.times.realtime_noloads = FormatTime(leaderboard.run.times.realtime_noloads);
            }

            if (leaderboard.run.times.ingame != null)
            {
                leaderboard.run.times.ingame = FormatTime(leaderboard.run.times.ingame);
            }

            // format platform name
            if (leaderboard.run.system.platform != null)
            {
                leaderboard.run.system.platform = FormatPlatform(leaderboard.run.system.platform);
            }
            
            // format player names
            if (leaderboard.run.players != null)
            {
                leaderboard.run.players = FormatPlayers(leaderboard.run.players);
            }

            // format region name
            if (leaderboard.run.system.region != null)
            {
                leaderboard.run.system.region = FormatRegion(leaderboard.run.system.region);
            }
        }

        /// <summary>
        /// Fetches the name of the leaderboard
        /// </summary>
        /// <param name="level">Leaderboard speedrun.com ID</param>
        /// <returns></returns>
        private string FormatLeaderboardName(string? level)
        {
            if (level == null)
            {
                return "";
            }

            Level fetchedLevel = SendRequest(LEVEL_LINK + level).Content.ReadAsAsync<Level>().Result;

            if (fetchedLevel == null || fetchedLevel.data == null || fetchedLevel.data.name == null)
            {
                return "";
            }

            return fetchedLevel.data.name;
        }

        /// <summary>
        /// Fetches region name.
        /// </summary>
        /// <param name="region">Region spedrun.com ID</param>
        /// <returns>Region name</returns>
        private string FormatRegion(string region)
        {
            Region fetchedRegion = SendRequest(REGION_LINK + region).Content.ReadAsAsync<Region>().Result;

            if (fetchedRegion == null || fetchedRegion.data == null || fetchedRegion.data.name == null)
            {
                return "";
            }

            return fetchedRegion.data.name;
        }

        /// <summary>
        /// Fetches player's name.
        /// </summary>
        /// <param name="playerList">List of player uri requests.</param>
        /// <returns>List of user speedrun.com account links.</returns>
        private List<Player>? FormatPlayers(List<Player> playerList)
        {
            if (playerList.Count == 0)
            {
                return playerList;
            }

            var result = new List<Player>();

            foreach(var player in playerList)
            {
                var task = _client.GetAsync(player.uri);
                task.Wait();
                var response = task.Result;
                FetchedPlayer fetched = response.Content.ReadAsAsync<FetchedPlayer>().Result;

                if (fetched == null || fetched.data == null || fetched.data.weblink == null)
                {
                    continue;
                }

                player.uri = fetched.data.weblink;

                result.Add(player);
            }
            return result;
        }

        /// <summary>
        /// Fetches the name of the platform the run was done on.
        /// </summary>
        /// <param name="platform">speedrun.com platform ID</param>
        /// <returns>Name of the platform</returns>
        private string FormatPlatform(string platform)
        {
            Platform plat = SendRequest(PLATFORM_LINK + platform).Content.ReadAsAsync<Platform>().Result;

            if (plat == null || plat.data == null || plat.data.name == null)
            {
                return "";
            }

            return plat.data.name;
        }

        /// <summary>
        /// Converts speedrun.com time to a format usable in Google Sheets.
        /// </summary>
        /// <param name="time">Run time in the format (PT)xxHxxMxx.xxxS</param>
        /// <returns>Formatted time string to be used in the sheet.</returns>
        private string FormatTime(string time)
        {
            if (time.Substring(0, 2) == "PT")
            {
                time = time.Substring(2);
            }

            string newTime = "";

            var parts = time.Split('H');
             if (parts.Length != 1)
            {
                newTime += parts[0] + ':';
            }

            parts = parts[parts.Length - 1].Split('M');
            if (parts.Length != 1)
            {
                newTime += parts[0] + ':';
            }

            parts = parts[parts.Length - 1].Split('S');
            if (parts.Length == 2 && parts[1] == "")
            {
                newTime += parts[0];
            }

            return newTime;
        }
        
        /// <summary>
        /// Puts together the leaderboard-specific part of the request link
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string BuildLeaderboardRequestParams(string line)
        {
            var levelCategory = line.Split('-');
            return Parameters["game"] + "/level/" + levelCategory[0] + '/' + levelCategory[1];
        }

        /// <summary>
        /// Applies filters and replaces the runs list with a single elligible run
        /// If no run is elligible or present, list is replaced with null
        /// </summary>
        /// <param name="leaderboard"></param>
        private void GetFastestRun(LeaderboardData leaderboard)
        {
            if (leaderboard == null || leaderboard.data == null ||
                leaderboard.data.runs == null)
            {
                return;
            }

            if (leaderboard.data.runs.Count == 0)
            {
                leaderboard.data.runs = null;
                return;
            }

            List<LeaderboardEntry> fastestRun = new List<LeaderboardEntry>();
            LeaderboardEntry? candidate = FilterRuns(leaderboard.data.runs);

            if (candidate == null) 
            {
                leaderboard.data.runs = null;
                return;
            }

            fastestRun.Add(candidate);
            leaderboard.data.runs = fastestRun;
        }

        /// <summary>
        /// Applies filter to a list of runs and returns the fastest run
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        private LeaderboardEntry? FilterRuns(List<LeaderboardEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry.run != null && entry.run.system != null && entry.run.status != null)
                {
                    if (!_emulator)
                    {
                        if (entry.run.system.emulated)
                        {
                            continue;
                        }
                    }

                    if (_verifiedOnly)
                    {
                        if (entry.run.status.status != "verified")
                        {
                            continue;
                        }
                    }

                    if (Parameters.Keys.Contains("subcategory"))
                    {
                        var subcat = Parameters["subcategory"].Split('.');
                        if (entry!.run!.values![subcat[0]] != subcat[1])
                        {
                            continue;
                        }
                    }
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if cell fulfills the right format.
        /// Only columns A-Z are allowed
        /// The rest of the cell has to be a number
        /// </summary>
        /// <param name="cell">Cell string</param>
        /// <returns></returns>
        private bool IsCell(string cell)
        {
            return cell.Length >= 2 && Char.IsUpper(cell[0]) &&
                int.TryParse(cell.Substring(1), out int _);
        }

        /// <summary>
        /// Parses a line of the [CONFIG] section, checks the basic format.
        /// If correct, adds it to <see cref="Parameters"/>
        /// </summary>
        /// <param name="line">A line of the [CONFIG] section</param>
        private void ParseConfig(string line)
        {
            var parts = line.Split('=');

            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Invalid [CONFIG] parameter format: \"" + line + '"');
                return;
            }

            // parameters which don't have cells as values
            if (parts[0] != "emulatorallowed" && parts[0] != "game" &&
                parts[0] != "spreadsheet" && parts[0] != "verifiedonly" &&
                parts[0] != "primarytime" && parts[0] != "subcategory")
            {
                if (!IsCell(parts[1]))
                {
                    Console.Error.WriteLine("Invalid Sheet cell format in [CONFIG]: \"" + line + '"');
                    return;
                }
            }

            Parameters.Add(parts[0], parts[1]);
        }
    }
}
