using System;
using System.Windows.Input;

namespace XCTApplication.Controls
{
    public partial class ImageSelectionPageView
    {
        private readonly ICommand _removeImageCommand;
        private readonly ICommand _setAsLandingImageCommand;
        private readonly ICommand _hideImageMenuCommand;

        private readonly int _index;

        public ImageSelectionPageView(string imageFile, ICommand removeImageCommand,
            ICommand setAsLandingImageCommand, ICommand hideImageMenuCommand, int index)
        {
            InitializeComponent();

            this.ImageDes.Source = imageFile;
            _removeImageCommand = removeImageCommand;
            _setAsLandingImageCommand = setAsLandingImageCommand;
            _hideImageMenuCommand = hideImageMenuCommand;
            _index = index;
        }

        private void SetAsLandingImage(object sender, EventArgs e)
        {
            _setAsLandingImageCommand?.Execute(_index);
        }

        private void RemoveImage(object sender, EventArgs e)
        {
            _removeImageCommand?.Execute(_index);
        }

        private void HideImageMenu(object sender, EventArgs e)
        {
            _hideImageMenuCommand?.Execute(_index);
        }
    }
}