 - [About](#About)
 - [Setup](#Setup)
 - [Using the Updater](#Using)
 - [Configuration file](#Configuration)
	 - [[CONFIG]](#Config)
	 - [[LEADERBOARDS]](#Leaderboards)

# About<a name="About"></a>
This app serves for periodic fetching of the fastest runs in the given speedrun.com leaderboards, formatting the information and uploading it to a Google Spreadsheet. This is primarily made for IL leaderboards, where the user can make a Spreadsheet for the fastest times from each IL leaderboard and keep the app running to update the Spreadsheet as new and faster runs are added to the speedrun.com leaderboards.

The idea is that the Spreadsheet will consist of rows [A-Z], where each row corresponds with one single run.

# Setup<a name="Setup"></a>

 - Download .NET - https://dotnet.microsoft.com/en-us/download
 - Set up a OAuth 2.0 Client ID
 
 
 ## Setting up OAuth 2.0
 The process should be fairly straightforward. Go to https://developers.google.com/workspace/guides/createproject and follow the instructions. When choosing which APIs to enable, the only one you need to pick is the **Google Sheets API**. Follow the steps in the instructions until you get to the step **Create access credentials**.

You only need to create an OAuth client ID, so click on that option, which will bring you to the corresponding section. When choosing the application type, pick **Desktop application** and follow the instructions. Once you're done, go to https://console.cloud.google.com/apis/credentials, where you should see your newly created credentials. Download the JSON file by clicking on the *Download* icon on the right and save the file somewhere where you won't accidentally delete it.

Copy the path to the JSON secret file (feel free to rename the file beforehand) and save it to a new Environment variable. Type *"Edit the system environment variables"* into the Windows search bar and click on the result. In the bottom right corner, there will be a *Environment Variables…" button. Click on it and create a new System variable. The name of the variable will be `SpeedrunSpreadsheetUpdaterSecret` and the value will be the path to your *secret* file.

**Because of this, you might have to run the Updater as administrator.**


# Using the Updater<a name="Using"></a>
Once you have created the Environment variable with your JSON file, you will also need to create a *configuration* file. You can use the one from the repository as a base and modify it to your liking. The structure and option will also be described bellow.

You have a few options how to run the app. You can:

 -  simply run the *.exe*, in which case, the app will look for `config.txt` in the same folder and will run the update cycle every 60 minutes
 - drag your configuration file on the *.exe*, so the app uses it (update time is still 60 minutes)
 - start the app from the command line, where the first argument will be the path to your configuration file and the second argument will be the time between each update cycle in minutes - `…/SSU.exe <path to your config file> <time in minutes>`
 
 # Configuration file<a name="Configuration"></a>
 
 The file has two sections - **[CONFIG]** and **[LEADERBOARDS]**. These have to be in the same order in the file, because someone was too lazy to rewrite the parsing to make it more robust. 

You can add empty lines and comments into the config to make it more readable -
`#your comment here`
 # [CONFIG]<a name="Config"></a>
 There are 3 mandatory parameters:
 
 - `game=name_of_the_game`
	 - https://www.speedrun.com/**r3**/level/IL_-_The_Fairy_Council_1?h=Any-1.0&x=l_o9xl5839-xk90g5yk-e8mq2zen.klr20eo1
 - `primarytime=realtime/realtimeloadless/ingametime`
	 - Specifies which time is the primary one. In case there are multiple times filled in, the app has to know which time to compare against which
 - `spreadsheet=spreadsheet_id|sheet_name`
	 - https://docs.google.com/spreadsheets/d/**Spreadsheet_ID**/edit#gid=0
	 - Sheet name is simply the name of the current sheet (for example Sheet1)

#
In cases where a category has multiple subcategories, such as: <a name="speedrunlink"></a>https://www.speedrun.com/r3/level/IL_-_The_Fairy_Council_1?h=Any-1.0&x=l_o9xl5839-xk90g5yk-e8mq2zen.klr20eo1 (1.0x and PC 1.2x), there is also the `subcategory` parameter. This consists of the last two strings separated by the dot, in this case, **e8mq2zen.klr20eo1** for 1.0x subcategory leaderboard. If you leave this parameter out, the fetched leaderboard data will consist of all subcategories put together.

`subcategory=e8mq2zen.klr20eo1`
#
There are also two filter parameters:

 - `emulatorallowed=true/false`
	 - decides whether to take emulator runs into consideration or not
 - `verifiedonly=true/false`
	 - decides whether to take unverified runs into consideration or not

#
Lastly, there are the Cell parameters. The cell marks the beginning position of each run attribute. For example, if I had a column C designated for the runner's name, I would have a header with the name of the column and maybe one empty cell before the run list. This would mean that the starting cell would be C3 - `player=C3`

 - `comment=A1`
	 - returns comment attached to each run
 - `date=A1`
	 - returns the date when the run was submitted
 - `emulator=A1`
	 - returns TRUE/FALSE based on whether the run was done on an emulator
 - `ingametime=A1`
	 - returns the in-game time
 - `level=A1`
	 - returns the name of the leaderboard
 - `lock=A1`
	 - checks whether the row in the Spreadsheet is locked
	 - values in this column have a form of TRUE/FALSE and are added by the user
 - `platform=A1`
	 - returns the name of the platform the run was done on (GameCube, PS1…)
 - `player=A1`
	 - returns the URL to the runner's speedrun.com account
 - `realtime=A1`
	 - returns the RTA time
 - `realtimeloadless=A1`
	 - returns the load-less RTA time
 - `region=A1`
	 - returns the region the platform belongs to (USA/NTSC, …)
 - `videolink=A1`
	 - returns the link of the run that is attached in the leaderboard submission

# [LEADERBOARDS]<a name="Leaderboards"></a>
Here are all the leaderboards we want the app to fetch and update in the Spreadsheet. If we take the previous [link](#speedrunlink), then you will notice the two strings before the subcategory - **o9xl5839-xk90g5yk**. This designates the leaderboard.

For example, this is how you would input the first 5 levels in the first world of Rayman 3:
`#Fairy Council`
`o9xl5839-xk90g5yk`
`4958o3m9-xk90g5yk`
`rdq12po9-xk90g5yk`
`5d73knq9-xk90g5yk`
`kwjelx09-xk90g5yk`