using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NStack;
using Terminal.Gui;

namespace Tubular
{
    public static class Program
    {
        private const string PROGRAM_NAME = "tubular";
        
        private static readonly string configEnv = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string? XDG_CONFIG_HOME = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        private static readonly string homeEnv = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static readonly string? XDG_CACHE_HOME = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
        
        private static readonly DirectoryInfo cache = Directory.CreateDirectory(Path.Combine(XDG_CACHE_HOME ?? $"{homeEnv}/.cache", PROGRAM_NAME));
        private static readonly DirectoryInfo config = Directory.CreateDirectory(Path.Combine(XDG_CONFIG_HOME ?? configEnv, PROGRAM_NAME));
        private static readonly string feedsPath = Path.Combine(config.FullName, "feeds");

        private static readonly HttpClient httpClient = new(new HttpClientHandler{ MaxConnectionsPerServer = 2 });
        private static readonly XmlSerializer xmlSerializer = new(typeof(Feed));

        private static readonly TimeSpan maxTimeToShow = TimeSpan.FromDays(365);
        
        private static async Task Main(string[] args)
        {
            bool noUpdate = false, runConfig = false;
            if(args.Length > 0)
            {
                noUpdate = args.Contains("--noupdate");
                runConfig = args[0] == "config";
            }

            if(runConfig)
            {
                Process.Start(new ProcessStartInfo(feedsPath){ UseShellExecute = true });
                return;
            }
            
            var feeds = await GetFeeds(noUpdate);
            
            int padding = 0;
            var videos = feeds.SelectMany(x =>
            {
                if(x.Author.Name.Length > padding)
                    padding = x.Author.Name.Length;
                
                return x.Entry;
            }).Where(x => x.Published > DateTime.Now - maxTimeToShow).OrderByDescending(x => x.Published).ToList();
            
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
                    var entry = videos[list.SelectedItem];
                    Utils.StartRedirectedProcess("/bin/mpv", entry.Link.Href);
                    MessageBox.Query("Info", $"Starting video:\n{entry.Title}", ustring.Make("OK"));
                };

                Application.Run();
            }
            finally
            {
                Application.Shutdown();
            }
        }

        private static async Task<IEnumerable<Feed>> GetFeeds(bool noUpdate)
        {
            if(!File.Exists(feedsPath))
                throw new FileNotFoundException(feedsPath);
            
            var feedLines = await File.ReadAllLinesAsync(feedsPath);
            var feeds = new ConcurrentBag<Feed>();
            var tasks = feedLines.Select(async line =>
            {
                var split = line.Split(',');
                var id = split[0].Trim();
                var url = split[1].Trim();
                var cachePath = Path.Combine(cache.FullName, id);

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
