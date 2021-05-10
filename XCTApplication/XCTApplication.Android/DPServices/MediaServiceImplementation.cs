using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Graphics;
using System.Threading.Tasks;
using Asksira.BSImagePickerLib;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using XCTApplication.Droid.DPServices;
using XCTApplication.Utils;

[assembly: Dependency(typeof(MediaServiceImplementation))]
namespace XCTApplication.Droid.DPServices
{
    public class MediaServiceImplementation : IMediaService
    {
        public MediaServiceImplementation()
        {
        }

        public static AndroidX.Fragment.App.FragmentManager SupportFragmentManager => CrossCurrentActivity.Current
            .Activity.As<MainActivity>().SupportFragmentManager;

        public async Task OpenGallery()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var multiSelectionPicker = new BSImagePicker.Builder("com.companyname.xctapplication.fileprovider")
                .IsMultiSelect() //Set this if you want to use multi selection mode.
                .SetMinimumMultiSelectCount(1) //Default: 1.
                .SetMaximumMultiSelectCount(24) //Default: Integer.MAX_VALUE (i.e. User can select as many images as he/she wants)
                /*//Default: #FFFFFF. You can also set it to a translucent color.
                //.SetMultiSelectBarBgColor(Resource.Color.primary)
                .SetMultiSelectTextColor(Resource.Color
                    .primary) //Default: #212121(Dark grey). This is the message in the multi-select bottom bar.
                .SetMultiSelectDoneTextColor(Resource.Color
                    .accent) //Default: #388e3c(Green). This is the color of the "Done" TextView.
                .SetOverSelectTextColor(Resource.Color
                    .error_color_material) //Default: #b71c1c. This is the color of the message shown when user tries to select more than maximum select count.*/
                .DisableOverSelectionMessage() //You can also decide not to show this over select message              
                .SetSpanCount(3) //Default: 3. This is the number of columns
                .SetGridSpacing(Asksira.BSImagePickerLib.Utils.Dp2Px(2)) //Default: 2dp. Remember to pass in a value in pixel.
                .SetPeekHeight(Asksira.BSImagePickerLib.Utils.Dp2Px(360)) //Default: 360dp. This is the initial height of the dialog.
                .SetTag("A request ID") //Default: null. Set this if you need to identify which picker is calling back your fragment / activity.
                .Build();
            multiSelectionPicker.Show(SupportFragmentManager, "Picker");
        }

        public async Task OpenCameraAndGallery()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var singleSelectionPicker = new BSImagePicker.Builder("com.companyname.xctapplication.fileprovider")
            .SetMaximumDisplayingImages(24) //Default: Integer.MAX_VALUE. Don't worry about performance :)
                .SetSpanCount(3) //Default: 3. This is the number of columns
                .SetGridSpacing(Asksira.BSImagePickerLib.Utils.Dp2Px(2)) //Default: 2dp. Remember to pass in a value in pixel.
                .SetPeekHeight(Asksira.BSImagePickerLib.Utils.Dp2Px(360)) //Default: 360dp. This is the initial height of the dialog.
                .HideGalleryTile() //Default: show. Set this if you don't want to further let user select from a gallery app. In such case, I suggest you to set maximum displaying images to Integer.MAX_VALUE.
                .SetTag("A request ID") //Default: null. Set this if you need to identify which picker is calling back your fragment / activity.
                .Build();
            singleSelectionPicker.Show(SupportFragmentManager, "Picker");
        }

        public Task<string> CreatePreviewPath(string path, string type)
        {
            return Task.FromResult(MediaServiceImplementationExs.CreatePreviewPath(path, type));
        }

        public void ClearFiles(List<string> filePaths)
        {
            foreach (var p in filePaths.Where(File.Exists))
            {
                File.Delete(p);
            }
        }
    }

    public static class MediaServiceImplementationExs
    {
        public static void ProceedSelectedImage(Android.Net.Uri uri)
        {
            var path = uri.Path.StartsWith("file://") ? uri.Path : uri.GetRealPathFromUri(CrossCurrentActivity.Current.AppContext);
            if(string.IsNullOrEmpty(path)) return;
            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            var ext = System.IO.Path.GetExtension(path) ?? string.Empty;
            var thumbImage = ImageHelpers.RotateImage(path, 0.25f);

            var thumbnailImagePath =
                MediaFileHelper.GetOutputPath(MediaFileType.Image, "TmpMedia",
                    $"{fileName}-THUMBNAIL{ext}");
            File.WriteAllBytes(thumbnailImagePath, thumbImage);

            var mediaFile = new MediaFile
            {
                Path = path,
                PreviewPath = thumbnailImagePath,
                Type = MediaFileType.Image
            };

            MediaChooser.Current.Success?.Invoke(new List<MediaFile> {mediaFile});
        }

        public static void ProceedSelectedImages(IList<Android.Net.Uri> uriList)
        {
            var mediaFiles = new List<MediaFile>(capacity: 24);

            foreach (var uri in uriList)
            {
                var path = uri.Path.StartsWith("file://") ? uri.Path : uri.GetRealPathFromUri(CrossCurrentActivity.Current.AppContext);
                if(string.IsNullOrEmpty(path)) continue;
                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var ext = System.IO.Path.GetExtension(path) ?? string.Empty;
                var thumbImage = ImageHelpers.RotateImage(path, 0.25f);

                var thumbnailImagePath =
                    MediaFileHelper.GetOutputPath(MediaFileType.Image, "TmpMedia",
                        $"{fileName}-THUMBNAIL{ext}");
                File.WriteAllBytes(thumbnailImagePath, thumbImage);

                mediaFiles.Add(new MediaFile
                {
                    Path = path,
                    PreviewPath = thumbnailImagePath,
                    Type = MediaFileType.Image
                });
            }

            if(!mediaFiles.Any()) return;

            MediaChooser.Current.Success?.Invoke(mediaFiles);
        }

        public static string CreatePreviewPath(string path, string type)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            var ext = System.IO.Path.GetExtension(path) ?? string.Empty;
            var thumbImage = ImageHelpers.RotateImage(path, 0.25f);

            var thumbnailImagePath =
                MediaFileHelper.GetOutputPath(MediaFileType.Image, "TmpMedia",
                    $"{fileName}-THUMBNAIL{ext}");
            File.WriteAllBytes(thumbnailImagePath, thumbImage);

            return thumbnailImagePath;
        }
    }

    public static class ImageHelpers
    {
        public static byte[] RotateImage(string path, float scaleFactor, int quality = 90)
        {
            byte[] imageBytes;

            var originalImage = BitmapFactory.DecodeFile(path);
            var rotation = GetRotation(path);
            var width = originalImage.Width * scaleFactor;
            var height = originalImage.Height * scaleFactor;
            var scaledImage = Bitmap.CreateScaledBitmap(originalImage, (int) width, (int) height, true);

            var rotatedImage = scaledImage;
            if (rotation != 0)
            {
                var matrix = new Matrix();
                matrix.PostRotate(rotation);
                rotatedImage = Bitmap.CreateBitmap(scaledImage, 0, 0, scaledImage.Width,
                    scaledImage.Height, matrix, true);
                scaledImage.Recycle();
                scaledImage.Dispose();
            }

            using (var ms = new MemoryStream())
            {
                rotatedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, ms);
                imageBytes = ms.ToArray();
            }

            originalImage?.Dispose();
            rotatedImage?.Dispose();
            GC.Collect();

            return imageBytes;
        }

        public static int GetRotation(string filePath)
        {
            using var ei = new Android.Media.ExifInterface(filePath);
            var orientation = (Android.Media.Orientation) ei.GetAttributeInt(
                tag: Android.Media.ExifInterface.TagOrientation,
                defaultValue: (int) Android.Media.Orientation.Normal);

            switch (orientation)
            {
                case Android.Media.Orientation.Rotate90:
                    return 90;

                case Android.Media.Orientation.Rotate180:
                    return 180;

                case Android.Media.Orientation.Rotate270:
                    return 270;

                default:
                    return 0;
            }
        }

        public static string GetRealPathFromUri(this Android.Net.Uri contentUri, Android.Content.Context context)
        {
            try
            {
                var fullPathToImage = "";

                var imageCursor = context.ContentResolver.Query(contentUri, null, null, null, null);
                imageCursor.MoveToFirst();
                var idx = imageCursor.GetColumnIndex(Android.Provider.MediaStore.Images.ImageColumns.Data);

                if (idx != -1)
                {
                    fullPathToImage = imageCursor.GetString(idx);
                }
                else
                {
                    var documentId = Android.Provider.DocumentsContract.GetDocumentId(contentUri);
                    var id = documentId.Split(':')[1];
                    var whereSelect = Android.Provider.MediaStore.Images.ImageColumns.Id + "=?";
                    var projections = new string[] {Android.Provider.MediaStore.Images.ImageColumns.Data};

                    var cursor = context.ContentResolver.Query(
                        Android.Provider.MediaStore.Images.Media.InternalContentUri,
                        projections, whereSelect, new string[] {id}, null);
                    if (cursor.Count == 0)
                    {
                        cursor = context.ContentResolver.Query(
                            Android.Provider.MediaStore.Images.Media.ExternalContentUri, projections, whereSelect,
                            new string[] {id}, null);
                    }

                    var colData = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.ImageColumns.Data);
                    cursor.MoveToFirst();
                    fullPathToImage = cursor.GetString(colData);
                }

                return fullPathToImage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }
    }
}