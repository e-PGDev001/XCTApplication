using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Widget;
using Asksira.BSImagePickerLib;
using Bumptech.Glide;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using XCTApplication.Droid.DPServices;

namespace XCTApplication.Droid
{
    [Activity(Label = "Diary App", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, 
        BSImagePicker.IOnMultiImageSelectedListener, BSImagePicker.IOnSingleImageSelectedListener,
        BSImagePicker.IImageLoaderDelegate
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this);
            //This forces the custom renderers to be used
            Android.Glide.Forms.Init(this);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }

        public void OnMultiImageSelected(IList<Android.Net.Uri> uriList, string tag)
        {
            System.Diagnostics.Debug.WriteLine($"Creating media files for tag: {tag}");
            MediaServiceImplementationExs.ProceedSelectedImages(uriList);
        }

        public void OnSingleImageSelected(Android.Net.Uri uri, string tag)
        {
            System.Diagnostics.Debug.WriteLine($"Creating media files for tag: {tag}");
            MediaServiceImplementationExs.ProceedSelectedImage(uri);
        }

        public void LoadImage(Android.Net.Uri imageUri, ImageView ivImage)
        {
            // Glide is just an example. You can use any image loading library you want;
            // This callback is to make sure the library has the flexibility to allow user to choose their own image loading method.
            Glide.With(CrossCurrentActivity.Current.Activity).Load(imageUri).Into(ivImage);
        }
    }
}