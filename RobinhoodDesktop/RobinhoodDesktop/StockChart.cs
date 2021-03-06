﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPlot;
using System.Drawing;

namespace RobinhoodDesktop
{
    public class StockChart : StockChartBasic
    {
        public StockChart(string symbol) : base(symbol) 
        {
            // Set up the axes
            stockPricePlot.XAxis1.TicksCrossAxis = false;
            stockPricePlot.XAxis1.TickTextNextToAxis = true;
            stockPricePlot.XAxis1.SmallTickSize = 0;
            stockPricePlot.XAxis1.LargeTickSize = 0;
            //stockPricePlot.XAxis1.NumberFormat = " h";
            stockPricePlot.XAxis1.TickTextColor = openLine.Pen.Color;
            stockPricePlot.XAxis1.TickTextFont = new System.Drawing.Font("monospace", 8.0f, System.Drawing.FontStyle.Bold);
            stockPricePlot.XAxis1.AxisColor = System.Drawing.Color.Transparent;
            stockPricePlot.XAxis1.TicksLabelAngle = (float)0;
            stockPricePlot.XAxis1.HideTickText = false;
            stockPricePlot.YAxis1.TickTextNextToAxis = true;
            stockPricePlot.YAxis1.TicksIndependentOfPhysicalExtent = true;
            stockPricePlot.YAxis1.TickTextColor = openLine.Pen.Color;
            stockPricePlot.YAxis1.HideTickText = false;
            this.MarginMax = 1.04;
            this.MarginMin = 0.975;


            // Create the interaction for the chart
            stockPricePlot.AddInteraction(new PlotDrag(true, false));
            stockPricePlot.AddInteraction(new AxisDrag());
            stockPricePlot.AddInteraction(new HoverInteraction(this));

            // Create the text controls
            priceText = new Label();
            priceText.Location = new System.Drawing.Point((stockPricePlot.Canvas.Width - 100) / 2, 10);
            priceText.Font = new System.Drawing.Font(GuiStyle.FONT_NAME, 12.0f, System.Drawing.FontStyle.Regular);
            priceText.ForeColor = GuiStyle.TEXT_COLOR;
            priceText.BackColor = System.Drawing.Color.Transparent;
            priceText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            stockPricePlot.Canvas.Controls.Add(priceText);
            priceText.BringToFront();
            changeText = new Label();
            changeText.Location = new Point(priceText.Location.X - 80, priceText.Location.Y + 25);
            changeText.Size = new Size(260, changeText.Size.Height);
            changeText.Font = new System.Drawing.Font(GuiStyle.FONT_NAME, 9.0f, System.Drawing.FontStyle.Regular);
            changeText.ForeColor = GuiStyle.TEXT_COLOR;
            changeText.BackColor = System.Drawing.Color.Transparent;
            changeText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            stockPricePlot.Canvas.Controls.Add(changeText);
            changeText.BringToFront();

            stockPricePlot.Refresh();
        }

        /// <summary>
        /// Sets the data for the chart to use
        /// </summary>
        protected override void UpdateChartData()
        {
            // Update the base class
            base.UpdateChartData();

            // Update the price text
            UpdatePriceText((DateTime)Source.Rows[Source.Rows.Count - 1][TIME_DATA_TAG]);

            // Execute the callback action
            Updated();
        }

        #region Constants
        #endregion

        #region Types
        private class HoverInteraction : NPlot.Interaction
        {
            public HoverInteraction(StockChart chart)
            {
                this.Chart = chart;
                Lines = new LineDrawer();
                //this.Chart.stockPricePlot.Add(Lines);
                //Lines.Canvas.Size = Chart.stockPricePlot.Canvas.Size;
                Lines.Canvas.Image = new System.Drawing.Bitmap(Chart.stockPricePlot.Canvas.Size.Width, Chart.stockPricePlot.Canvas.Size.Height);
                Lines.Canvas.BackColor = System.Drawing.Color.Transparent;
                Lines.Canvas.Size = Chart.stockPricePlot.Canvas.Size;
                Lines.Canvas.Enabled = false;
                this.Chart.stockPricePlot.Canvas.Controls.Add(Lines.Canvas);

                this.Chart.stockPricePlot.Canvas.Resize += (object sender, System.EventArgs e) => 
                {
                    int width = Chart.stockPricePlot.Canvas.Size.Width;
                    int height = Chart.stockPricePlot.Canvas.Size.Height;
                    Lines.Canvas.Image = new System.Drawing.Bitmap(width, height);
                    Lines.Canvas.Size = Chart.stockPricePlot.Canvas.Size;
                    Chart.priceText.Location = new Point(((width / 2) - (Chart.priceText.Width / 2)), Chart.priceText.Location.Y);
                    Chart.changeText.Location = new Point(((width / 2) - (Chart.changeText.Width / 2)), Chart.changeText.Location.Y);
                };
            }

            private class LineDrawer
            {
                /// <summary>
                /// The pen used to draw the time line
                /// </summary>
                public System.Drawing.Pen TimePen = new System.Drawing.Pen(GuiStyle.PRICE_COLOR_POSITIVE, 2.0f);

                /// <summary>
                /// The pen used to draw the price lines
                /// </summary>
                public System.Drawing.Pen PricePen = new System.Drawing.Pen(GuiStyle.GUIDE_COLOR, 1.5f);

                /// <summary>
                /// The canvas used to draw additional overlay lines
                /// </summary>
                public System.Windows.Forms.PictureBox Canvas = new PictureBox();
            }

            /// <summary>
            /// The chart the interaction should update
            /// </summary>
            public StockChart Chart;

            /// <summary>
            /// The percentage from the current price at which the min and max guidelines should be drawn
            /// </summary>
            public float GuideLinePercentage = 1.025f;

            /// <summary>
            /// Indicates if the mouse is currently hovering over the chart
            /// </summary>
            public bool Hovering = false;

            /// <summary>
            /// Draws lines on top of the chart
            /// </summary>
            private LineDrawer Lines;

            /// <summary>
            /// Handles the mouse enter event
            /// </summary>
            /// <param name="ps">The plot surface</param>
            /// <returns>false</returns>
            public override bool DoMouseEnter(InteractivePlotSurface2D ps)
            {
                Hovering = true;
                Lines.Canvas.Visible = true;
                return false;
            }

            /// <summary>
            /// Handles the mouse leave event
            /// </summary>
            /// <param name="ps">The plot surface</param>
            /// <returns>false</returns>
            public override bool DoMouseLeave(InteractivePlotSurface2D ps)
            {
                Hovering = false;
                Lines.Canvas.Visible = false;
                if(Chart.Source != null)
                {
                    Chart.UpdatePriceText((DateTime)Chart.Source.Rows[Chart.Source.Rows.Count - 1][TIME_DATA_TAG]);
                }
                return false;
            }

            /// <summary>
            /// Handles a move move event
            /// </summary>
            /// <param name="X">The X mouse coordinate</param>
            /// <param name="Y">The Y mouse coordinate</param>
            /// <param name="keys">The mouse buttons that are pressed</param>
            /// <param name="ps">The plot surface the mouse is moving over</param>
            /// <returns></returns>
            public override bool DoMouseMove(int X, int Y, Modifier keys, InteractivePlotSurface2D ps)
            {
                DateTime time = new DateTime((long)Chart.stockPricePlot.PhysicalXAxis1Cache.PhysicalToWorld(new System.Drawing.Point(X, Y), false));
                int idx = Chart.GetTimeIndex(time);
                if(idx >= 0)
                {
                    float price = (float)Chart.Source.Rows[idx]["Price"];
                    Chart.UpdatePriceText(time);
                    using(System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(Lines.Canvas.Image))
                    {
                        PhysicalAxis xAxis = Chart.stockPricePlot.PhysicalXAxis1Cache;
                        PhysicalAxis yAxis = Chart.stockPricePlot.PhysicalYAxis1Cache;
                        g.Clear(System.Drawing.Color.Transparent);

                        // Draw the time line
                        System.Drawing.PointF timePoint = xAxis.WorldToPhysical(time.Ticks, true);
                        g.DrawLine(Lines.TimePen, timePoint.X, yAxis.PhysicalMin.Y, timePoint.X, yAxis.PhysicalMax.Y);

                        // Draw the guide lines
                        System.Drawing.PointF minPoint = yAxis.WorldToPhysical(price / GuideLinePercentage, true);
                        System.Drawing.PointF maxPoint = yAxis.WorldToPhysical(price * GuideLinePercentage, true);
                        g.DrawLine(Lines.PricePen, xAxis.PhysicalMin.X, minPoint.Y, xAxis.PhysicalMax.X, minPoint.Y);
                        g.DrawLine(Lines.PricePen, xAxis.PhysicalMin.X, maxPoint.Y, xAxis.PhysicalMax.X, maxPoint.Y);
                    }

                    // Use this as a hook to update the minimum and maximum displayed prices
                    Chart.UpdatePriceMinMax();

                    // Refresh the canvas to display the updated lines
                    Chart.stockPricePlot.Canvas.Refresh();
                }

                Chart.RefreshSourceData();

                return false;
            }

            /// <summary>
            /// Handles a mouse scroll wheel event
            /// </summary>
            /// <param name="X">The X coordinate of the mouse</param>
            /// <param name="Y">The Y coordinate of the mouse</param>
            /// <param name="direction">The mouse wheel movement</param>
            /// <param name="keys">The mouse buttons that are pressed</param>
            /// <param name="ps">The plot surface the mouse is scrolling over</param>
            /// <returns></returns>
            public override bool DoMouseScroll(int X, int Y, int direction, Modifier keys, InteractivePlotSurface2D ps)
            {
                if(Hovering && (Chart.Source != null))
                {
                    double percentChange = ((direction > 0) ? (1 / 1.2) : (1.2));
                    DateTime anchor = new DateTime((long)Chart.stockPricePlot.PhysicalXAxis1Cache.PhysicalToWorld(new System.Drawing.Point(X, Y), false));

                    double ratio = ((double)X / Chart.stockPricePlot.PhysicalXAxis1Cache.PhysicalLength);
                    if((direction < 0) && (Chart.stockPricePlot.XAxis1.WorldMax > ((DateTime)Chart.Source.Rows[Chart.Source.Rows.Count - 1][TIME_DATA_TAG]).Ticks))
                    {
                        ratio = 1.0;
                    }
                    Chart.stockPricePlot.XAxis1.IncreaseRange(percentChange - 1.0, ratio);

                    Chart.UpdatePriceMinMax();
                    Chart.RefreshSourceData();
                    Chart.stockPricePlot.Refresh();
                }

                return false;
            }
        }
#endregion

#region Variables
        /// <summary>
        /// Callback function that is executed when the chart receives updated data
        /// </summary>
        public Action Updated;

        /// <summary>
        /// The text item used to display the price
        /// </summary>
        private Label priceText;

        /// <summary>
        /// The text item used to display the delta in the stock price, and the time
        /// </summary>
        private Label changeText;

        /// <summary>
        /// Mutex used to syncrhonize access for data requests
        /// </summary>
        private System.Threading.Semaphore DataRequestMutex = new System.Threading.Semaphore(1, 1);

        /// <summary>
        /// Stores the minimum data range that has been requested
        /// </summary>
        private DateTime RequestedDateMin = DateTime.MaxValue;
#endregion

#region Properties
#endregion

#region Utility Functions
        /// <summary>
        /// Updates the text describing the price at the given time
        /// </summary>
        /// <param name="time">The time to describe</param>
        private void UpdatePriceText(DateTime time)
        {
            int idx = GetTimeIndex(time);
            if((idx >= 0) && (DailyData != null))
            {
                float price = (float)Source.Rows[idx]["Price"];
                priceText.Text = String.Format("{0:c}", price);
                float basePrice = (float)DailyData.Rows[GetTimeIndex(time, DailyData)][PRICE_DATA_TAG];
                float percentChange = -1.0f + (price / basePrice);
                changeText.Text = String.Format("{0}{1:c}({0}{2:P2}) {3:t} {3:MMM d} '{3:yy}", ((percentChange >= 0) ? "+" : ""), (price - basePrice), percentChange, time);
            }
        }

        /// <summary>
        /// Checks the source data to ensure enough is available to cover the intended range
        /// </summary>
        private void RefreshSourceData()
        {
            if((Source != null) && ((long)stockPricePlot.XAxis1.WorldMin != 0))
            {
                DateTime chartMin = new DateTime((long)stockPricePlot.XAxis1.WorldMin);
                DateTime dataMin = (DateTime)Source.Rows[0][TIME_DATA_TAG];
                DateTime dataMax = (DateTime)Source.Rows[Source.Rows.Count - 1][TIME_DATA_TAG];
                DateTime desiredDataMin = chartMin.AddHours(-((dataMax - chartMin).TotalHours * 2));
                if((dataMin.Date > desiredDataMin.Date) && (desiredDataMin < RequestedDateMin))
                {
                    RequestedDateMin = desiredDataMin;
                    RequestData(desiredDataMin, dataMax, TimeSpan.FromMinutes(1));
                }
            }
        }
#endregion
    }
}
