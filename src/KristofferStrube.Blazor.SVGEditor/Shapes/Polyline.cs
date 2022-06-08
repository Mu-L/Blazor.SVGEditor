﻿using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor;

public class Polyline : Shape
{
    public Polyline(IElement element, SVG svg) : base(element, svg)
    {
        Points = Element.GetAttributeOrEmpty("points").ToPoints();
    }

    public override Type Editor => typeof(PolylineEditor);

    public List<(double x, double y)> Points { get; set; }

    public override IEnumerable<(double x, double y)> SelectionPoints => Points;

    private void UpdatePoints()
    {
        Element.SetAttribute("points", PointsToString(Points));
        Changed.Invoke(this);
    }

    public static string PointsToString(List<(double x, double y)> points)
    {
        return string.Join(" ", points.Select(point => $"{point.x.AsString()},{point.y.AsString()}"));
    }

    public override void HandleMouseMove(MouseEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.MoveAnchor:
                if (SVG.CurrentAnchor == null)
                {
                    SVG.CurrentAnchor = 0;
                }
                Points[(int)SVG.CurrentAnchor] = (x, y);
                UpdatePoints();
                break;
            case EditMode.Move:
                (double x, double y) diff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                Points = Points.Select(point => { point.x += diff.x; point.y += diff.y; return point; }).ToList();
                UpdatePoints();
                break;
            case EditMode.Add:
                if (Points.Count == 0)
                {
                    (double x, double y) startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
                    Points.Add((startPos.x, startPos.y));
                    Points.Add((x, y));
                }
                Points[^1] = (x, y);
                UpdatePoints();
                break;
        }
    }

    public override void HandleMouseUp(MouseEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.MoveAnchor:
                SVG.CurrentAnchor = null;
                SVG.EditMode = EditMode.None;
                if (Points.Count == 0)
                {
                    SVG.RemoveElement(this);
                    SVG.SelectedShapes.Clear();
                    Changed.Invoke(this);
                }
                break;
            case EditMode.Move:
                SVG.EditMode = EditMode.None;
                break;
            case EditMode.Add:
                Points.Add((x, y));
                break;
        }
    }

    public override void HandleMouseOut(MouseEventArgs eventArgs)
    {
    }

    public static void AddNew(SVG SVG)
    {
        IElement element = SVG.Document.CreateElement("POLYLINE");

        Polyline polyline = new(element, SVG)
        {
            Changed = SVG.UpdateInput,
            Stroke = "black",
            StrokeWidth = "3",
            Fill = "none"
        };
        SVG.EditMode = EditMode.Add;

        SVG.SelectedShapes.Clear();
        SVG.SelectedShapes.Add(polyline);
        SVG.AddElement(polyline);
    }

    public override void Complete()
    {
        Points.RemoveAt(Points.Count - 1);
        UpdatePoints();
    }
}