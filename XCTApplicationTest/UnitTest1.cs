using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using XCTApplication.ViewModels;

namespace XCTApplicationTest
{
    public class Tests
    {
        private Fixture _fixture;
        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void TestNullMediaFiles()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var viewModel = mock.Create<XCTNewDiaryPageViewModel>();
                viewModel.MediaFiles.Should().BeEmpty();
            }
        }

        [Test]
        public void TestOtherFields()
        {
            using var mock = AutoMock.GetLoose();
            var viewModel = mock.Create<XCTNewDiaryPageViewModel>();

            viewModel.Comments.Should().BeNullOrEmpty();

            viewModel.SelectedArea.Should().BeNullOrEmpty();
            viewModel.SelectedEvent.Should().BeNullOrEmpty();
            viewModel.TaskCategory.Should().BeNullOrEmpty();
        }
    }
}