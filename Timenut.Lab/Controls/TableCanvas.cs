using System;

using Timenut.Lab.Views;
using Timenut.Lab.Input;
using Timenut.Lab.Utils;

using SkiaSharp;

using w = Timenut.Lab.Wrapping;

using BindableObject = System.Windows.DependencyObject;
using BindableProperty = System.Windows.DependencyProperty;
using System.Collections.Generic;

namespace Timenut.Lab.Controls
{
    enum ScrollOrientation
    {
        None,
        Vertical,
        Horizontal
    }

    public class TableCanvas : BaseCanvasView, ITouchView
    {
        #region [ Property ]
        public static readonly BindableProperty StartDateProperty =
            w.BindableProperty.Create(
                nameof(StartDate), typeof(DateTime), typeof(TableCanvas), DateTime.Now);

        public static readonly BindableProperty CurrentDateProperty =
            w.BindableProperty.Create(
                nameof(CurrentDate), typeof(DateTime), typeof(TableCanvas), DateTime.Now);

        public static readonly BindableProperty HorizontalOffsetProperty =
            w.BindableProperty.Create(
                nameof(HorizontalOffset), typeof(float), typeof(TableCanvas), 0f,
                propertyChanged: HorizontalOffset_Changed);

        public static readonly BindableProperty VerticalOffsetProperty =
            w.BindableProperty.Create(
                nameof(VerticalOffset), typeof(float), typeof(TableCanvas), 0f,
                propertyChanged: VerticalOffset_Changed);

        public static readonly BindableProperty VirtualHeightProperty =
            w.BindableProperty.Create(
                nameof(VirtualHeight), typeof(double), typeof(TableCanvas), -1d);

        public static readonly BindableProperty MonthProperty =
            w.BindableProperty.Create(
                nameof(Month), typeof(string), typeof(TableCanvas), "1월");

        public static readonly BindableProperty IsNotTodayPostionProperty =
            w.BindableProperty.Create(nameof(IsNotTodayPosition), typeof(bool), typeof(TableCanvas), false);

        private static void VerticalOffset_Changed(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TableCanvas).OnVerticalOffsetChanged();
        }

        private static void HorizontalOffset_Changed(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TableCanvas).OnHorizontalOffsetChanged();
        }

        public DateTime StartDate
        {
            get { return (DateTime)GetValue(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public DateTime CurrentDate
        {
            get { return (DateTime)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }

        public float HorizontalOffset
        {
            get { return (float)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public float VerticalOffset
        {
            get { return (float)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public double VirtualHeight
        {
            get { return (double)GetValue(VirtualHeightProperty); }
            set { SetValue(VirtualHeightProperty, value); }
        }
        public string Month
        {
            get { return (string)GetValue(MonthProperty); }
            set { SetValue(MonthProperty, value); }
        }
        public bool IsNotTodayPosition
        {
            get { return (bool)GetValue(IsNotTodayPostionProperty); }
            set { SetValue(IsNotTodayPostionProperty, value); }
        }
        #endregion

        #region [ Resources ]
#if DEBUG
        SKPaint debugPaint;
#endif
        SKPaint linePaint;
        SKPaint rectPaint;
        SKPaint todayPaint;
        SKPaint hourPaint;
        SKPaint hourRectPaint;
        SKPaint dayNamePaint;

        SKRect rect;

        #endregion

        #region [ Scroll Data ]
        const int ScrollThreshold = 30;

        ScrollOrientation orientation;

        bool isScroll = false;
        SKPoint beginPosition;
        float hourBarOffset;
        float beginHorizontalOffset;
        float beginVerticalOffset;
        float columnNum;
        float headerOffset;
        #endregion
       

        public TableCanvas()
        {
            this.hourBarOffset = 30;
            this.headerOffset = 30;
            this.columnNum = 5;
            this.VirtualHeight = 2000;
        }

        protected override void OnLoadResources()
        {
            linePaint = new SKPaint()
            {
                Color = SKColors.LightGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            hourRectPaint = new SKPaint()
            {
                Color = SKColor.Parse("#f6f6f6")
            };
            hourPaint = new SKPaint()
            {
                Color = SKColor.Parse("#989898"),
                TextAlign = SKTextAlign.Center,
                TextSize = 12
            };
            dayNamePaint = new SKPaint()
            {
                Color = SKColors.White,
                TextAlign = SKTextAlign.Center,
                TextSize = 12
            };
            rectPaint = new SKPaint()
            {
                Color = SKColor.Parse("#ff7962")
            };
            todayPaint = new SKPaint()
            {
                Color = SKColor.Parse("#60ffffff")
            };

#if DEBUG
            debugPaint = new SKPaint()
            {
                Color = SKColors.Blue,
                TextAlign = SKTextAlign.Left
            };
#endif
        }

        protected virtual void OnHorizontalOffsetChanged()
        {
            if (Math.Abs(HorizontalOffset) > (Width / 2))
                IsNotTodayPosition = true;
            else
                IsNotTodayPosition = false;
            this.InvalidateSurface();
        }

        private void OnVerticalOffsetChanged()
        {
            this.InvalidateSurface();
        }

        protected override void OnDispatchPaint(SKCanvas c)
        {
            c.Clear();

            // Translation
            c.Translate(new SKPoint(-HorizontalOffset + hourBarOffset, -VerticalOffset));

            // Header


            this.DrawHourBar(c);
            foreach (int hourOffset in this.GetIntersectHourOffsets())
            {
                this.DrawRowLine(c, hourOffset);
                this.DrawHour(c, hourOffset);
            }

            this.DrawHeader(c);

            foreach (int dayOffset in this.GetIntersectDayOffsets())
            {
                this.DrawColumnLine(c, dayOffset);
                this.DrawColumnHeader(c, dayOffset);
            }


            this.CurrentDate = StartDate.AddDays(this.MeasureEndIndex());


#if DEBUG
            // Debug
            c.DrawText($"HorizontalOffset: {HorizontalOffset}", 10 + HorizontalOffset, 100 + VerticalOffset, debugPaint);
            c.DrawText($"VerticalOffset: {VerticalOffset}", 10 + HorizontalOffset, 120 + VerticalOffset, debugPaint);
            c.DrawText($"StartIndex: {this.MeasureStartIndex()}", 10 + HorizontalOffset, 140 + VerticalOffset, debugPaint);
            c.DrawText($"EndIndex: {this.MeasureEndIndex()}", 10 + HorizontalOffset, 160 + VerticalOffset, debugPaint);
#endif
        }

        #region [ Drawing ]
        protected virtual void DrawHeader(SKCanvas c)
        {
            rect = new SKRect(-hourBarOffset, 0, (float)Width, headerOffset);

            c.DrawRect(Abs(ref rect), rectPaint);
        }

        protected virtual void DrawColumnHeader(SKCanvas c, int dayOffset)
        {
            float boxWidth = this.MeasureBoxWidth();
            float left = dayOffset * boxWidth;
            DateTime date = StartDate.AddDays(dayOffset);

            string weekName = date.GetDayOfWeekName();

            this.BeginClip(c, 0, 0, (float)(Width - hourBarOffset), headerOffset);

            // Today
            if (StartDate == date)
            {
                rect = new SKRect(left + boxWidth / 6, 5, left + boxWidth * 5 / 6, 25);

                c.DrawRoundRect(AbsY(ref rect), 100, 100, todayPaint);
            }

            c.DrawText($"{date.Day} {weekName}", left + boxWidth / 2f, 20 + VerticalOffset, dayNamePaint);

            this.EndClip(c);
        }

        protected virtual void DrawRowLine(SKCanvas c, int hourOffset)
        {
            float boxWidth = this.MeasureBoxWidth();

            c.DrawLine(
                0 + HorizontalOffset, hourOffset * boxWidth + headerOffset,
                (float)Width + HorizontalOffset, hourOffset * boxWidth + headerOffset,
                linePaint);
        }
        protected virtual void DrawHourBar(SKCanvas c)
        {
            rect = new SKRect(-hourBarOffset, headerOffset, 0, (float)VirtualHeight);

            c.DrawRect(Abs(ref rect), hourRectPaint);
        }

        protected virtual void DrawHour(SKCanvas c, int hourOffset)
        {
            //this.BeginClip(c, 0, headerOffset, hourBarOffset, (float)Height);
            float boxWidth = this.MeasureBoxWidth();
            float textSizeOffset = (hourPaint.TextSize / 2f);
            if (hourOffset == 0)
                textSizeOffset = hourPaint.TextSize;
            else if (hourOffset == 24)
                textSizeOffset = -hourPaint.TextSize;
            c.DrawText($"{hourOffset}", AbsX((-hourBarOffset / 2f)),
                hourOffset * boxWidth + headerOffset + textSizeOffset, hourPaint);
            //this.EndClip(c);
        }

        protected virtual void DrawColumnLine(SKCanvas c, int dayOffset)
        {
            float boxWidth = this.MeasureBoxWidth();

            this.BeginClip(c, 0, headerOffset, (float)Width, (float)Height);

            c.DrawLine(
                dayOffset * boxWidth, headerOffset + VerticalOffset,
                dayOffset * boxWidth, (float)VirtualHeight,
                linePaint);

            this.EndClip(c);
        }


        protected virtual IEnumerable<int> GetIntersectHourOffsets()
        {
            // Day box
            for (int i = 0; i <= 24; i++)
            {
                yield return i;
            }
        }

        protected virtual IEnumerable<int> GetIntersectDayOffsets()
        {
            // Day box
            for (int i = MeasureStartIndex(); i <= MeasureEndIndex(); i++)
            {
                yield return i;
            }
        }
        #endregion

        #region [ Absolute Functions ]
        protected virtual SKRect Abs(ref SKRect rect)
        {
            rect.Left += HorizontalOffset;
            rect.Top += VerticalOffset;
            rect.Right += HorizontalOffset;
            rect.Bottom += VerticalOffset;

            return rect;
        }
        protected virtual SKPoint Abs(ref SKPoint point)
        {
            point.X += HorizontalOffset;
            point.Y += VerticalOffset;

            return point;
        }
        protected virtual SKRect AbsX(ref SKRect rect)
        {
            rect.Left += HorizontalOffset;
            rect.Right += HorizontalOffset;

            return rect;
        }
        protected virtual SKRect AbsY(ref SKRect rect)
        {
            rect.Top += VerticalOffset;
            rect.Bottom += VerticalOffset;

            return rect;
        }
        protected virtual float AbsX(float X)
        {
            X += HorizontalOffset;

            return X;
        }
        protected virtual float AbsY(float Y)
        {
            Y += VerticalOffset;

            return Y;
        }
        #endregion

        #region [ Clipping ]
        private void BeginClip(SKCanvas c, float x, float y, float width, float height)
        {
            var rect = new SKRect(x, y, x + width, y + height);

            c.Save();
            c.ClipRect(Abs(ref rect));
        }

        private void EndClip(SKCanvas c)
        {
            c.Restore();
        }
        #endregion

        #region [ Measure ]
        protected virtual int MeasureStartIndex()
        {
            float boxWidth = this.MeasureBoxWidth();

            return (int)Math.Floor(HorizontalOffset / boxWidth);
        }

        protected virtual int MeasureEndIndex()
        {
            float boxWidth = this.MeasureBoxWidth();

            return (int)Math.Ceiling((Width + HorizontalOffset) / boxWidth);
        }

        protected virtual float MeasureBoxWidth()
        {
            // 보이는 요일 수를 나눠 줘야함 (5)
            return (float)(Width - hourBarOffset) / columnNum;
        }
        #endregion

        public void OnDown(float x, float y)
        {
            if (!isScroll)
            {
                orientation = ScrollOrientation.None;
                isScroll = true;

                beginPosition = new SKPoint(x, y);
                beginHorizontalOffset = this.HorizontalOffset;
                beginVerticalOffset = this.VerticalOffset;
            }
        }

        public void OnUp(float x, float y)
        {
            isScroll = false;
            orientation = ScrollOrientation.None;
        }

        public void OnMove(float x, float y)
        {
            if (isScroll)
            {
                var delta = new SKPoint(x, y) - beginPosition;

                switch (orientation)
                {
                    case ScrollOrientation.None:
                        if (Math.Abs(delta.X) >= ScrollThreshold)
                        {
                            orientation = ScrollOrientation.Horizontal;
                        }
                        else if (Math.Abs(delta.Y) >= ScrollThreshold && VirtualHeight > 0)
                        {
                            orientation = ScrollOrientation.Vertical;
                        }
                        break;

                    case ScrollOrientation.Vertical:
                        double maxVerticalOffset = VirtualHeight - this.Height;

                        VerticalOffset = (float)Math.Min(Math.Max(beginVerticalOffset - delta.Y / Density, 0), maxVerticalOffset);
                        break;

                    case ScrollOrientation.Horizontal:
                        HorizontalOffset = beginHorizontalOffset - delta.X / Density;
                        break;
                }
            }
        }
    }
}
