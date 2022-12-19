using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;

namespace IL_Loader
{
    public class GoogleSheetsClient
    {
        private readonly string? CLIENT_SECRET = Environment.GetEnvironmentVariable("SpeedrunSpreadsheetUpdaterSecret");
        private readonly string NAME = "IL Sheet Loader";
        private readonly string[] SCOPE = { SheetsService.Scope.Spreadsheets };

        private SheetsService? _sheetsService;
        private readonly string[] _sheetInfo;
        private UserCredential? _userCredential;
        
        /// <summary>
        /// Initializes the Google API service, takes care of authentication
        /// and authorization.
        /// </summary>
        /// <param name="sheet"></param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public GoogleSheetsClient(string sheet)
        {
            _sheetInfo = sheet.Split('|');

            if (_sheetInfo.Length != 2 || _sheetInfo[1] == "")
            {
                throw new FormatException("Sheet parameter doesn't have the correct format - Spreadsheet_id|Sheet_id\n" +
                    "Actual parameter: " + sheet);
            }

            // sensitive data - credentials and client secret - are stored
            // in environment variables
            string credentialsPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);

            _userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromFile(CLIENT_SECRET).Secrets,
                SCOPE,
                "user",
                CancellationToken.None,
                new FileDataStore(credentialsPath, true)).Result;

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _userCredential,
                ApplicationName = NAME
            });

            if (_sheetsService == null)
            {
                throw new NullReferenceException("Couldn't initialize Sheets service.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="data"></param>
        /// <param name="range"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void UpdateSheet(Dictionary<string, string> parameters, LeaderboardData data, List<string> range)
        {
            if (_sheetsService == null)
            {
                throw new NullReferenceException("Sheets service is null.");
            }

            string oldValue = "";

            // A1 range format as per https://developers.google.com/sheets/api/guides/concepts#expandable-1
            string rangeString = _sheetInfo[1] + '!' + range[0]; // range for a single cell

            if (range[0] != range[1])
            {
                rangeString += ':' + range[1]; // range for multiple cells within the same row
            }

            SpreadsheetsResource.ValuesResource.GetRequest request =
            _sheetsService.Spreadsheets.Values.Get(_sheetInfo[0], rangeString);
            var response = request.Execute();
            var spreadsheetValues = response.Values;

            if (spreadsheetValues != null)
            {
                IList<object> list = spreadsheetValues[0];
                // This is ugly AF but sometimes the request doesn't return the full range,
                // if the range after the last value is just empty values
                // So this is to prevent wrong index access
                for (int i = 0; i < 20; i++)
                {
                    list.Add("");
                }

                // Check if lock column is specified
                if (parameters.Keys.Contains("lock"))
                {
                    // (int)parameters["lock"][0] - range[0][0]
                    // getting the right index within the specified range
                    // takes column letter from the beginning of the range as the base
                    string lockString = (string)list[(int)parameters["lock"][0] - range[0][0]];
                    bool locked = false;

                    if (lockString != null && lockString != "" && bool.TryParse(lockString, out bool _))
                    {
                        locked = bool.Parse(lockString);
                    }
                    
                    if (locked)
                    {
                        Console.WriteLine(data!.data!.level + " is locked, skipping...");
                        return;
                    }
                }

                // parameters["primarytime"] is realtime/realtimeloadless/ingametime
                oldValue = (string)list[(int)parameters[parameters["primarytime"]][0] - range[0][0]];
            }
            var lst = new List<object>();
            var newValues = new List<IList<object>> { lst };

            if (oldValue != "" && !CompareTimes(oldValue, GetPrimaryTime(parameters["primarytime"], data)))
            {
                Console.WriteLine(data!.data!.level + " is already up to date, skipping...");
                return;
            }


            var sorted = parameters.OrderBy(pair => pair.Value).Take(parameters.Count).ToDictionary(x => x.Key);

            foreach (var key in sorted.Keys)
            {
                if (key != "game" && key != "spreadsheet" &&
                    key != "verifiedonly" && key != "emulatorallowed" &&
                    key != "primarytime" && key != "subcategory")
                {
                    newValues[0].Add(AddValue(key, data));
                }
            }

            response.Values = newValues;
            var update = _sheetsService.Spreadsheets.Values.Update(response, _sheetInfo[0], rangeString);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            update.Execute();
            Console.WriteLine(data.data!.level + " has been updated.");
        }

        private string GetPrimaryTime(string time, LeaderboardData data)
        {
            switch(time)
            {
                case "realtime":
                    return data!.data!.runs![0].run!.times!.realtime!;

                case "realtimeloadless":
                    return data!.data!.runs![0].run!.times!.realtime_noloads!;
                case "ingametime":
                    return data!.data!.runs![0].run!.times!.ingame!;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Checks if time1 is faster than time2
        /// This is also written very lazily and could be decomposed but eeeeh
        /// </summary>
        /// <param name="time1"></param>
        /// <param name="time2"></param>
        /// <returns></returns>
        private bool CompareTimes(string time1, string time2)
        {
            var parts1 = time1.Split(':');
            var parts2 = time2.Split(':');
            decimal num1 = 0;
            decimal num2 = 0;

            if (parts1.Length != 1)
            {
                num1 = decimal.Parse(parts1[0],
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }

            if (parts2.Length != 1)
            {
                num2 = decimal.Parse(parts2[0],
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }

            if (num1 != num2)
            {
                return num1 > num2;
            }

            parts1 = parts1[parts1.Length - 1].Split(':');
            parts2 = parts2[parts2.Length - 1].Split(':');
            num1 = 0;
            num2 = 0;

            if (parts1.Length != 1)
            {
                num1 = decimal.Parse(parts1[0],
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }

            if (parts2.Length != 1)
            {
                num2 = decimal.Parse(parts2[0],
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }

            if (num1 != num2)
            {
                return num1 > num2;
            }

            num1 = decimal.Parse(parts1[parts1.Length - 1],
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            num2 = decimal.Parse(parts2[parts2.Length - 1],
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"));

            return num1 > num2;
            
        }

        /// <summary>
        /// Returns the corresponding value from the leaderboard.
        /// </summary>
        /// <param name="item">Name of the value we want.</param>
        /// <param name="data">Leaderboard data where the value is stored.</param>
        /// <returns></returns>
        private string AddValue(string item, LeaderboardData data)
        {
            switch(item)
            {
                case "comment":
                    return data.data!.runs![0].run!.comment ?? "";

                case "date":
                    return data.data!.runs![0].run!.date ?? "";

                case "emulator":
                    return data.data!.runs![0].run!.system!.emulated.ToString() ?? "";

                case "ingametime":
                    return data.data!.runs![0]!.run!.times!.ingame ?? "";

                case "level":
                    return data.data!.level ?? "";

                case "platform":
                    return data.data!.runs![0].run!.system!.platform ?? "";

                case "player":
                    return data.data!.runs![0].run!.players![0].uri ?? "";

                case "realtime":
                    return data.data!.runs![0].run!.times!.realtime ?? "";

                case "realtimeloadless":
                    return data.data!.runs![0].run!.times!.realtime_noloads ?? "";

                case "region":
                    return data.data!.runs![0].run!.system!.region ?? "";

                case "videolink":
                    return data.data!.runs![0].run!.videos!.links![0].uri ?? "";

                default:
                    return "";
            }
        }
    }
}
