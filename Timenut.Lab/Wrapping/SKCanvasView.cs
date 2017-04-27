using System.Windows.Input;
using SkiaSharp.Views.WPF;
using Timenut.Lab.Input;
using System.Windows;

namespace Timenut.Lab.Wrapping
{
    public class SKCanvasView : SKElement
    {
        protected double Width => this.ActualWidth;
        protected double Height => this.ActualHeight;

        public void InvalidateSurface()
        {
            this.InvalidateVisual();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            Point pos = e.GetPosition(this);

            if (this is ITouchView touch)
                touch.OnDown((float)pos.X, (float)pos.Y);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            Point pos = e.GetPosition(this);

            if (this is ITouchView touch)
                touch.OnUp((float)pos.X, (float)pos.Y);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point pos = e.GetPosition(this);

            if (this is ITouchView touch)
                touch.OnMove((float)pos.X, (float)pos.Y);
        }
    }
}
