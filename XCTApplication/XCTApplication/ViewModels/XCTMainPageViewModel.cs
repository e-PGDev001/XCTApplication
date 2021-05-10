using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Xamarin.Forms;
using XCTApplication.Views;

namespace XCTApplication.ViewModels
{
    public class XCTMainPageViewModel : XCTViewModelBase
    {
        public XCTMainPageViewModel()
        {
            CreateNewDiaryCommand = ReactiveCommand.CreateFromTask(CreateNewDiaryTask);
        }

        #region CreateNewDiaryCommand

        public ICommand CreateNewDiaryCommand { get; }

        private async Task CreateNewDiaryTask()
        {
            await this.NavigationService.PushModalAsync(new NavigationPage(new XCTNewDiaryPageView()), animated: true);
        }

        #endregion
    }
}