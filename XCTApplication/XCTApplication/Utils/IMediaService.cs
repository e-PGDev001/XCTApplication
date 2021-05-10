using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XCTApplication.Utils
{
    public interface IMediaService
    {
        Task OpenGallery();

        Task OpenCameraAndGallery();

        Task<string> CreatePreviewPath(string path, string type);

        void ClearFiles(List<string> filePaths);
    }

    public class MediaChooser
    {
        public static MediaChooser Current { get; set; }

        public MediaChooser()
        {
            Current = this;
        }

        public string TakePhotoTitle { get; set; } = "Take Photo";

        public string PhotoLibraryTitle { get; set; } = "Photo Library";

        public string CancelButtonTitle { get; set; } = "Cancel";

        public string SelectSourceTitle { get; set; } = "Select source";

        public Action<IList<MediaFile>> Success { get; set; }

        public Action Failure { get; set; }

        public async Task Show(Page page)
        {
            var cameraPer = await PermissionsHelper.NewCheckPermission(Permission.Camera);
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            var photoPer = await PermissionsHelper.NewCheckPermission(Permission.Photos);
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            var storagePer = await PermissionsHelper.NewCheckPermission(Permission.Storage);
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            if (!cameraPer && !photoPer && !storagePer) return;

            await CrossMedia.Current.Initialize();
            var srv = DependencyService.Get<IMediaService>();
            var action = await page.DisplayActionSheet(SelectSourceTitle, CancelButtonTitle, null,
                TakePhotoTitle, PhotoLibraryTitle);
            if (string.IsNullOrEmpty(action) || action.Equals(CancelButtonTitle)) return;

            if (action == TakePhotoTitle)
            {
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await page.DisplayAlert("No Camera", ":( No camera available.", "OK");
                    Failure?.Invoke();
                    return;
                }

                // small delay
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                await MainThread.InvokeOnMainThreadAsync(async () => { await srv.OpenCameraAndGallery(); });
            }
            else
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await page.DisplayAlert("Error", "This device is not supported to pick photo.", "OK");
                    Failure?.Invoke();
                    return;
                }

                // small delay
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                await MainThread.InvokeOnMainThreadAsync(async () => { await srv.OpenGallery(); });
            }
        }

        public async Task ShowSingleSelection(Page page)
        {
            var permission = await PermissionsHelper.NewCheckPermission(Permission.Camera);
            var permission2 = await PermissionsHelper.NewCheckPermission(Permission.Storage);
            if (!permission || !permission2) return;

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await page.DisplayAlert("No Camera", ":( No camera available.", "OK");
                Failure?.Invoke();
                return;
            }

            var srv = DependencyService.Get<IMediaService>();
            // small delay
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await srv.OpenCameraAndGallery();
        }

        public async Task ShowMultipleSelection(Page page)
        {
            var permission = await PermissionsHelper.NewCheckPermission(Permission.Camera);
            var permission2 = await PermissionsHelper.NewCheckPermission(Permission.Storage);
            if (!permission || !permission2) return;

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await page.DisplayAlert("Error", "This device is not supported to pick photo.", "OK");
                Failure?.Invoke();
                return;
            }

            var srv = DependencyService.Get<IMediaService>();
            // small delay
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await srv.OpenGallery();
        }
    }

    public enum MediaFileType
    {
        Image,
        Video
    }

    public class MediaFile
    {
        public string PreviewPath { get; set; }

        public string Path { get; set; }

        public MediaFileType Type { get; set; }

        public bool IsLanding { get; set; }
    }

    public static class MediaFileHelper
    {
        public static string GetUniquePath(MediaFileType type, string path, string name)
        {
            var ext = Path.GetExtension(name);
            if (ext == string.Empty)
                ext = ((type == MediaFileType.Image) ? ".jpg" : ".mp4");

            name = Path.GetFileNameWithoutExtension(name);

            var nname = name + ext;
            var i = 1;
            while (File.Exists(Path.Combine(path, nname)))
                nname = name + "_" + (i++) + ext;

            return Path.Combine(path, nname);
        }


        public static string GetOutputPath(MediaFileType type, string path, string name)
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path);
            Directory.CreateDirectory(path);

            if (string.IsNullOrWhiteSpace(name))
            {
                var timestamp = DateTime.Now.ToString("yyyMMdd_HHmmss");
                if (type == MediaFileType.Image)
                    name = "IMG_" + timestamp + ".jpg";
                else
                    name = "VID_" + timestamp + ".mp4";
            }

            return Path.Combine(path, GetUniquePath(type, path, name));
        }
    }
}