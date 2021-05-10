using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using XCTApplication.Utils;

namespace XCTApplication.ViewModels
{
    public class XCTNewDiaryPageViewModel : XCTViewModelBase
    {
        public XCTNewDiaryPageViewModel()
        {
            CreateNewDiaryCommand = ReactiveCommand.CreateFromTask(CreateNewDiaryTask);
            MediaFiles = new List<MediaFile>();
        }

        #region Properties

        private string _comments;

        public string Comments
        {
            get => _comments;
            set => this.RaiseAndSetIfChanged(ref _comments, value);
        }


        #endregion

        public List<MediaFile> MediaFiles { get; set; }

        #region CreateNewDiaryCommand

        public ICommand CreateNewDiaryCommand { get; }

        private async Task CreateNewDiaryTask()
        {
        }

        #endregion
    }
}