using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace XCTApplication.Controls
{
    public class WrapLayout : Layout<View>
    {
        public static BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(WrapLayout), default,
                defaultBindingMode: BindingMode.OneWay,
                propertyChanged: (bindable, oldvalue, newvalue) =>
                    ((WrapLayout) bindable).OnChildrenChanged(oldvalue, newvalue));

        public IList ItemsSource
        {
            get => (IList) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static BindableProperty ItemTemplateProperty =
            BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(WrapLayout));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static BindableProperty ItemTappedCommandProperty =
            BindableProperty.Create(nameof(ItemTappedCommand), typeof(ICommand), typeof(WrapLayout));

        public ICommand ItemTappedCommand
        {
            get => (ICommand) GetValue(ItemTappedCommandProperty);
            set => SetValue(ItemTappedCommandProperty, value);
        }

        public static BindableProperty ItemTappedCommandParameterProperty =
            BindableProperty.Create(nameof(ItemTappedCommandParameter), typeof(object), typeof(WrapLayout));

        public object ItemTappedCommandParameter
        {
            get => GetValue(ItemTappedCommandParameterProperty);
            set => SetValue(ItemTappedCommandParameterProperty, value);
        }

        public static BindableProperty SpacingProperty =
            BindableProperty.Create(nameof(Spacing), typeof(double), typeof(WrapLayout), default(double),
                defaultBindingMode: BindingMode.OneWay,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout) bindable).InvalidateMeasure());

        /// <summary>
        /// Spacing added between elements
        /// </summary>
        public double Spacing
        {
            get => (double) GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public static BindableProperty UniformColumnsProperty =
            BindableProperty.Create(nameof(UniformColumns), typeof(int), typeof(WrapLayout), default(int),
                defaultBindingMode: BindingMode.OneWay,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout) bindable).InvalidateMeasure());

        /// <summary>
        ///  number for uniform child width
        /// </summary>
        public int UniformColumns
        {
            get => (int) GetValue(UniformColumnsProperty);
            set => SetValue(UniformColumnsProperty, value);
        }

        public static BindableProperty IsSquareProperty =
            BindableProperty.Create(nameof(IsSquare), typeof(bool), typeof(WrapLayout), false,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout) bindable).InvalidateMeasure());

        /// <summary>
        ///  make item height equal to item width when UniformColums > 0
        /// </summary>
        public bool IsSquare
        {
            get => (bool) GetValue(IsSquareProperty);
            set => SetValue(IsSquareProperty, value);
        }

        public event EventHandler<ItemTappedEventArgs> ItemTapped;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (WidthRequest > 0)
                widthConstraint = Math.Min(widthConstraint, WidthRequest);
            if (HeightRequest > 0)
                heightConstraint = Math.Min(heightConstraint, HeightRequest);

            var internalWidth = double.IsPositiveInfinity(widthConstraint)
                ? double.PositiveInfinity
                : Math.Max(0, widthConstraint);
            var internalHeight = double.IsPositiveInfinity(heightConstraint)
                ? double.PositiveInfinity
                : Math.Max(0, heightConstraint);

            if (double.IsPositiveInfinity(widthConstraint) && double.IsPositiveInfinity(heightConstraint))
            {
                return new SizeRequest(Size.Zero, Size.Zero);
            }

            return UniformColumns > 0
                ? UniformMeasureAndLayout(internalWidth, internalHeight)
                : VariableMeasureAndLayout(internalWidth, internalHeight);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (UniformColumns > 0)
            {
                UniformMeasureAndLayout(width, height, true, x, y);
            }
            else
            {
                VariableMeasureAndLayout(width, height, true, x, y);
            }
        }

        private void OnChildrenChanged(object oldvalue, object newvalue)
        {
            if (oldvalue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            if (newvalue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnItemsSourceCollectionChanged;
            }

            if (!(newvalue is IList items))
                return;

            Children.Clear();

            foreach (var item in items)
            {
                View view = ViewFor(item);
                if (view != null)
                {
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += (s, e) => SendItemTapped(items.IndexOf(item), item);
                    view.GestureRecognizers.Add(tap);

                    Children.Add(view);
                }
            }
        }

        private View ViewFor(object item)
        {
            View view = null;

            if (ItemTemplate != null)
            {
                var content = ItemTemplate.CreateContent();

                view = content is View view1 ? view1 : ((ViewCell) content).View;

                view.BindingContext = item;
            }

            return view;
        }

        private void SendItemTapped(int index, object item)
        {
            try
            {
                var param = ItemTappedCommandParameter ?? item;

                if (ItemTappedCommand != null && ItemTappedCommand.CanExecute(param))
                {
                    ItemTappedCommand.Execute(param);
                }

                ItemTapped?.Invoke(this, new ItemTappedEventArgs(this, item, index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    Children.Clear();
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (!(e.NewItems is IList items))
                        return;

                    foreach (var item in items)
                    {
                        View view = ViewFor(item);
                        if (view != null)
                        {
                            var tap = new TapGestureRecognizer();
                            tap.Tapped += (s, args) => SendItemTapped(items.IndexOf(item), item);
                            view.GestureRecognizers.Add(tap);

                            Children.Add(view);
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    Children.RemoveAt(e.OldStartingIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private SizeRequest UniformMeasureAndLayout(double widthConstraint, double heightConstraint,
            bool doLayout = false, double x = 0, double y = 0)
        {
            double totalWidth = 0;
            double totalHeight = 0;
            double rowHeight = 0;
            double minWidth = 0;
            double minHeight = 0;
            var xPos = x;
            var yPos = y;

            totalWidth = widthConstraint;
            var exceptedWidth = widthConstraint - (UniformColumns - 1) * Spacing; //excepted spacing width

            var columnsSize = exceptedWidth / UniformColumns;
            if (columnsSize < 1)
            {
                columnsSize = 1;
            }

            foreach (var child in Children.Where(c => c.IsVisible))
            {
                var size = child.Measure(widthConstraint, heightConstraint);
                var itemWidth = columnsSize;
                var itemHeight = size.Request.Height;

                if (IsSquare)
                {
                    itemHeight = columnsSize;
                }

                rowHeight = Math.Max(rowHeight, itemHeight + Spacing);

                minHeight = Math.Max(minHeight, itemHeight);
                minWidth = Math.Max(minWidth, itemWidth);

                if (doLayout)
                {
                    var region = new Rectangle(xPos, yPos, itemWidth, itemHeight);
                    LayoutChildIntoBoundingRegion(child, region);
                }

                xPos += itemWidth + Spacing;

                if (xPos + columnsSize - x > widthConstraint)
                {
                    xPos = x;
                    yPos += rowHeight;
                    totalHeight += rowHeight;
                    rowHeight = 0;
                }
            }

            totalHeight = Math.Max(totalHeight + rowHeight - Spacing, 0);

            return new SizeRequest(new Size(totalWidth, totalHeight), new Size(minWidth, minHeight));
        }

        private SizeRequest VariableMeasureAndLayout(double widthConstraint, double heightConstraint,
            bool doLayout = false, double x = 0, double y = 0)
        {
            double totalWidth = 0;
            double totalHeight = 0;
            double rowHeight = 0;
            double rowWidth = 0;
            double minWidth = 0;
            double minHeight = 0;
            var xPos = x;
            var yPos = y;

            var visibleChildren = Children.Where(c => c.IsVisible).Select(c => new
            {
                child = c,
                size = c.Measure(widthConstraint, heightConstraint)
            });

            var enumerable = visibleChildren.ToList();
            var nextChildren = enumerable.Skip(1).ToList();
            nextChildren.Add(null); //make element count same

            var zipChildren = enumerable.Zip(nextChildren, (c, n) => new {current = c, next = n});

            foreach (var childBlock in zipChildren)
            {
                var child = childBlock.current.child;
                var size = childBlock.current.size;
                var itemWidth = size.Request.Width;
                var itemHeight = size.Request.Height;

                rowHeight = Math.Max(rowHeight, itemHeight + Spacing);
                rowWidth += itemWidth + Spacing;

                minHeight = Math.Max(minHeight, itemHeight);
                minWidth = Math.Max(minWidth, itemWidth);

                if (doLayout)
                {
                    var region = new Rectangle(xPos, yPos, itemWidth, itemHeight);
                    LayoutChildIntoBoundingRegion(child, region);
                }

                if (childBlock.next == null)
                {
                    totalHeight += rowHeight;
                    totalWidth = Math.Max(totalWidth, rowWidth);
                    break;
                }

                xPos += itemWidth + Spacing;
                var nextWidth = childBlock.next.size.Request.Width;

                if (xPos + nextWidth - x > widthConstraint)
                {
                    xPos = x;
                    yPos += rowHeight;
                    totalHeight += rowHeight;
                    totalWidth = Math.Max(totalWidth, rowWidth);
                    rowHeight = 0;
                    rowWidth = 0;
                }
            }

            totalWidth = Math.Max(totalWidth - Spacing, 0);
            totalHeight = Math.Max(totalHeight - Spacing, 0);

            return new SizeRequest(new Size(totalWidth, totalHeight), new Size(minWidth, minHeight));
        }
    }
}