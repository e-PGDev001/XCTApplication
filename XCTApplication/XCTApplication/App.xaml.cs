using Xamarin.Forms;
using XCTApplication.Views;

namespace XCTApplication
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new XCTMainPageView {Title = "Home Page"});
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}