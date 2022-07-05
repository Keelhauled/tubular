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
            
            var runConfig = new System.CommandLine.Command("config", "Open config file in text editor");
            runConfig.SetHandler(() => Process.Start(new ProcessStartInfo(feedsPath){ UseShellExecute = true }));

            var rootCommand = new RootCommand("List videos from youtube rss feeds in chronological order"){ noUpdate, runConfig, filter, maxTime };
            rootCommand.SetHandler<bool, string, int>(Run, noUpdate, filter, maxTime);
            await rootCommand.InvokeAsync(args);
        }

        private record Video(string title, Entry entry);

        private static async Task Run(bool noUpdate, string? filter, int maxTime)
        {
            var feeds = await GetFeeds(noUpdate, filter);
            
            int padding = 0;
            var videos = feeds.SelectMany(x =>
            {
                if(x.Author.Name.Length > padding)
                    padding = x.Author.Name.Length;
                return x.Entry;
            }).Where(x => x.Published > DateTime.Now - TimeSpan.FromDays(maxTime)).OrderByDescending(x => x.Published)
              .Select(x => new Video($"[{x.Published:yy/MM/dd}] {x.Author.Name.PadRight(padding)} {x.Title}", x)).ToList();
            var filtered = new List<Video>(videos);

            try
            {
                Application.Init();
                
                var menuBar = new MenuBar(new[]{
                    new MenuBarItem("File", new[]{
                        new MenuItem("Quit", "", () => Application.RequestStop())
                    }),
                });

                var videoList = new ListView(filtered.Select(x => x.title).ToList()){ X = 0, Y = 2, Width = Dim.Fill(), Height = Dim.Fill() - 1 };
                videoList.OpenSelectedItem += x =>
                {
                    var entry = filtered[x.Item].entry;
                    var val = MessageBox.Query("", $"{entry.Author.Name}\n{entry.Title}\n{entry.Published}", "Play", "Link", "Channel", "Cancel");
                    switch(val)
                    {
                        case 0:
                            Utils.StartRedirectedProcess("mpv", entry.Link.Href);
                            MessageBox.Query("", $"Playing video:\n{entry.Title}", "OK");
                            break;
                        
                        case 1:
                            Process.Start(new ProcessStartInfo(entry.Link.Href){ UseShellExecute = true });
                            break;
                        
                        case 2:
                            Process.Start(new ProcessStartInfo(entry.Author.Uri){ UseShellExecute = true });
                            break;
                    }
                };

                var filterText = new TextField(){ X = 0, Y = 1, Width = Dim.Fill(), Height = 1 };
                filterText.TextChanged += x =>
                {
                    var text = filterText.Text.ToString()?.ToLowerInvariant();
                    Debug.Assert(text != null, nameof(text) + " != null");
                    filtered = videos.Where(y => y.title.ToLowerInvariant().Contains(text)).ToList();
                    videoList.SetSource(filtered.Select(y => y.title).ToList());
                };

                var statusBar = new StatusBar(new []
                {
                    new StatusItem(Key.CtrlMask | Key.q, "~Ctrl+Q~ Quit", () => {}),
                    new StatusItem(Key.f, "~F~ Filter", () => filterText.SetFocus())
                });
                
                Application.Top.Add(menuBar, filterText, videoList, statusBar);
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
