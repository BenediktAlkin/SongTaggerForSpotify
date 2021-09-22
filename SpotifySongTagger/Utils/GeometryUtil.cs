using System;
using System.Windows;
using System.Windows.Media;

namespace SpotifySongTagger.Utils
{
    public static class GeometryUtil
    {
        private static Point[] GetAnchors(Rect rect)
        {
            return new[]
            {
                new Point(rect.X, rect.Y + rect.Height / 2), // left
                new Point(rect.X + rect.Width / 2, rect.Y), // top
                new Point(rect.X + rect.Width, rect.Y + rect.Height / 2), // right
                new Point(rect.X + rect.Width / 2, rect.Y + rect.Height), // top
            };
        }

        public static (Point, Point) GetShortestPathBetweenRectangles(Rect r1, Rect r2)
        {
            var anchors1 = GetAnchors(r1);
            var anchors2 = GetAnchors(r2);

            var bestDistance = double.PositiveInfinity;
            Point p1, p2;
            foreach (var anchor1 in anchors1)
            {
                foreach (var anchor2 in anchors2)
                {
                    var distance = (anchor2 - anchor1).Length;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        p1 = anchor1;
                        p2 = anchor2;
                    }
                }
            }

            return (p1, p2);
        }

        public static GeometryGroup GetArrow(Point start, Point end)
        {
            var lineGroup = new GeometryGroup();
            double theta = Math.Atan2((end.Y - start.Y), (end.X - start.X)) * 180 / Math.PI;

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure { IsFilled = true };
            var p = end; // point where the arrow is drawn
            pathFigure.StartPoint = p;

            var lpoint = new Point(p.X + 6, p.Y + 15);
            var rpoint = new Point(p.X - 6, p.Y + 15);
            var seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            var seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            var seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            var transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            var connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = start;
            connectorGeometry.EndPoint = end;
            lineGroup.Children.Add(connectorGeometry);

            return lineGroup;
        }
    }
}
