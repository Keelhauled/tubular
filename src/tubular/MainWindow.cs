using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terminal.Gui;

namespace Tubular
{
    public class MainWindow : Toplevel
    {
        private List<Video> AllVideos { get; set; }
        private List<Video> FilteredVideos { get; set; }
        private bool showingLatest;

        public MainWindow(IList<Video> videos)
        {
            AllVideos = videos.ToList();
            FilteredVideos = videos.ToList();
            CreateWindow();
        }

        private void CreateWindow()
        {
            var menuBar = new MenuBar(new[]{
                new MenuBarItem("File", new[]{
                    new MenuItem("Quit", "", () => Application.RequestStop())
                }),
            });

            var videoList = new ListView(FilteredVideos.Select(x => x.title).ToList()){ X = 0, Y = 2, Width = Dim.Fill(), Height = Dim.Fill() - 1 };
            videoList.OpenSelectedItem += x =>
            {
                var entry = FilteredVideos[x.Item].entry;
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

            var filterText = new TextField(){ X = 0, Y = 1, Width = Dim.Fill(), Height = 1, ColorScheme = Colors.Menu };
            filterText.TextChanged += x =>
            {
                var text = filterText.Text.ToString()?.ToLowerInvariant();
                Debug.Assert(text != null, nameof(text) + " != null");
                showingLatest = false;
                FilteredVideos = AllVideos.Where(y => y.title.ToLowerInvariant().Contains(text)).ToList();
                videoList.SetSource(FilteredVideos.Select(y => y.title).ToList());
            };

            var statusBar = new StatusBar(new []
            {
                new StatusItem(Key.CtrlMask | Key.q, "~Ctrl+Q~ Quit", () => {}),
                new StatusItem(Key.CtrlMask | Key.f, "~Ctrl+F~ Filter", () => filterText.SetFocus()),
                new StatusItem(Key.p, "~P~ Play", () =>
                {
                    var entry = FilteredVideos[videoList.SelectedItem].entry;
                    Utils.StartRedirectedProcess("mpv", entry.Link.Href);
                    MessageBox.Query("", $"Playing video:\n{entry.Title}", "OK");
                }),
                new StatusItem(Key.l, "~L~ Latest", () =>
                {
                    if(showingLatest)
                    {
                        FilteredVideos = AllVideos.ToList();
                        videoList.SetSource(FilteredVideos.Select(y => y.title).ToList());
                    }
                    else
                    {
                        FilteredVideos = AllVideos.GroupBy(x => x.entry.Author.Name).Select(x => x.First()).ToList();
                        videoList.SetSource(FilteredVideos.Select(y => y.title).ToList());
                    }

                    showingLatest = !showingLatest;
                })
            });
            
            Add(menuBar, filterText, videoList, statusBar);
        }
    }

    public record Video(string title, Entry entry);
}