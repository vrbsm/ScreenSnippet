﻿//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paragon.Plugins.ScreenCapture
{
    /// <summary>
    /// Interaction logic for SnippingWindow.xaml
    /// </summary>
    public partial class SnippingWindow : Window
    {
        private readonly DrawingAttributes[] highlightColors;
        private readonly DrawingAttributes[] penColors;
        private DrawingAttributes selectedHighlightColor;
        private DrawingAttributes selectedPenColor;
        private string outputFilename;

        // outputFileName: write output screen capture to given file name  
        public SnippingWindow(string outputFilename)
        {
            InitializeComponent();

            this.outputFilename = outputFilename;

            penColors = ((DrawingAttributes[]) FindResource("PenColors"));
            highlightColors = ((DrawingAttributes[]) FindResource("HighlightColors"));

            selectedPenColor = penColors.First();
            selectedHighlightColor = highlightColors.First();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var result = SnippingTool.TakeSnippet();

            if (result == null)
            {
                Close();
                return;
            }

            WindowState = WindowState.Normal;

            var rect = result.SelectedRectangle;
            // reposition the window so there's a neat effect of showing 
            // the screenshot edit window in place of the selected region
            
            var top = rect.Top - 80 + SystemInformation.VirtualScreen.Top;
            var left = rect.Left - 80 + SystemInformation.VirtualScreen.Left;
            rect.Y = rect.Top - 80 + SystemInformation.VirtualScreen.Top;
            rect.X = rect.Left - 80 + SystemInformation.VirtualScreen.Left;

            var screen = Screen.GetBounds(rect);
            Matrix m = System.Windows.PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dx = m.M11;
            double dy = m.M22;

            Top = top < screen.Top ? (screen.Top / dy) : (top / dy);
            Left = left < screen.Left ? (screen.Left / dx) : (left / dx);

            var image = result.Image;

            ImageBrush.ImageSource = result.ImageToBitmapSource();

            var width = 400;
            // adjust window size to be slightly larger than 
            // the image so nothing is cropped
            if (image.Width > width && image.Width > Width)
            {
                width = image.Width + 100;
            }

            var height = 300;

            if (image.Height > height && image.Height > Height)
            {
                height = image.Height + 100;
            }

            Width = width;
            Height = height;

            // adjust canvas that is hosting the image to match the image size
            InkCanvas.Width = image.Width;
            InkCanvas.Height = image.Height;
        }

        private void OnColorClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (System.Windows.Controls.MenuItem) sender;
            var color = (DrawingAttributes) menuItem.Header;

            InkCanvas.DefaultDrawingAttributes = color;

            if (penColors.Contains(color))
            {
                selectedPenColor = color;
            }
            else
            {
                selectedHighlightColor = color;
            }
        }

        private void OnPenClick(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            InkCanvas.DefaultDrawingAttributes = selectedPenColor;

            ShowContextMenu(sender as ToggleButton, PenContextMenu);
        }

        private void OnHighlightClick(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            InkCanvas.DefaultDrawingAttributes = selectedHighlightColor;

            ShowContextMenu(sender as ToggleButton, HighlightContextMenu);
        }

        private void OnEraseClick(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        private void OnDoneClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var renderTargetBitmap = new RenderTargetBitmap(
                    (int)InkCanvas.Width,
                    (int)InkCanvas.Height,
                    96d,
                    96d,
                    PixelFormats.Default);

                renderTargetBitmap.Render(InkCanvas);

                using (var fileStream = new FileStream(outputFilename, FileMode.Create))
                {
                    var jpegEncoder = new JpegBitmapEncoder();
                    jpegEncoder.QualityLevel = 70;
                    jpegEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    jpegEncoder.Save(fileStream);
                }
            }
            finally
            {
                Close();
            }

        }

        private void ShowContextMenu(ToggleButton sender, System.Windows.Controls.ContextMenu contextMenu)
        {
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = sender;
            contextMenu.IsOpen = true;
        }
    }
}