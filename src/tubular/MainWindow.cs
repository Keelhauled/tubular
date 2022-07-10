using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Tubular
{
    public class MainWindow : Toplevel
    {
        private readonly List<Video> allVideos;
        private List<Video> filteredVideos;
        private bool showingLatest;
        private readonly Dictionary<string, Color> authorColors = new();

        public MainWindow(List<Video> videos)
        {
            allVideos = videos.ToList();
            filteredVideos = videos.ToList();
            CreateWindow();
        }

        private void CreateWindow()
        {
            var filterText = new TextField(){ Width = Dim.Fill(), Height = 1, ColorScheme = Colors.Dialog };
            var videoList = new ListView(filteredVideos.Select(x => x.title).ToList()){ Y = Pos.Bottom(filterText), Width = Dim.Fill(), Height = Dim.Fill() };

            videoList.RowRender += args =>
            {
                if(args.Row == videoList.SelectedItem)
                    return;
                
                var author = filteredVideos[args.Row].entry.Author.Name;
                if(!authorColors.TryGetValue(author, out var color))
                {
                    var blacklist = new[]{Color.Black, Color.DarkGray, Color.Magenta, Color.Brown};
                    var colors = Enum.GetValues<Color>().Except(blacklist).ToArray();
                    color = colors[new Random().Next(colors.Length)];
                    authorColors.Add(author, color);
                }

                args.RowAttribute = Attribute.Make(color, Color.Black);
            };

            filterText.TextChanged += x =>
            {
                var text = filterText.Text.ToString()?.ToLowerInvariant();
                Debug.Assert(text != null, nameof(text) + " != null");
                showingLatest = false;
                filteredVideos = allVideos.Where(y => y.title.ToLowerInvariant().Contains(text)).ToList();
                videoList.SetSource(filteredVideos.Select(y => y.title).ToList());
            };

            var statusBar = new StatusBar(new []
            {
                new StatusItem(Key.CtrlMask | Key.Q, "~Ctrl+Q~ Quit", () => Application.RequestStop()),
                new StatusItem(Key.CtrlMask | Key.F, "~Ctrl+F~ Filter", () => filterText.SetFocus()),
                new StatusItem(Key.Enter, "~Enter~ Play", () =>
                {
                    var entry = filteredVideos[videoList.SelectedItem].entry;
                    Utils.StartRedirectedProcess("mpv", entry.Link.Href);
                    MessageBox.Query("", $"Playing video:\n{entry.Title}", "OK");
                }),
                new StatusItem(Key.l, "~L~ Latest", () =>
                {
                    filteredVideos = (showingLatest ? allVideos : allVideos.GroupBy(x => x.entry.Author.Name).Select(x => x.First())).ToList();
                    videoList.SetSource(filteredVideos.Select(y => y.title).ToList());
                    showingLatest = !showingLatest;
                }),
                new StatusItem(Key.i, "~I~ Info", () =>
                {
                    var entry = filteredVideos[videoList.SelectedItem].entry;
                    var val = MessageBox.Query("", $"{entry.Author.Name}\n{entry.Title}\n{entry.Published}", "Link", "Channel", "Cancel");
                    switch(val)
                    {
                        case 0:
                            Process.Start(new ProcessStartInfo(entry.Link.Href){ UseShellExecute = true });
                            break;
                    
                        case 1:
                            Process.Start(new ProcessStartInfo(entry.Author.Uri){ UseShellExecute = true });
                            break;
                    }
                })
            });

            Add(filterText, videoList, statusBar);
        }
    }

    public record Video(string title, Entry entry);
}
