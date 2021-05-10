using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using ReactiveUI;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using XCTApplication.Controls;
using XCTApplication.Utils;

namespace XCTApplication.Views
{
    public partial class XCTNewDiaryPageView
    {
        private readonly CompositeDisposable _compositeDisposable;

        private int _imageCount = 1;

        public XCTNewDiaryPageView()
        {
            InitializeComponent();
            _compositeDisposable = new CompositeDisposable();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetupObserver();
        }

        private void SetupObserver()
        {
            Observable.FromEventPattern(h => ButtonAddPhoto.Clicked += h,
                    h => ButtonAddPhoto.Clicked -= h)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async args =>
                {
                    if (this.ViewModel == null) return;
                    var cameraPer = await PermissionsHelper.NewCheckPermission(Permission.Camera);
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    var photoPer = await PermissionsHelper.NewCheckPermission(Permission.Photos);
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    var storagePer = await PermissionsHelper.NewCheckPermission(Permission.Storage);
                    await Task.Delay(TimeSpan.FromMilliseconds(200));

                    if (cameraPer && photoPer && storagePer)
                    {
                        var cropper = new MediaChooser
                        {
                            PhotoLibraryTitle = "Pick image from gallery",
                            TakePhotoTitle = "Pick image from camera",
                            SelectSourceTitle = "Options",
                            Success = async imageFiles =>
                            {
                                await Task.Delay(250);
                                foreach (var imageFile in imageFiles)
                                {
                                    var image = new PostImageDetailsViewCell
                                    {
                                        Index = _imageCount - 1,
                                        HeightRequest = 80,
                                        WidthRequest = 80
                                    };
                                    image.SetImage(imageFile.PreviewPath);
                                    image.GestureRecognizers.Add(new TapGestureRecognizer
                                    {
                                        Command = ReactiveCommand.CreateFromTask(async () =>
                                        {
                                            await Task.Delay(TimeSpan.FromMilliseconds(100));
                                            // show image context
                                            var page = new ImageSelectionPageView(imageFile.PreviewPath,
                                                removeImageCommand: ReactiveCommand.CreateFromTask<int>(async index =>
                                                {
                                                    await this.Navigation.PopPopupAsync();
                                                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                                                    this.LayoutImages.Children.RemoveAt(index: index);
                                                    this._imageCount -= 1;
                                                    this.ViewModel.MediaFiles.Remove(imageFile);
                                                }),
                                                setAsLandingImageCommand: ReactiveCommand.CreateFromTask<int>(
                                                    async index =>
                                                    {
                                                        await this.Navigation.PopPopupAsync();
                                                        await Task.Delay(TimeSpan.FromMilliseconds(100));
                                                        image.SetAsLandingImage(landing: true);
                                                        this.ViewModel.MediaFiles[index].IsLanding =
                                                            true;

                                                        var childs = this.LayoutImages.Children;
                                                        foreach (var view in childs)
                                                        {
                                                            if (!(view is PostImageDetailsViewCell)) continue;
                                                            var realView = view.As<PostImageDetailsViewCell>();
                                                            if (realView.Index == index) return;
                                                            realView.SetAsLandingImage(landing: false);
                                                            this.ViewModel.MediaFiles[index]
                                                                .IsLanding = false;
                                                        }
                                                    }), hideImageMenuCommand: ReactiveCommand.CreateFromTask<int>(
                                                    async index =>
                                                    {
                                                        await this.Navigation.PopPopupAsync();
                                                        await Task.Delay(TimeSpan.FromMilliseconds(100));
                                                    }), index: image.Index);

                                            await this.Navigation.PushPopupAsync(page: page);
                                        }, outputScheduler: RxApp.MainThreadScheduler)
                                    });

                                    await MainThread.InvokeOnMainThreadAsync(() =>
                                    {
                                        this.LayoutImages.Children.Insert(_imageCount - 1, image);
                                        this._imageCount += 1;
                                        this.ViewModel.MediaFiles.Add(imageFile);
                                    });
                                }
                            }
                        };
                        await Task.Delay(TimeSpan.FromMilliseconds(100));
                        await cropper.Show(this);
                    }
                }).DisposeWith(_compositeDisposable);

            this.WhenAnyValue(view => view.EntryComment.Text,
                    view => view.ComboBoxSelectedArea.SelectedItem,
                    view => view.ComboBoxSelectedEvent.SelectedItem,
                    view => view.ComboBoxTaskCategory.SelectedItem)
                .Subscribe(tuble =>
                {
                    // only enable create new diary button if fill up data
                    this.ButtonPostNewDiary.IsEnabled = !string.IsNullOrEmpty(tuble.Item1)
                                                        && tuble.Item2 != null
                                                        && tuble.Item3 != null
                                                        && tuble.Item4 != null;
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