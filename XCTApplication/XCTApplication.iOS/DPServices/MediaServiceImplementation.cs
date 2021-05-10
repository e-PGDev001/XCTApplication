using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GMImagePicker;
using Photos;
using UIKit;
using Xamarin.Forms;
using XCTApplication.iOS.DPServices;
using XCTApplication.Utils;

[assembly: Dependency(typeof(MediaServiceImplementation))]
namespace XCTApplication.iOS.DPServices
{
    public class MediaServiceImplementation : IMediaService, IDisposable
    {
        private GMImagePickerController _currentPicker;
        private TaskCompletionSource<IList<MediaFile>> _mediaPickTcs;
        private PHAsset[] _preselectedAssets;

        public MediaServiceImplementation()
        {
        }

        private void GeneratePicker()
        {
            _currentPicker = new GMImagePickerController
            {
                Title = "Pick an image",
                CustomDoneButtonTitle = "Finish",
                CustomCancelButtonTitle = "Cancel",
                ColsInPortrait = 3,
                ColsInLandscape = 5,
                MinimumInteritemSpacing = 2.0f,
                DisplaySelectionInfoToolbar = true,
                DisplayAlbumsNumberOfAssets = true,
                AllowsEditingCameraImages = true,
                ModalPresentationStyle = UIModalPresentationStyle.Popover,
                // Only Image
                MediaTypes = new[] { PHAssetMediaType.Image },
                // You can limit which galleries are available to browse through
                CustomSmartCollections = new[]
                {
                    PHAssetCollectionSubtype.SmartAlbumUserLibrary, PHAssetCollectionSubtype.AlbumRegular
                }
            };

            // GMImagePicker can be treated as a PopOver as well:
            var popPC = _currentPicker.PopoverPresentationController;
            if (popPC == null) return;
            popPC.PermittedArrowDirections = UIPopoverArrowDirection.Any;
            popPC.BarButtonItem = new UIBarButtonItem();
        }

        public async Task OpenGallery()
        {
            GeneratePicker();

            _currentPicker.ShouldSelectAsset += (sender, args) =>
            {
                args.Cancel = _currentPicker.SelectedAssets.Count > 24;
            };

            // Camera integration
            _currentPicker.AllowsMultipleSelection = true;
            _currentPicker.ShowCameraButton = false;
            _currentPicker.AutoSelectCameraImages = false;

            var mediaFiles = await PickMediaAsync();
            MediaChooser.Current.Success?.Invoke(mediaFiles);
        }

        public async Task OpenCameraAndGallery()
        {
            GeneratePicker();
            
            _currentPicker.ShouldSelectAsset += (sender, args) =>
            {
                args.Cancel = _currentPicker.SelectedAssets.Count > 1;
            };

            // Camera integration
            _currentPicker.AllowsMultipleSelection = false;
            _currentPicker.CameraButtonTintColor = UIColor.Purple;
            _currentPicker.ShowCameraButton = true;
            _currentPicker.AutoSelectCameraImages = true;

            var mediaFiles = await PickMediaAsync();
            MediaChooser.Current.Success?.Invoke(mediaFiles);
        }

        private async Task<IList<MediaFile>> PickMediaAsync()
        {
            _mediaPickTcs = new TaskCompletionSource<IList<MediaFile>>();

            _currentPicker.FinishedPickingAssets += FinishedPickingAssets;
            _currentPicker.PresentUsingRootViewController();

            var mediaFiles = await _mediaPickTcs.Task;

            _currentPicker.FinishedPickingAssets -= FinishedPickingAssets;
            return mediaFiles;
        }

        private async void FinishedPickingAssets(object sender, MultiAssetEventArgs args)
        {
            var results = new List<MediaFile>(24);
            var tcs = new TaskCompletionSource<IList<MediaFile>>();

            Debug.WriteLine("User finished picking assets. {0} items selected.", args.Assets.Length);

            _preselectedAssets = args.Assets;

            // For demo purposes: just show all chosen pictures in order every second
            foreach (var asset in _preselectedAssets)
            {
                // Get information about the asset, e.g. file patch
                asset.RequestContentEditingInput(new PHContentEditingInputRequestOptions(),
                    completionHandler: (input, options) =>
                    {
                        var path = input.FullSizeImageUrl.ToString();
                        path = path.Replace("file://", "");

                        var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                        var ext = System.IO.Path.GetExtension(path);
                        var imgOption = MediaFileGetImageOptions.CreateDefaultThumb();
                        var thumbImageBytes = AssetImageService.GetImageBytes(asset, imgOption);

                        var thumbnailImagePath =
                            MediaFileHelper.GetOutputPath(MediaFileType.Image, "TmpMedia",
                                $"{fileName}-THUMBNAIL{ext}");
                        System.IO.File.WriteAllBytes(thumbnailImagePath, thumbImageBytes);

                        results.Add(new MediaFile
                        {
                            Path = path,
                            PreviewPath = thumbnailImagePath,
                            Type = MediaFileType.Image
                        });
                    });

                await Task.Delay(250);
            }

            tcs.TrySetResult(results);
            _mediaPickTcs?.TrySetResult(await tcs.Task);
            _preselectedAssets = null;
        }

        public async Task<string> CreatePreviewPath(string path, string type)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            var ext = System.IO.Path.GetExtension(path);

            var strs = path.Split('/');
            var assetResult = PHAsset.FetchAssetsUsingLocalIdentifiers(strs, null);
            var asset = assetResult.firstObject as PHAsset;

            var thumbImageBytes =
                AssetImageService.GetImageBytes(asset, MediaFileGetImageOptions.CreateDefaultThumb());

            var thumbnailImagePath =
                MediaFileHelper.GetOutputPath(MediaFileType.Image, "TmpMedia",
                    $"{fileName}-THUMBNAIL{ext}");
            System.IO.File.WriteAllBytes(thumbnailImagePath, thumbImageBytes);

            return thumbnailImagePath;
        }

        public void ClearFiles(List<string> filePaths)
        {
            var documentsDirectory = Environment.GetFolderPath
                (Environment.SpecialFolder.Personal);

            if (System.IO.Directory.Exists(documentsDirectory))
            {
                foreach (var p in filePaths)
                {
                    System.IO.File.Delete(p);
                }
            }
        }

        /// <summary>
        /// dispose old session
        /// </summary>
        public void Dispose()
        {
            _currentPicker?.Dispose();
        }
    }

    public class MediaFileGetImageOptions
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public int Quality { get; set; }

        public MediaFileImageOrientation Orientation { get; set; }

        public ImageResizeAspect ResizeAspect { get; set; }

        public MediaFileGetImageOptions()
        {
            Orientation = MediaFileImageOrientation.Up;
            ResizeAspect = ImageResizeAspect.Fill;
            Quality = 100;
        }

        public static MediaFileGetImageOptions CreateDefaultThumb()
        {
            return new MediaFileGetImageOptions
            {
                Width = 200,
                Height = 200,
                Quality = 80,
                ResizeAspect = ImageResizeAspect.AspectFill
            };
        }

        public static MediaFileGetImageOptions CreateDefault()
        {
            return new MediaFileGetImageOptions
            {
                Quality = 90
            };
        }

        public enum ImageResizeAspect
        {
            /// <summary>
            /// Stretches the image to completely and exactly fill the display area. This may result in the image being distorted.
            /// </summary>
            Fill,

            /// <summary>
            /// Clips the image so that it fills the display area while preserving the aspect (ie.no distortion).
            /// </summary>
            AspectFill,

            /// <summary>
            /// Letterboxes the image(if required) so that the entire image fits into the display area, with blank space added to the top/bottom or sides depending on the whether the image is wide or tall.
            /// </summary>
            AspectFit
        }
    }

    public enum MediaFileImageOrientation
    {
        Up,
        UpMirrored,
        Down,
        DownMirrored,
        Left,
        LeftMirrored,
        Right,
        RightMirrored
    }

    public static class iOSViewExtensions
    {
        /// <summary>
        /// show view controller via extension
        /// </summary>
        /// <param name="controller"></param>
        public static void PresentUsingRootViewController(this UIViewController controller)
        {
            if (controller == null)
#if DEBUG
                throw new ArgumentNullException(nameof(controller));
#else
                return;
#endif
            var visibleViewController = GetVisibleViewController(null);
            visibleViewController?.PresentViewController(controller, true, () => { Debug.WriteLine("VC Completed"); });
        }

        public static void DissmissUsingRootViewController(this UIViewController controller)
        {
            var visibleViewController = GetVisibleViewController(null);
            visibleViewController?.DismissModalViewController(true);
        }

        public static UIViewController GetVisibleViewController(UIViewController controller)
        {
            if (controller == null)
            {
                controller = UIApplication.SharedApplication.KeyWindow.RootViewController;
            }

            if (controller?.NavigationController?.VisibleViewController != null)
            {
                return controller.NavigationController.VisibleViewController;
            }

            if (controller != null && (controller.IsViewLoaded && controller.View?.Window != null))
            {
                return controller;
            }

            if (controller != null)
            {
                foreach (var childViewController in controller.ChildViewControllers)
                {
                    var foundVisibleViewController = GetVisibleViewController(childViewController);
                    if (foundVisibleViewController == null)
                        continue;

                    return foundVisibleViewController;
                }
            }
            return controller;
        }

    }

    internal static class AssetImageService
    {
        internal static byte[] GetImageBytes(PHAsset asset, MediaFileGetImageOptions options)
        {
            nfloat w = options.Width;
            nfloat h = options.Height;

            switch (options.Orientation)
            {
                case MediaFileImageOrientation.Left:
                case MediaFileImageOrientation.LeftMirrored:
                case MediaFileImageOrientation.Right:
                case MediaFileImageOrientation.RightMirrored:
                    var wT = w;
                    w = h;
                    h = wT;
                    break;
            }

            if (w <= 0)
            {
                if (h > 0)
                {
                    w = asset.PixelWidth * h / asset.PixelHeight;
                }
                else
                {
                    w = asset.PixelWidth;
                }
            }

            if (h <= 0)
            {
                h = asset.PixelHeight * w / asset.PixelWidth;
            }

            byte[] imageBytes = null;

            PHImageManager.DefaultManager.RequestImageForAsset(
                asset,
                new CoreGraphics.CGSize(w, h),
                ImageResizeAspectToPH(options.ResizeAspect),
                new PHImageRequestOptions { Synchronous = true, ResizeMode = PHImageRequestOptionsResizeMode.Exact },
                (requestedImage, info) =>
                {
                    if (options.Orientation != MediaFileImageOrientation.Up ||
                        requestedImage.Size.Width != w ||
                        requestedImage.Size.Height != h)
                    {
                        var destW = w;
                        var destH = h;

                        if (options.ResizeAspect == MediaFileGetImageOptions.ImageResizeAspect.AspectFit)
                        {
                            var widthScale = w / requestedImage.Size.Width;
                            var heightScale = h / requestedImage.Size.Height;
                            var scale = (nfloat)Math.Min(widthScale, heightScale);

                            switch (options.Orientation)
                            {
                                case MediaFileImageOrientation.Left:
                                case MediaFileImageOrientation.LeftMirrored:
                                case MediaFileImageOrientation.Right:
                                case MediaFileImageOrientation.RightMirrored:
                                    h = requestedImage.Size.Width * scale;
                                    w = requestedImage.Size.Height * scale;
                                    destW = h;
                                    destH = w;
                                    break;

                                default:
                                    w = requestedImage.Size.Width * scale;
                                    h = requestedImage.Size.Height * scale;
                                    destW = w;
                                    destH = h;
                                    break;
                            }
                        }
                        else
                        {
                            switch (options.Orientation)
                            {
                                case MediaFileImageOrientation.Left:
                                case MediaFileImageOrientation.LeftMirrored:
                                case MediaFileImageOrientation.Right:
                                case MediaFileImageOrientation.RightMirrored:
                                    var wT = w;
                                    w = h;
                                    h = wT;
                                    break;
                            }
                        }

                        var cg = requestedImage.CGImage;
                        int bytesPerRow = (int)w * 4;
                        var ctx = new CoreGraphics.CGBitmapContext(null, (int)w, (int)h, 8, bytesPerRow,
                            cg.ColorSpace, CoreGraphics.CGImageAlphaInfo.PremultipliedLast);

                        Func<float, float> radians = (degrees) => { return degrees * ((float)Math.PI / 180f); };

                        switch (options.Orientation)
                        {
                            case MediaFileImageOrientation.UpMirrored:
                                ctx.TranslateCTM(w, 0);
                                ctx.ScaleCTM(-1f, 1f);
                                break;
                            case MediaFileImageOrientation.Down:
                                ctx.TranslateCTM(w, h);
                                ctx.RotateCTM(radians(180f));
                                break;
                            case MediaFileImageOrientation.DownMirrored:
                                ctx.TranslateCTM(0, h);
                                ctx.RotateCTM(radians(180f));
                                ctx.ScaleCTM(-1f, 1f);
                                break;
                            case MediaFileImageOrientation.Left:
                                ctx.TranslateCTM(w, 0);
                                ctx.RotateCTM(radians(90f));
                                break;
                            case MediaFileImageOrientation.LeftMirrored:
                                ctx.TranslateCTM(w, h);
                                ctx.RotateCTM(radians(270f));
                                ctx.ScaleCTM(1f, -1f);
                                break;
                            case MediaFileImageOrientation.Right:
                                ctx.TranslateCTM(0, h);
                                ctx.RotateCTM(radians(270f));
                                break;
                            case MediaFileImageOrientation.RightMirrored:
                                ctx.TranslateCTM(0, 0);
                                ctx.RotateCTM(radians(90f));
                                ctx.ScaleCTM(1f, -1f);
                                break;
                            default:
                                break;
                        }

                        ctx.DrawImage(new CoreGraphics.CGRect(0, 0, destW, destH), cg);

                        requestedImage = UIImage.FromImage(ctx.ToImage());

                        ctx.Dispose();
                    }

                    imageBytes = requestedImage.AsJPEG(options.Quality / 100f).ToArray();
                    requestedImage.Dispose();
                });

            return imageBytes;
        }

        private static PHImageContentMode ImageResizeAspectToPH(MediaFileGetImageOptions.ImageResizeAspect resizeAspect)
        {
            switch (resizeAspect)
            {
                case MediaFileGetImageOptions.ImageResizeAspect.AspectFill:
                    return PHImageContentMode.AspectFill;
                case MediaFileGetImageOptions.ImageResizeAspect.AspectFit:
                    return PHImageContentMode.AspectFit;
                default:
                    return PHImageContentMode.Default;
            }
        }
    }
}