using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Timenut.Lab.Wrapping;

namespace Timenut.Lab.Views
{
    public class BaseCanvasView : SKCanvasView
    {
        internal float Density = 1;

        public BaseCanvasView()
        {
            OnLoadResources();
        }

        protected virtual void OnLoadResources() { }

        protected virtual void OnDispatchPaint(SKCanvas c) { }
        
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            e.Surface.Canvas.Scale(Density);
            OnDispatchPaint(e.Surface.Canvas);
        }
    }
}
