using NUnit.Framework;
using QuickTest;
using XCTApplication;

namespace XCTApplicationTest
{

    public class UITestScript1 : QuickTest<App>
    {
        [SetUp]

        protected override void SetUp()
        {
            base.SetUp();

            Launch(new App());
        }

        [Test]
        public void LoginDiaryApp()
        {
            Input("EntryUserAId", "test user");
            Input("EntryPasswordAId", "mysecret");
            Tap("ButtonLoginAId");
            ShouldSee("Syncfusion License");
        }

        [Test]
        public void TestComboboxSource()
        {
            Input("EntryUserAId", "test user");
            Input("EntryPasswordAId", "mysecret");
            Tap("ButtonLoginAId");

            After(seconds: 7);

            // go back at the first time because syncfusion license dialog, we need press back button to close it
            GoBack();

            // 
            Tap("ButtonAddPhotoAId");
            ShouldSee("Pick image from camera");
        }
    }
}