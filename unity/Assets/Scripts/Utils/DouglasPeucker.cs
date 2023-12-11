using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DouglasPeucker
{
    private class Segment
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Perpendicular { get; set; }
        public double Distance { get; set; }
    }

    private static double GetDistance(Vector2 start, Vector2 end, Vector2 point)
    {
        float x = end.x - start.x;
        float y = end.y - start.y;

        float m = x * x + y * y;

        float u = ((point.x - start.x) * x + (point.y - start.y) * y) / m;

        if (u < 0)
        {
            x = start.x;
            y = start.y;
        }
        else if (u > 1)
        {
            x = end.x;
            y = end.y;
        }
        else
        {
            x = start.x + u * x;
            y = start.y + u * y;
        }

        x = point.x - x;
        y = point.y - y;

        return Math.Sqrt(x * x + y * y);
    }

    private static Segment CreateSegment(int start, int end, List<Vector2> points)
    {
        var count = end - start;

        if (count >= 2) // Adjusted from 3 to 2 for minimum segment size.
        {
            var first = points[start];
            var last = points[end];

            var max = points.GetRange(start + 1, count - 1)
                .Select((point, index) => new
                {
                    Index = start + 1 + index,
                    Distance = GetDistance(first, last, point)
                }).OrderByDescending(p => p.Distance).First();

            return new Segment
            {
                Start = start,
                End = end,
                Perpendicular = max.Index,
                Distance = max.Distance
            };
        }

        return new Segment
        {
            Start = start,
            End = end,
            Perpendicular = -1
        };
    }

    private static IEnumerable<Segment> SplitSegment(Segment segment, List<Vector2> points)
    {
        return new[]
        {
            CreateSegment(segment.Start, segment.Perpendicular, points),
            CreateSegment(segment.Perpendicular, segment.End, points)
        };
    }

    private static IEnumerable<Segment> GetSegments(List<Vector2> points)
    {
        yield return CreateSegment(0, points.Count - 1, points);
    }

    private static void Reduce(ref List<Segment> segments, List<Vector2> points, int max, double tolerance)
    {
        while (segments.Count < max)
        {
            var current = segments.OrderByDescending(s => s.Distance).First();

            if (current.Distance <= tolerance)
            {
                break;
            }

            segments.Remove(current);

            var split = SplitSegment(current, points);

            segments.AddRange(split);
        }
    }

    public static List<Vector3> Simplify(List<Vector3> vertices, int max, double tolerance)
    {
        var points = vertices.Select(v => new Vector2(v.x, v.y)).ToList();
        var segments = GetSegments(points).ToList();

        Reduce(ref segments, points, max, tolerance);

        return segments.OrderBy(s => s.Start)
                       .SelectMany((s, i) => new[] { vertices[s.Start], vertices[s.End] })
                       .Distinct()
                       .ToList();
    }
}
