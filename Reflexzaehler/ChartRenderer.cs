using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Reflexzaehler
{
    class ChartRenderer
    {
        public void RenderAxes(CanvasControl canvas, CanvasDrawEventArgs args)
        {
            var width = (float)canvas.ActualWidth;
            var height = (float)(canvas.ActualHeight);
            
            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                // Horizontal line
                cpb.BeginFigure(new Vector2( 1, height));
                cpb.AddLine(new Vector2(width, height));
                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Black, 2);
            }

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.1 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.1 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.2 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.2 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);
                
                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.3 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.3 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.4 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.4 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.5 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.5 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.6 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.6 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.7 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.7 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.8 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.8 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(0.9 * height)));
                cpb.AddLine(new Vector2(width, (float)(0.9 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                // Horizontal line
                cpb.BeginFigure(new Vector2(1, (float)(1 * height)));
                cpb.AddLine(new Vector2(width, (float)(1 * height)));
                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.LightGray, 1);
            }

            args.DrawingSession.DrawText("480", width - 50, height - 40, Colors.Black);

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                // Vertical line
                cpb.BeginFigure(new Vector2(1, height));
                cpb.AddLine(new Vector2(1, 1));
                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Black, 2);
            }

            args.DrawingSession.DrawText("100", 5, 5, Colors.Black);
        }

        public void RenderData(CanvasControl canvas, CanvasDrawEventArgs args, Color color, float thickness, List<double> data, bool renderArea)
        {
            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * ( 1 - (data[0] / 100)))));

                for (int i = 1; i < data.Count; i++)
                {
                    cpb.AddLine(new Vector2(i * 4, (float)(canvas.ActualHeight * ( 1 - (data[i] / 100)))));
                }

                if (renderArea)
                {
                    cpb.AddLine(new Vector2(data.Count, (float)canvas.ActualHeight));
                    cpb.AddLine(new Vector2(0, (float)canvas.ActualHeight));
                    cpb.EndFigure(CanvasFigureLoop.Closed);
                    args.DrawingSession.FillGeometry(CanvasGeometry.CreatePath(cpb), Colors.LightGreen);
                }
                else
                {
                    cpb.EndFigure(CanvasFigureLoop.Open);
                    args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
                }
            }
        }
        
        public void RenderMovingAverage(CanvasControl canvas, CanvasDrawEventArgs args, Color color, float thickness, int movingAverageRange, List<double> data)
        {
            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * (1 - data[0]))));

                double total = data[0];

                int previousRangeLeft = 0;
                int previousRangeRight = 0;

                for (int i = 1; i < data.Count; i++)
                {
                    var range = Math.Max(0, Math.Min(movingAverageRange / 2, Math.Min(i, data.Count - 1 - i)));
                    int rangeLeft = i - range;
                    int rangeRight = i + range;

                    for (int j = previousRangeLeft; j < rangeLeft; j++)
                    {
                        total -= data[j];
                    }

                    for (int j = previousRangeRight + 1; j <= rangeRight; j++)
                    {
                        total += data[j];
                    }

                    previousRangeLeft = rangeLeft;
                    previousRangeRight = rangeRight;

                    cpb.AddLine(new Vector2(i, (float)(canvas.ActualHeight * (1 - total / (range * 2 + 1)))));
                }

                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), color, thickness);
            }
        }
    }
}
