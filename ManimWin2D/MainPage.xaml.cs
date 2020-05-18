using ManimLib.Mobject.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.Geometry;
using MathNet.Numerics.LinearAlgebra;
using ManimLib.Mobject.Svg;
using ManimLib.Mobject;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ManimWin2D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".svg");
            openPicker.FileTypeFilter.Add(".xdv");

            file = await openPicker.PickSingleFileAsync();
            RenderingCanvas.Invalidate();
        }

        private StorageFile file;

        private void CanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (file == null)
                return;
            // Application now has read/write access to the picked file

            var rect = new Rectangle();
            var pathBuilder = new CanvasPathBuilder(args.DrawingSession);
            foreach (Vector<double> point in rect.Points)
            {
                pathBuilder.AddLine((float)point[0], (float)point[1]);
            }
            args.DrawingSession.DrawGeometry(
                CanvasGeometry.CreatePath(pathBuilder),
                new System.Numerics.Vector2((float)rect.Points[0][1], (float)rect.Points[0][1]),
                Colors.White
            );

            //var svg = new SvgMobject(svgText: FileIO.ReadTextAsync(file).GetResults());
            //svg.GeneratePoints();

            //foreach (VMobject vmobj in svg.Submobjects)
            //{
            //    var pathBuilder = new CanvasPathBuilder(args.DrawingSession);
            //    foreach (Vector<double> point in vmobj.Points)
            //    {
            //        pathBuilder.AddLine((float)point[0], (float)point[1]);
            //    }
            //    args.DrawingSession.DrawGeometry(
            //        CanvasGeometry.CreatePath(pathBuilder),
            //        new System.Numerics.Vector2((float)vmobj.Points[0][1], (float)vmobj.Points[0][1]),
            //        Colors.White
            //    );
            //}
        }
    }
}
