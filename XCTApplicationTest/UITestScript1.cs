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
    }
}