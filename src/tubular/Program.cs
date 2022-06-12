using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Terminal.Gui;
using System.CommandLine;

namespace Tubular
{
    public static class Program
    {
        private const string PROGRAM_NAME = "tubular";
        private const string BASE_URL = "https://www.youtube.com/feeds/videos.xml?";
        
        private static readonly string configEnv = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string? XDG_CONFIG_HOME = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        private static readonly string homeEnv = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static readonly string? XDG_CACHE_HOME = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
        
        private static readonly DirectoryInfo cache = Directory.CreateDirectory(Path.Combine(XDG_CACHE_HOME ?? $"{homeEnv}/.cache", PROGRAM_NAME));
        private static readonly DirectoryInfo config = Directory.CreateDirectory(Path.Combine(XDG_CONFIG_HOME ?? configEnv, PROGRAM_NAME));
        private static readonly string feedsPath = Path.Combine(config.FullName, "feeds");

        private static readonly HttpClient httpClient = new(new HttpClientHandler{ MaxConnectionsPerServer = 2 });
        private static readonly XmlSerializer xmlSerializer = new(typeof(Feed));
        
        private static async Task Main(string[] args)
        {
            var noUpdate = new Option<bool>(new[]{"--no-update", "-u"}, "Don't update feeds, just read from cache");
            var filter = new Option<string?>(new[]{"--filter", "-f"}, "Filter channels by their name");
            var maxTime = new Option<int>(new[]{"--maxtime", "-t"}, () => 365, "How many days of videos to show");
            
            var runConfig = new Command("config", "Open config file in text editor");
            runConfig.SetHandler(() => Process.Start(new ProcessStartInfo(feedsPath){ UseShellExecute = true }));

            var rootCommand = new RootCommand("List videos from youtube rss feeds in chronological order"){ noUpdate, runConfig, filter, maxTime };
            rootCommand.SetHandler<bool, string, int>(Run, noUpdate, filter, maxTime);
            await rootCommand.InvokeAsync(args);
        }

        private static async Task Run(bool noUpdate, string? filter, int maxTime)
        {
            var feeds = await GetFeeds(noUpdate, filter);
            
            int padding = 0;
            var videos = feeds.SelectMany(x =>
            {
                if(x.Author.Name.Length > padding)
                    padding = x.Author.Name.Length;
                return x.Entry;
            }).Where(x => x.Published > DateTime.Now - TimeSpan.FromDays(maxTime)).OrderByDescending(x => x.Published).ToList();
            
            var titles = videos.Select(x => $"[{x.Published:yy/MM/dd}] {x.Author.Name.PadRight(padding)} {x.Title}").ToList();

            try
            {
                Application.Init();
                var top = Application.Top;

                var list = new ListView(titles)
                {
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };
                top.Add(list);
                
                list.OpenSelectedItem += x =>
                {
                    var entry = videos[x.Item];
                    var val = MessageBox.Query("", $"{entry.Author.Name}\n{entry.Title}\n{entry.Published}", "Play", "Link", "Cancel");
                    switch(val)
                    {
                        case 0:
                            MessageBox.Query("", $"Playing video:\n{entry.Title}", "OK");
                            Utils.StartRedirectedProcess("/bin/mpv", entry.Link.Href);
                            break;
                        
                        case 1:
                            Process.Start(new ProcessStartInfo(entry.Link.Href){ UseShellExecute = true });
                            break;
                        
                        case 2:
                            Process.Start(new ProcessStartInfo(entry.Author.Uri){ UseShellExecute = true });
                            break;
                    }
                };

                Application.Run();
            }
            finally
            {
                Application.Shutdown();
            }
        }

        private static async Task<IEnumerable<Feed>> GetFeeds(bool noUpdate, string? filter)
        {
            filter ??= "";

            if(!File.Exists(feedsPath))
                throw new FileNotFoundException("Run \"tubular config\" to set up the feeds file", feedsPath);
            
            var feedLines = await File.ReadAllLinesAsync(feedsPath);
            var feeds = new ConcurrentBag<Feed>();
            var tasks = feedLines.Select(async line =>
            {
                var split = line.Split(',');
                var id = split[0].Trim();
                var url = $"{BASE_URL}{split[1].Trim()}";
                var cachePath = Path.Combine(cache.FullName, id);

                if(!id.ToLower().Contains(filter.ToLower()))
                    return;

                string? xml = null;
                if(noUpdate && File.Exists(cachePath))
                {
                    xml = await File.ReadAllTextAsync(cachePath);
                }
                else
                {
                    try
                    {
                        xml = await httpClient.GetStringAsync(url);
                        await File.WriteAllTextAsync(cachePath, xml);
                    }
                    catch(HttpRequestException e)
                    {
                        Console.WriteLine($"Error loading url {url}\n{e}");
                    }
                }

                if(xml == null)
                    return;
                
                using var textReader = new StringReader(xml);
                var feed = (Feed?)xmlSerializer.Deserialize(textReader);
                
                if(feed != null)
                    feeds.Add(feed);
            });
            await Task.WhenAll(tasks);

            return feeds;
        }
    }
}
