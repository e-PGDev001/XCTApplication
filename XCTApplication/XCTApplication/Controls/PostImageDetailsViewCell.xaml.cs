namespace XCTApplication.Controls
{
    public partial class PostImageDetailsViewCell
    {
        public PostImageDetailsViewCell()
        {
            InitializeComponent();
        }

        public int Index { get; set; }

        public void SetImage(string imageFile)
        {
            this.ImageDes.Source = imageFile;
        }

        public void SetAsLandingImage(bool landing)
        {
            this.GridLandingImage.IsVisible = landing;
        }
    }
}