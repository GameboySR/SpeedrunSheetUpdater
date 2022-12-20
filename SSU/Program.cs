using IL_Loader;

using System.Net.Http.Headers;
using System.Reflection;

class Program
{
    /// <summary>
    /// Main
    /// </summary>
    /// <param name="args">Both optional, first is the config path, second is the loop period in minutes.</param>
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("\nSpeedrun Spreadsheet Updater " +
            Assembly.GetExecutingAssembly().GetName().Version!.ToString(3));
        Console.WriteLine("\n====================================\n");

        string configPath = "";
        int timer = 60 * 60 * 1000; // 60 minutes - 3 600 seconds - 3 600 000 miliseconds

        if (args.Length == 0)
        {
            configPath = "config.txt";
        }
        else
        {
            configPath = args[0];

            if (args.Length > 1)
            {
                timer = int.Parse(args[1]) * 60 * 1000; // x minutes - 60*x seconds - 60 000*x miliseconds
            }
        }

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Config config = new Config(configPath, ref client);
        config.Parse();

        MainLoop(ref config, timer);
    }

    private static void MainLoop(ref Config config, int timer)
    {
        while (true)
        {
            // clean up things that went out of scope after each cycle
            GC.Collect();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            config.SetClient(ref client);

            try
            {
                var leaderboards = config.FetchLeaderboards();
                GoogleSheetsClient sheetClient = new GoogleSheetsClient(config.Parameters["spreadsheet"]);

                foreach (var leaderboard in leaderboards)
                {
                    if (leaderboard != null)
                    {
                        sheetClient.UpdateSheet(config.Parameters, leaderboard, config.Range);
                    }
                    IncrementCells(ref config.Parameters);

                    var sorted = config.Parameters.Values.Where(value => char.IsUpper(value[0]) && char.IsDigit(value[1])).ToList();
                    sorted.Sort();
                    config.Range[0] = sorted[0];
                    config.Range[1] = sorted[sorted.Count - 1];
                    sorted.Clear();
                }

                Console.WriteLine("\nSheet: " +
                    config.Parameters["spreadsheet"].Split('|')[1] +
                    " has been updated.\n\n" +
                    "Next update will be at " + DateTime.Now.AddMilliseconds(timer) +
                    "\n\n====================================\n");

                // resetting cell-related variables
                // back to the starting cells from the config file
                config.ResetParameters();
                config.GetRange();
                // resetting clients and leaderboards to prevent memory growth
                client.Dispose();
                sheetClient.ResetService();
                leaderboards.Clear();

                Thread.Sleep(timer);
                Console.Clear();
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine(ex.Message);
                client.Dispose();
            }
            catch (NullReferenceException ex)
            {
                Console.Error.WriteLine(ex.Message);
                client.Dispose();
            }
        }
    }

    /// <summary>
    /// Increment the cell numbers to move the to the next row.
    /// </summary>
    /// <param name="parms"></param>
    private static void IncrementCells(ref Dictionary<string, string> parms)
    {
        foreach (var key in parms.Keys)
        {
            // parameters which don't have cells as values
            if (key == "game" || key == "spreadsheet" ||
                key == "emulatorallowed" || key == "verifiedonly" ||
                key == "primarytime" || key == "subcategory")
            {
                continue;
            }
            parms[key] = parms[key][0] + (int.Parse(parms[key].Substring(1)) + 1).ToString();
        }
    }
}