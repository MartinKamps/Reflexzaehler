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
                // Horizontal base line
                cpb.BeginFigure(new Vector2( 1, height));
                cpb.AddLine(new Vector2(width, height));
                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Black, 2);
            }

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                for (int n = 1; n < 10; n++)
                {   // Horizontal seperator line
                    cpb.BeginFigure(new Vector2(1, (float)(0.1 * n * height)));
                    cpb.AddLine(new Vector2(width, (float)(0.1 * n * height)));
                    cpb.EndFigure(CanvasFigureLoop.Open);
                }
                
                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.LightGray, 1);
            }

            args.DrawingSession.DrawText("480", width - 50, height - 40, Colors.Black);

            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                // Vertical base line
                cpb.BeginFigure(new Vector2(1, height));
                cpb.AddLine(new Vector2(1, 1));
                cpb.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawGeometry(CanvasGeometry.CreatePath(cpb), Colors.Black, 2);
            }

            args.DrawingSession.DrawText("Reflexion: 100p", 5, 5, Colors.DarkOrange);
            args.DrawingSession.DrawText("Sekunden pro Umdrehung - 300s", 5, 25, Colors.DarkGreen);
        }

        public void RenderData(CanvasControl canvas, CanvasDrawEventArgs args, Color color, float thickness, List<double> data, bool renderArea, int steps, int factor)
        {
            using (var cpb = new CanvasPathBuilder(args.DrawingSession))
            {
                cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * ( 1 - (data[0] / factor)))));

                for (int i = 1; i < data.Count; i++)
                {
                    cpb.AddLine(new Vector2(i * steps, (float)(canvas.ActualHeight * ( 1 - (data[i] / factor))) + 2 ));
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
