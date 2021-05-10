using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XCTApplication.Views;

namespace XCTApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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