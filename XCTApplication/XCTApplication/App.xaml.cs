using Xamarin.Forms;
using XCTApplication.Views;

namespace XCTApplication
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            Sharpnado.MaterialFrame.Initializer.Initialize(loggerEnable: false, debugLogEnable: false);
            // we can claim license for free if our application have revenue <= 1M USD/year
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                "NDQ1NDU5QDMxMzkyZTMxMmUzMEZadHNnWTJyTkxlVkVrMGZadVpFczBkL1FBK0hwODNlUnlicVltVUIveDg9");
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