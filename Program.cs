using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Threading;
using System.IO;

namespace ConsoleApplication
{
    public class Program
    {
        public static DateTime LastUpdate { get; set; }
        public static void Main(string[] args)
        {
            DateTime lastUpdate;
            if (!File.Exists("version.txt"))
                File.WriteAllText("version.txt", "");
            if (!DateTime.TryParse(File.ReadAllText("version.txt"), out lastUpdate))
                lastUpdate = DateTime.MinValue;
            LastUpdate = lastUpdate;
            System.Console.WriteLine("Current version release date: " + LastUpdate);

            MainAsync().Wait();
            WriteLine("***PROGRAM ENDED***", ConsoleColor.Red);
            Console.ReadKey();
        }

        private static async Task MainAsync()
        {
            WriteLine("***NADEKOBOT UPDATER***", ConsoleColor.Green);

            while (true)
            {
                WriteLine("PICK AN OPTION: (type 1-3)", ConsoleColor.Magenta);
                WriteLine("1. Check for newest stable release.", ConsoleColor.Magenta);
                WriteLine("2. Check for any newest release.", ConsoleColor.Magenta);
                WriteLine("3. Exit", ConsoleColor.Magenta);
                var input = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (input == "3")
                    break;
                try
                {
                    if (input == "1")
                    {
                        WriteLine("Getting data...");
                        var data = await GetReleaseData("https://api.github.com/repos/Kwoth/NadekoBot/releases/latest");
                        if (ConfirmReleaseUpdate(data))
                            await Update(data);
                        continue;
                    }
                    if (input == "2")
                    {
                        WriteLine("Getting data...");
                        var data = await GetReleaseData("https://api.github.com/repos/Kwoth/NadekoBot/releases/latest");
                        if (ConfirmReleaseUpdate(data))
                            await Update(data);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Unable to sync. {ex.Message}");
                }
            }
        }

        private static bool ConfirmReleaseUpdate(GithubReleaseModel data)
        {
            if (data.PublishedAt <= LastUpdate)
            {
                WriteLine("You already have an up-to-date version!", ConsoleColor.Red);
                return false;
            }
            WriteLine("Newer version found!\n\nAre you sure you want to update? (Y or N)", ConsoleColor.Magenta);
            return Console.ReadLine().ToLower() == "y";
        }

        private static async Task Update(GithubReleaseModel data)
        {
            WriteLine("........................................");
            try
            {
                using (var httpClient = new HttpClient())
                using (var cancelSource = new CancellationTokenSource())
                {
                    var cancelToken = cancelSource.Token;
                    var waitTask = Task.Run(async () => await Waiter(cancelToken));
                    Console.WriteLine("Downloading. Be patient. There is no need to open an issue if it takes long.");
                    var stream = await httpClient.GetStreamAsync(data.Assets[0].DownloadLink);
                    var arch = new ZipArchive(stream);
                    cancelSource.Cancel();
                    await waitTask;
                    WriteLine("Backing up old version...", ConsoleColor.DarkYellow);
                    if (Directory.Exists("NadekoBot"))
                    {
                        if (Directory.Exists("NadekobBot_old"))
                        {
                            Directory.Delete("NadekoBot_old", true);
                        }
                        DirectoryCopy(@".\NadekoBot", @".\NadekoBot_old", true);
                    }
                    WriteLine("Saving...", ConsoleColor.Green);
                    arch.ExtractToDirectory(@".\NadekoBot");
                    File.WriteAllText("version.txt", data.PublishedAt.ToString());
                    WriteLine("Done!");
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }

        public static async Task<GithubReleaseModel> GetReleaseData(string link)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0)");

                var response = await client.GetStringAsync(link);
                var release = JsonConvert.DeserializeObject<GithubReleaseModel>(response);
                Console.WriteLine($"\tReleased At: {release.PublishedAt}\n\tVersion: {release.VersionName}\n\tLink: {release.Assets[0].DownloadLink}");
                return release;
            }
        }

        public static void Write(string text, ConsoleColor clr = ConsoleColor.White)
        {
            var oldClr = Console.ForegroundColor;
            Console.ForegroundColor = clr;
            Console.Write(text);
            Console.ForegroundColor = oldClr;
        }

        public static void WriteLine(string text, ConsoleColor clr = ConsoleColor.White)
        {
            Write(text + Environment.NewLine, clr);
        }

        public static async Task Waiter(CancellationToken cancel)
        {
            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    Write("|");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    await Task.Delay(333, cancel);
                    Write("/");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    await Task.Delay(333, cancel);
                    Write("--");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    await Task.Delay(333, cancel);
                    Write("\\");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    await Task.Delay(333, cancel);
                    Write("--");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    await Task.Delay(333, cancel);
                    Write("\\");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    await Task.Delay(333, cancel);
                }
            }
            catch (OperationCanceledException)
            {
                WriteLine("Download complete.");
                return; // 👌
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
    public class GithubReleaseModel
    {

        [JsonPropertyAttribute("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonPropertyAttribute("tag_name")]
        public String VersionName { get; set; }

        [JsonPropertyAttribute("body")]
        public String PatchNotes { get; set; }

        public Asset[] Assets { get; set; }

        public class Asset
        {
            [JsonPropertyAttribute("browser_download_url")]
            public string DownloadLink { get; set; }
        }
    }

}
