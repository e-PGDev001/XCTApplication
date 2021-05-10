using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Xamarin.Forms;
using XCTApplication.Utils;

namespace XCTApplication.ViewModels
{
    public class XCTNewDiaryPageViewModel : XCTViewModelBase
    {
        public XCTNewDiaryPageViewModel()
        {
            CreateNewDiaryCommand = ReactiveCommand.CreateFromTask(CreateNewDiaryTask);
            MediaFiles = new List<MediaFile>();
            DateSelectedCommand = ReactiveCommand.CreateFromTask<DateChangedEventArgs>(DateSelectedTask);
        }

        #region Properties

        private string _comments;

        public string Comments
        {
            get => _comments;
            set => this.RaiseAndSetIfChanged(ref _comments, value);
        }

        private string _selectedEvent;

        public string SelectedEvent
        {
            get => _selectedEvent;
            set => this.RaiseAndSetIfChanged(ref _selectedEvent, value);
        }


        private string _taskCategory;

        public string TaskCategory
        {
            get => _taskCategory;
            set => this.RaiseAndSetIfChanged(ref _taskCategory, value);
        }


        private string _selectedArea;

        public string SelectedArea
        {
            get => _selectedArea;
            set => this.RaiseAndSetIfChanged(ref _selectedArea, value);
        }


        private string _selectedDate;

        public string SelectedDate
        {
            get => _selectedDate;
            set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
        }


        #endregion

        public List<MediaFile> MediaFiles { get; set; }

        #region CreateNewDiaryCommand

        public ICommand CreateNewDiaryCommand { get; }

        private async Task CreateNewDiaryTask()
        {
            var client = new HttpClient();
            var uri = new Uri("https://reqres.in/");

            var postData = new PostItemData
            {
                selected_area = SelectedArea,
                selected_catalog = TaskCategory,
                selected_date = SelectedDate,
                selected_event = SelectedEvent,
                selected_images = new List<string>()
            };

            foreach (var mediaFile in MediaFiles)
            {
                if (string.IsNullOrEmpty(mediaFile.Path)) return;
                var bytes = System.IO.File.ReadAllBytes(mediaFile.Path);

                postData.selected_images.Add(Convert.ToBase64String(bytes));
            }

            var json = JsonSerializer.Serialize(postData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync(uri, content);
        }

        #endregion


        #region DateSelectedCommand

        public ICommand DateSelectedCommand { get; }

        private async Task DateSelectedTask(DateChangedEventArgs args)
        {
            if(args == null) return;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var newDate = args.NewDate;
            SelectedDate = newDate.ToString("MM/dd/yyyy");
        }

        #endregion
    }

    public class PostItemData
    {
        public string selected_date { get; set; }

        public string selected_catalog { get; set; }

        public string selected_area { get; set; }

        public string selected_event { get; set; }

        public List<string> selected_images { get; set; }
    }
}