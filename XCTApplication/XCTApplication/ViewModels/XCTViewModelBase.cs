using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Xamarin.Forms;

namespace XCTApplication.ViewModels
{
    public class XCTViewModelBase : ReactiveObject
    {
        public XCTViewModelBase()
        {
        }


        #region Abtraction

        public virtual void OnNavigationBack(Dictionary<string, object> parameters)
        {
        }

        #endregion

        #region Properties

        private INavigation _navigationService;
        public INavigation NavigationService => _navigationService ?? (_navigationService = GetNavigationService());

        private static INavigation GetNavigationService()
        {
            var page = Application.Current.MainPage;
            var navigation = page.Navigation;
            var curPage = navigation.GetCurrentContentPage();
            var curNavSer = curPage.Navigation;

            return curNavSer;
        }

        #endregion


        #region PopModalCommand

        private ICommand _popModalCommand;

        public ICommand PopModalCommand =>
            _popModalCommand ?? (_popModalCommand = ReactiveCommand.CreateFromTask(PopModalTask));

        private async Task PopModalTask()
        {
            await this.NavigationService.PopModalAsync(animated: true);
        }

        #endregion
    }

    public static class PageExtensions
    {
        public static Page GetCurrentContentPage(this INavigation navigation)
        {
            var lastModalPage = navigation.ModalStack.LastOrDefault();
            if (lastModalPage == null)
            {
                var lastPage = navigation.NavigationStack.LastOrDefault();
                switch (lastPage)
                {
                    case NavigationPage page:
                        {
                            return page.CurrentPage;
                        }

                    case MasterDetailPage page:
                        {
                            var curDetailPage = page.Detail;
                            if (curDetailPage is NavigationPage navPage) return navPage.CurrentPage;
                            return curDetailPage;
                        }

                    case TabbedPage page:
                        {
                            var curTabPage = page.CurrentPage;
                            if (curTabPage is NavigationPage navPage) return navPage.CurrentPage;
                            return curTabPage;
                        }

                    default:
                        {
                            return lastPage;
                        }
                }
            }
            else
            {
                switch (lastModalPage)
                {
                    case NavigationPage page:
                        {
                            return page.CurrentPage;
                        }

                    case MasterDetailPage page:
                        {
                            var curDetailPage = page.Detail;
                            if (curDetailPage is NavigationPage navPage) return navPage.CurrentPage;
                            return curDetailPage;
                        }

                    case TabbedPage page:
                        {
                            var curTabPage = page.CurrentPage;
                            if (curTabPage is NavigationPage navPage) return navPage.CurrentPage;
                            return curTabPage;
                        }

                    default:
                        {
                            return lastModalPage;
                        }
                }
            }
        }
    }
}