using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageViewer
{
    public partial class MainWindow : Window
    {
        private int margin = 20;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        bool imageLoaded = false;
        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png)|*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected image into the Image control
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                ImageViewer.Source = bitmap;

                // Save the original dimensions of the image
                ImageCanvas.Width = bitmap.PixelWidth;
                ImageCanvas.Height = bitmap.PixelHeight;

                imageLoaded = true;
            }
        }

        private void HorizontalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateZoom();
        }

        private void VerticalZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateZoom();
        }


        private void HorizontalPanSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePan();
        }

        private void VerticalPanSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePan();
        }

        private void UpdateCanvasBounds()
        {
            if (ImageViewer.Source is BitmapSource bitmap)
            {
                // Calculate scaled image dimensions
                double scaledWidth = bitmap.PixelWidth * HorizontalZoomSlider.Value;
                double scaledHeight = bitmap.PixelHeight * VerticalZoomSlider.Value;

                // Ensure the canvas accommodates either the scaled image or the viewer size (whichever is larger)
                ImageCanvas.Width = Math.Max(scaledWidth, ViewerBorder.ActualWidth);
                ImageCanvas.Height = Math.Max(scaledHeight, ViewerBorder.ActualHeight);

                // Constrain panning to the bounds of the visible viewer area
                double horizontalMaxPan = Math.Max(0, scaledWidth - ViewerBorder.ActualWidth);
                double verticalMaxPan = Math.Max(0, scaledHeight - ViewerBorder.ActualHeight);

                HorizontalPanSlider.Maximum = horizontalMaxPan;
                VerticalPanSlider.Maximum = verticalMaxPan;

               System.Diagnostics.Debug.WriteLine($"ViewerBorder Size: {ViewerBorder.ActualWidth}x{ViewerBorder.ActualHeight}");
               System.Diagnostics.Debug.WriteLine($"Canvas Size: {ImageCanvas.Width}x{ImageCanvas.Height}");
               System.Diagnostics.Debug.WriteLine($"Scaled Image Size: {bitmap.PixelWidth * HorizontalZoomSlider.Value}x{bitmap.PixelHeight * VerticalZoomSlider.Value}");
            }
        }

        private void AutoScaleCheckbox_Checked1(object sender, RoutedEventArgs e)
        {
            if (imageLoaded)
            {
                // Calculate scaling factor to fit the height of the Border
                double borderHeight = ViewerBorder.ActualHeight;
                //ImageCanvas.Height = ViewerBorder.ActualHeight;

                // Apply clipping region to the canvas
                //ImageCanvas.Clip = new RectangleGeometry(new Rect(0, 0, ViewerBorder.ActualWidth, ViewerBorder.ActualHeight));

                if (ImageViewer.Source is BitmapSource bitmap)
                {
                    // Scale the image proportionally to match the Border's height
                    double scaleFactor = borderHeight / bitmap.PixelHeight;

                    ScaleTransform scaleTransform = new ScaleTransform(1, scaleFactor);
                    ImageViewer.LayoutTransform = scaleTransform;
                    VerticalZoomSlider.IsEnabled = false;
                    VerticalPanSlider.IsEnabled = VerticalZoomSlider.IsEnabled;
                }
            }
        }

        private void AutoScaleCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (imageLoaded)
            {
                // Calculate the height of the Border, accounting for any border thickness
                double availableHeight = ViewerBorder.ActualHeight - ViewerBorder.BorderThickness.Top - ViewerBorder.BorderThickness.Bottom;

                if (ImageViewer.Source is BitmapSource bitmap)
                {
                    // Calculate scale factor to fit the height
                    double scaleFactor = availableHeight / bitmap.PixelHeight;

                    // Apply vertical scaling only
                    ScaleTransform scaleTransform = new ScaleTransform(1, 2*scaleFactor);
                    ImageViewer.LayoutTransform = scaleTransform;

                    // Optionally center the image horizontally within the Canvas
                    double horizontalOffset = 0; // (ImageCanvas.Width - bitmap.PixelWidth * 1) / 2; // 1 = no horizontal scaling
                    Canvas.SetLeft(ImageViewer, horizontalOffset > 0 ? horizontalOffset : 0); // Ensure no negative offsets
                }
            }
        }

        private void AutoScaleCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (imageLoaded)
            {
                // Reset the image scaling to manual zoom levels
                VerticalZoomSlider.IsEnabled = true;
                VerticalPanSlider.IsEnabled = VerticalZoomSlider.IsEnabled;
                UpdateZoom();
            }
        }

        private void UpdatePan()
        {
            if (!imageLoaded)
                return;

            double horizontalOffset = HorizontalPanSlider.Value;
            double verticalOffset = VerticalPanSlider.Value;

            Canvas.SetLeft(ImageViewer, -horizontalOffset);
            Canvas.SetTop(ImageViewer, -verticalOffset);
        }

        private void UpdateZoom()
        {
            if (!imageLoaded)
                return;

            double horizontalScale = HorizontalZoomSlider.Value;
            double verticalScale = VerticalZoomSlider.Value;

            ScaleTransform scaleTransform = new ScaleTransform(horizontalScale, verticalScale);
            ImageViewer.LayoutTransform = scaleTransform;

            UpdateCanvasBounds(); // Update the panning sliders' max values
        }

        private void ViewerBorder_SizeChanged1(object sender, SizeChangedEventArgs e)
        {
            // Sync canvas size with the border size dynamically
            ImageCanvas.Width = ViewerBorder.ActualWidth;
            ImageCanvas.Height = ViewerBorder.ActualHeight;

            UpdateCanvasBounds(); // Recalculate pan limits
        }

        private void ViewerBorder_SizeChanged2(object sender, SizeChangedEventArgs e)
        {
            // Update canvas size to match the border dimensions
            ImageCanvas.Width = ViewerBorder.ActualWidth;
            ImageCanvas.Height = ViewerBorder.ActualHeight;

            // Apply clipping region to the canvas
            ImageCanvas.Clip = new RectangleGeometry(new Rect(0, 0, ViewerBorder.ActualWidth, ViewerBorder.ActualHeight));

            UpdateCanvasBounds(); // Recalculate pan and zoom limits
        }

        private void ViewerBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Handle dynamic resizing when auto-scaling is enabled
            if (AutoScaleCheckbox.IsChecked == true)
            {
                ImageCanvas.Width = ViewerBorder.ActualWidth;
                ImageCanvas.Height = ViewerBorder.ActualHeight;
                ImageCanvas.Clip = new RectangleGeometry(new Rect(0, 0, ViewerBorder.ActualWidth, ViewerBorder.ActualHeight));
                AutoScaleCheckbox_Checked(null, null);

            }
            else
            {
                ImageCanvas.Width = ViewerBorder.ActualWidth;
                ImageCanvas.Height = ViewerBorder.ActualHeight;

                // Apply clipping region to the canvas
                ImageCanvas.Clip = new RectangleGeometry(new Rect(0, 0, ViewerBorder.ActualWidth, ViewerBorder.ActualHeight));
                UpdateCanvasBounds();
            }
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateZoom();
        }
    }
}