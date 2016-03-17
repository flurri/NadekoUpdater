using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApplication
{
    public class Program
    {
        public static DateTime LastUpdate {get;set;} = DateTime.MinValue;
        public static void Main(string[] args)
        {
            MainAsync().Wait();
            WriteLine("***PROGRAM ENDED***",ConsoleColor.Red);
            Console.ReadKey();
        }

        private static async Task MainAsync()
        {
            WriteLine("***NADEKOBOT UPDATER***",ConsoleColor.Green);
            
            while(true){
                WriteLine("PICK AN OPTION: (type 1-3)",ConsoleColor.Magenta);
                WriteLine("1. Check for newest stable release.",ConsoleColor.Magenta);
                WriteLine("2. Check for any newest release.",ConsoleColor.Magenta);
                WriteLine("3. Exit",ConsoleColor.Magenta);
                var input = Console.ReadLine().Trim();
                if(string.IsNullOrWhiteSpace(input))
                    continue;
                if(input == "3")
                    break;
                try{
                    if(input == "1"){
                        WriteLine("Getting data...");
                        var data = await GetReleaseData("https://api.github.com/repos/Kwoth/NadekoBot/releases/latest");
                        if(ConfirmReleaseUpdate(data))
                            Update(data);
                        continue;
                        
                    }
                    if(input == "2"){
                        WriteLine("Getting data...");
                        var data = await GetReleaseData("https://api.github.com/repos/Kwoth/NadekoBot/releases/latest");
                        if(ConfirmReleaseUpdate(data))
                            Update(data);
                        continue;
                    }
                } catch(Exception ex){
                    System.Console.WriteLine($"Unable to sync. {ex.Message}");
                }
            }
        }

        private static bool ConfirmReleaseUpdate(GithubReleaseModel data)
        {
            if(data.PublishedAt <= LastUpdate){
                WriteLine("You already have a newer version!", ConsoleColor.Red);
                return false;
            }
            WriteLine("Newer version found!\n\nAre you sure you want to update? (Y or N)", ConsoleColor.Magenta);
            return Console.ReadLine().ToLower() == "y";
        }

        private static void Update(GithubReleaseModel data)
        {
            WriteLine("........................................");
            throw new NotImplementedException();
        }

        public static async Task<GithubReleaseModel> GetReleaseData(string link){
            using(HttpClient client = new HttpClient()){
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept","application/vnd.github.v3+json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0)");
                
                    var response = await client.GetStringAsync(link);
                    var release = JsonConvert.DeserializeObject<GithubReleaseModel>(response);
                    Console.WriteLine($"{release.PublishedAt}\n{release.VersionName}\n{release.Assets[0].DownloadLink}");
                    return release;
            }
        }
        
        public static void Write(string text, ConsoleColor clr = ConsoleColor.White){
            var oldClr = Console.ForegroundColor;
            Console.ForegroundColor = clr;
            Console.Write(text);
            Console.ForegroundColor = oldClr;
        }
        
        public static void WriteLine(string text, ConsoleColor clr = ConsoleColor.White){
            Write(text+ Environment.NewLine,clr);
        }
    }
    public class GithubReleaseModel{
        
        [JsonPropertyAttribute("published_at")]
        public DateTime PublishedAt { get; set; }
        
        [JsonPropertyAttribute("tag_name")]
        public String VersionName {get;set;}
        
        [JsonPropertyAttribute("body")]
        public String PatchNotes {get;set;}
        
        public Asset[] Assets{ get; set;}
        
        public class Asset
        {
            [JsonPropertyAttribute("browser_download_url")]
            public string DownloadLink { get; set; }
        }
    }
    
}
