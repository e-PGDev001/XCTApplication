using System.Reactive.Disposables;
using ReactiveUI;
using System;

namespace XCTApplication.Views
{
    public partial class XCTMainPageView
    {
        private readonly CompositeDisposable _compositeDisposable;

        public XCTMainPageView()
        {
            InitializeComponent();
            _compositeDisposable = new CompositeDisposable();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.WhenAnyValue(view => view.EntryPassword.Text,
                    view => view.EntryUser.Text)
                .Subscribe(values =>
                {
                    this.ButtonLogin.IsEnabled =
                        !string.IsNullOrEmpty(values.Item1) && !string.IsNullOrEmpty(values.Item2);
                })
                .DisposeWith(_compositeDisposable);
        }

        protected override void OnDisappearing()
        {
            _compositeDisposable.Dispose();
            base.OnDisappearing();
        }
    }
}