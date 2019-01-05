using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ManimInteractive
{
    public class RelativeLayoutPanel : Panel
    {
        public static readonly DependencyProperty RelativeRectProperty = DependencyProperty.RegisterAttached(
            "RelativeRect", typeof(Rect), typeof(RelativeLayoutPanel),
            new FrameworkPropertyMetadata(new Rect(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static Rect GetRelativeRect(UIElement element)
        {
            return (Rect)element.GetValue(RelativeRectProperty);
        }

        public static void SetRelativeRect(UIElement element, Rect value)
        {
            element.SetValue(RelativeRectProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in InternalChildren)
            {
                var rect = GetRelativeRect(element);

                double newW = (rect.Width * finalSize.Width);
                double newH = (rect.Height * finalSize.Height);
                double newX = (rect.X * finalSize.Width) - (newW / 2);
                double newY = (rect.Y * finalSize.Height) - (newH / 2);

                element.Arrange(new Rect(
                    newX,
                    newY,
                    newW,
                    newH));
            }

            return finalSize;
        }
    }

    public class Draggable : Panel
    {
        #region Properties
        // TODO: public bool IsOriginCenterOfMass;

        public bool IsDraggable;
        public bool IsDeletable;
        private bool _dragging = false;
        public bool IsDragging {
            get {
                return _dragging;
            }
            internal set {
                bool oldValue = _dragging;
                _dragging = value;

                DragStateChanged?.Invoke(null, new DragStateChanged { OldState = oldValue, NewState = value });
            }
        }
        public event EventHandler<DragStateChanged> DragStateChanged;
        private Point mouseOffset = new Point();
        #endregion

        public Draggable() { }
        public Draggable(Rect rect, bool isDraggable = true)
        {
            SetRelativeRect(this, rect);
            IsDraggable = isDraggable;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            MouseDown += Draggable_MouseDown;
            MouseUp += Draggable_MouseUp;
            MouseMove += Draggable_MouseMove;
        }

        private void Draggable_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDraggable && IsMouseCaptured)
            {
                IsDragging = true;
                Point mouseDelta = Mouse.GetPosition(this);
                mouseDelta.Offset(-mouseOffset.X, -mouseOffset.Y);

                // Recalculate if in Viewport
                if (Parent != null)
                {
                    var view = Parent as Panel;
                    if (view != null)
                    {
                        var RelativeRect = GetRelativeRect(this);
                        RelativeRect.X += (mouseDelta.X / view.ActualWidth);
                        RelativeRect.Y += (mouseDelta.Y / view.ActualHeight);
                        SetRelativeRect(this, RelativeRect);
                    }
                }

                /*Margin = new Thickness(
                    Margin.Left + mouseDelta.X,
                    Margin.Top + mouseDelta.Y,
                    Margin.Right - mouseDelta.X,
                    Margin.Bottom - mouseDelta.Y
                );*/
            }
        }

        private void Draggable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDraggable)
            {
                // Drop item
                IsDragging = false;
                ReleaseMouseCapture();

                // Recalculate if in Viewport
                if (Parent != null)
                {
                    if (Parent is Panel)
                    {
                        RecalculateRelative(Parent as Panel);
                        Console.WriteLine(GetRelativeRect(this));
                    }
                }
            }
        }

        private void Draggable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDraggable)
            {
                mouseOffset = Mouse.GetPosition(this);
                CaptureMouse();
            }
        }

        /// <summary>
        /// Draws a red border with thickness 10 around the object
        /// </summary>
        /// <param name="canvas">The containing Viewport</param>
        public void DrawBorder()//RelativeLayoutPanel canvas)
        {
            //Rect bounds = TransformToVisual(canvas).TransformBounds(new Rect(RenderSize));
            Children.Add(new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(10)
            });
        }

        /// <summary>
        /// Moves the item to the specified position
        /// </summary>
        /// <param name="vector">Relative change</param>
        /// <param name="view">The containing view</param>
        public void MoveToLocation(Point vector, Panel view)
        {
            var rect = GetRelativeRect(this);
            rect.X = vector.X;
            rect.Y = vector.Y;
            SetRelativeRect(this, rect);
            view.InvalidateArrange();
        }

        public void SetX(Double NewX, Panel view)
        {
            MoveToLocation(new Point(NewX, GetRelativeRect(this).Y), view);
        }
        public void SetY(Double NewY, Panel view)
        {
            MoveToLocation(new Point(GetRelativeRect(this).X, NewY), view);
        }
        public void SetWidth(Double NewWidth, Panel view)
        {
            var rect = GetRelativeRect(this);
            rect.Width = NewWidth;
            SetRelativeRect(this, rect);
            view.InvalidateArrange();
        }
        public void SetHeight(Double NewHeight, Panel view)
        {
            var rect = GetRelativeRect(this);
            rect.Height = NewHeight;
            SetRelativeRect(this, rect);
            view.InvalidateArrange();
        }

        /// <summary>
        /// Recalculates the relative location according to the specified Viewport. Useful for resetting Margin offsets but keeping translation.
        /// </summary>
        /// <param name="view">The containing Viewport</param>
        private void RecalculateRelative(Panel view, bool ResetMargin = true)
        {
            Rect RelativeRect = GetRelativeRect(this);
            Point AbsLocation = TranslatePoint(new Point(0, 0), view);
            RelativeRect.X = (AbsLocation.X / view.ActualWidth) + (RelativeRect.Width / 2);
            RelativeRect.Y = (AbsLocation.Y / view.ActualHeight) + (RelativeRect.Height / 2);
            SetRelativeRect(this, RelativeRect);
            if (ResetMargin)
                Margin = new Thickness(0);

            // RelativeRect updated with new calculations, now force the containing view to rearrange
            view.InvalidateArrange();
        }

        public static void SetRelativeRect(UIElement element, Rect rect)
        {
            RelativeLayoutPanel.SetRelativeRect(element, rect);
        }
        public static Rect GetRelativeRect(UIElement element)
        {
            return RelativeLayoutPanel.GetRelativeRect(element);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 1)
            {
                var Child = Children[0];
                var rect = new Rect(0, 0, finalSize.Width, finalSize.Height);

                Child.Arrange(rect);
            }

            return finalSize;
        }
    }

    public class DragStateChanged : EventArgs
    {
        /// <summary>
        /// True if was previously dragging
        /// </summary>
        public bool OldState { get; set; }

        /// <summary>
        /// True if started dragging
        /// </summary>
        public bool NewState { get; set; }
    }
}
