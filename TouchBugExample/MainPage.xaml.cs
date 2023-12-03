using System.Diagnostics;
using Microsoft.Maui.Layouts;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Maui.Skia;

namespace TouchBugExample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var plot1 = new PlotView();
        var plot2 = new PlotView();
        
        InitPlotView(plot1, OxyColors.Aqua, "a1");
        InitPlotView(plot2, OxyColors.Green, "a2");
        
        this.AbsoluteLayout.SetLayoutFlags(plot1, AbsoluteLayoutFlags.All);
        this.AbsoluteLayout.SetLayoutBounds(plot1, new Rect(0, 0, 1, 1));
        this.AbsoluteLayout.Add(plot1);
        
        this.AbsoluteLayout.SetLayoutFlags(plot2, AbsoluteLayoutFlags.All);
        this.AbsoluteLayout.SetLayoutBounds(plot2, new Rect(0, 0, 1, 1));
        this.AbsoluteLayout.Add(plot2);

        // if both have opacity 1 there is no crash.
        // if plot 2 has opacity 0, there is a crash.
        plot1.Opacity = 1;
        plot2.Opacity = 0;
    }

    private void InitPlotView(PlotView plotView, OxyColor color, string annotationName)
    {
        plotView.Model = new PlotModel();
        plotView.Model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Minimum = 0,
            Maximum = 1,
            IsPanEnabled = false,
            IsZoomEnabled = false
        });
        plotView.Model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Minimum = 0,
            Maximum = 1,
            IsPanEnabled = false,
            IsZoomEnabled = false
        });

        plotView.Model.Background = color;

        var _ = new MyAnnotation(plotView.Model, annotationName);
        plotView.Model.InvalidatePlot(false);
    }
}

public class MyAnnotation : Annotation
{

    private ScreenPoint? _position;

    private ScreenPoint? Position
    {
        get
        {
            if (_position is null)
            {
                _position = PlotModel?.PlotArea.Center;
            }
            return _position;
        }
        set => _position = value;
    }

    public MyAnnotation(PlotModel model, string name)
    {
        
        TouchStarted += (_, args) =>
        {
            Debug.WriteLine($"Started {name}");
            args.Handled = true;
        };
        TouchDelta += (_, args) =>
        {
            Debug.WriteLine($"Dragging {name}"); 
            HandleDrag(args);
        };
        TouchCompleted += (_, args) =>
        {
            Debug.WriteLine($"Completed {name}");
            args.Handled = true;
        };
        
        model.Annotations.Add(this);
        EnsureAxes();
    }

    public override void Render(IRenderContext rc)
    {
        if (Position.HasValue)
        {
            rc.DrawCircle(Position.Value, 20, OxyColors.Blue, OxyColors.Blue, 0, EdgeRenderingMode.Automatic);

        }
        base.Render(rc);
    }

    protected override HitTestResult HitTestOverride(HitTestArguments args)
    {
        if (Position is null) return new HitTestResult(this, ScreenPoint.Undefined);
        return args.Point.DistanceTo(Position.Value) <= 20 * 2 ? new HitTestResult(this, args.Point) : null;
    }

    private void HandleDrag(OxyInputEventArgs e)
    {
        var pos = e switch
        {
            OxyTouchEventArgs evt => evt.Position,
            OxyMouseEventArgs evt => evt.Position,
            _ => ScreenPoint.Undefined
        };
        _position = new ScreenPoint(pos.X, pos.Y);
        PlotModel.InvalidatePlot(true);
        e.Handled = true;
    }
}