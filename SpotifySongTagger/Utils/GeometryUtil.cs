using Serilog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SpotifySongTagger.Utils
{
    public static class GeometryUtil
    {
        public enum AnchorLocation
        {
            // integers are used for rotation of arrow
            Bottom = 0, Left = 1, Top = 2, Right = 3, 
        }
        public record Anchor(AnchorLocation Location, Point Point);
        public static AnchorLocation OppositeLocation(AnchorLocation loc) => loc switch
        {
            AnchorLocation.Bottom => AnchorLocation.Top,
            AnchorLocation.Top => AnchorLocation.Bottom,
            AnchorLocation.Left => AnchorLocation.Right,
            AnchorLocation.Right => AnchorLocation.Left,
            _ => AnchorLocation.Bottom,
        };

        private static Anchor[] GetAnchors(Rect rect)
        {
            return new Anchor[]
            {
                new(AnchorLocation.Left, new Point(rect.X, rect.Y + rect.Height / 2)),
                new(AnchorLocation.Top, new Point(rect.X + rect.Width / 2, rect.Y)),
                new(AnchorLocation.Right, new Point(rect.X + rect.Width, rect.Y + rect.Height / 2)),
                new(AnchorLocation.Bottom, new Point(rect.X + rect.Width / 2, rect.Y + rect.Height)),
            };
        }
        public static Anchor GetNearestAnchor(Point point, Rect rect)
        {
            var anchors = GetAnchors(rect);

            Point bestPoint;
            AnchorLocation bestAnchorType = AnchorLocation.Left;
            var bestDistance = double.PositiveInfinity;
            foreach(var (anchorType, anchorPoint) in anchors)
            {
                var distance = (point - anchorPoint).Length;
                if(distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPoint = anchorPoint;
                    bestAnchorType = anchorType;
                }
            }
            return new Anchor(bestAnchorType, bestPoint);
        }

        public static (Anchor, Anchor) GetShortestPathBetweenRectangles(Rect r1, Rect r2)
        {
            var anchors1 = GetAnchors(r1);
            var anchors2 = GetAnchors(r2);

            var bestDistance = double.PositiveInfinity;
            Point p1, p2;
            AnchorLocation t1 = AnchorLocation.Left;
            AnchorLocation t2 = AnchorLocation.Left;
            foreach (var (anchorType1, anchorPoint1) in anchors1)
            {
                foreach (var (anchorType2, anchorPoint2) in anchors2)
                {
                    var distance = (anchorPoint2 - anchorPoint1).Length;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        p1 = anchorPoint1;
                        t1 = anchorType1;
                        p2 = anchorPoint2;
                        t2 = anchorType2;
                    }
                }
            }

            return (new Anchor(t1, p1), new Anchor(t2, p2));
        }

        public static GeometryGroup GetArrow(Anchor startAnchor, Anchor endAnchor)
        {
            var start = startAnchor.Point;
            var end = endAnchor.Point;

            var lineGroup = new GeometryGroup();

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure { IsFilled = true };
            var p = end; // point where the arrow is drawn
            pathFigure.StartPoint = p;

            const int arrowHalfWidth = 6;
            const int arrowHeight = 15;
            var lpoint = new Point(p.X + arrowHalfWidth, p.Y + arrowHeight);
            var rpoint = new Point(p.X - arrowHalfWidth, p.Y + arrowHeight);
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
            // linear connection
            //double theta = Math.Atan2((end.Y - start.Y), (end.X - start.X)) * 180 / Math.PI;
            //transform.Angle = theta + 90;
            transform.Angle = 90 * (int)endAnchor.Location;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            // bezier ends in arrow base
            const double bezierPointFactor = 0.4; // how far away from the start/endpoint to put the bezier points
            var arrowBase = transform.Transform(new Point(end.X, end.Y + arrowHeight));
            Point bezier1, bezier2;
            switch (startAnchor.Location)
            {
                case AnchorLocation.Left:
                case AnchorLocation.Right:
                    bezier1 = new Point(start.X + (arrowBase.X - start.X) * bezierPointFactor, start.Y);
                    break;
                case AnchorLocation.Top:
                case AnchorLocation.Bottom:
                    bezier1 = new Point(start.X, start.Y + (arrowBase.Y - start.Y) * bezierPointFactor);
                    break;
            }
            switch (endAnchor.Location)
            {
                case AnchorLocation.Left:
                case AnchorLocation.Right:
                    bezier2 = new Point(arrowBase.X - (arrowBase.X - start.X) * bezierPointFactor, arrowBase.Y);
                    break;
                case AnchorLocation.Top:
                case AnchorLocation.Bottom:
                    bezier2 = new Point(arrowBase.X, arrowBase.Y - (arrowBase.Y - start.Y) * bezierPointFactor);
                    break;
            }
            var bezier = new BezierSegment(bezier1, bezier2, arrowBase, true);
            var figure = new PathFigure(start, new PathSegmentCollection { bezier }, false) { IsFilled = false };
            var connectorGeometry = new PathGeometry(new PathFigureCollection { figure });

            // linear connection
            //var connectorGeometry = new LineGeometry();
            //connectorGeometry.StartPoint = start;
            //connectorGeometry.EndPoint = end;
            
            lineGroup.Children.Add(connectorGeometry);

            return lineGroup;
        }
    }
}
