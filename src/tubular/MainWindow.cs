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
        private bool openVideosFullscreen;

        public MainWindow(List<Video> videos)
        {
            allVideos = videos.ToList();
            filteredVideos = videos.ToList();
            CreateWindow();
        }

        private void CreateWindow()
        {
            var menuBar = new MenuBar(new[]
            {
                new MenuBarItem("File", new[]
                {
                    new MenuItem("Quit", "", () => Application.RequestStop())
                }),
                new MenuBarItem("Settings", new []
                {
                    new MenuItem("Open videos fullscreen", "", () => openVideosFullscreen = !openVideosFullscreen)
                })
            });
            
            var filterText = new TextField(){ Y = Pos.Bottom(menuBar), Width = Dim.Fill(), Height = 1, ColorScheme = Colors.Dialog };
            var videoList = new ListView(filteredVideos.Select(x => x.title).ToList()){ Y = Pos.Bottom(filterText), Width = Dim.Fill(), Height = Dim.Fill(1) };

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
                new StatusItem(Key.Unknown, "~Ctrl+Q~ Quit", () => {}),
                new StatusItem(Key.Unknown, "~F9~ Menu", () => {}),
                new StatusItem(Key.CtrlMask | Key.F, "~Ctrl+F~ Filter", () => filterText.SetFocus()),
                new StatusItem(Key.Enter, "~Enter~ Play", () =>
                {
                    var entry = filteredVideos[videoList.SelectedItem].entry;
                    Utils.StartRedirectedProcess("mpv", $"{(openVideosFullscreen ? "-fs " : "")}{entry.Link.Href}");
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
                            Utils.ShellExecute(entry.Link.Href);
                            break;
                        case 1:
                            Utils.ShellExecute(entry.Author.Uri);
                            break;
                    }
                })
            });

            Add(menuBar, filterText, videoList, statusBar);
        }
    }

    public record Video(string title, Entry entry);
}
