using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XCTApplication.Controls;
using XCTApplication.Droid.ControlRenderer;
using PickerRenderer = Xamarin.Forms.Platform.Android.AppCompat.PickerRenderer;

[assembly: ExportRenderer(typeof(CustomPicker), typeof(CustomPickerRenderer))]

namespace XCTApplication.Droid.ControlRenderer
{
    public class CustomPickerRenderer : PickerRenderer
    {

        public CustomPickerRenderer(Context context) : base(context)
        {

        }


        private CustomPicker _element;

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            _element = (CustomPicker) this.Element;

            if (Control != null && this.Element != null && !string.IsNullOrEmpty(_element.Image))
                Control.Background = AddPickerStyles(_element.Image);

        }

        public LayerDrawable AddPickerStyles(string imagePath)
        {
            ShapeDrawable border = new ShapeDrawable();
            border.Paint.Color = Android.Graphics.Color.Gray;
            border.SetPadding(10, 10, 10, 10);
            border.Paint.SetStyle(Paint.Style.Stroke);

            Drawable[] layers = {border, AViewExtensions.GetDrawable(imagePath)};
            LayerDrawable layerDrawable = new LayerDrawable(layers);
            layerDrawable.SetLayerInset(0, 0, 0, 0, 0);

            return layerDrawable;
        }
    }

    public static class AViewExtensions
    {
        public static Drawable GetDrawable(string resourceName)
        {
            var context = CrossCurrentActivity.Current.AppContext;
            return context.GetDrawable(resourceName);
        }

        public static Drawable GetDrawable(int resourceId)
        {
            var context = CrossCurrentActivity.Current.AppContext;
            return context.GetDrawable(resourceId);
        }
    }
}