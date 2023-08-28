using System;
using System.Windows;
using System.Windows.Media;

using Vintasoft.Imaging;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI;
using Vintasoft.Primitives;

#if !REMOVE_BARCODE_SDK
using Vintasoft.Barcode; 
#endif

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// Determines how to display a recognized barcode field
    /// and how user can interact with it.
    /// </summary>
    public class WpfBarcodeFieldView : WpfFormFieldView
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeFieldView"/> class.
        /// </summary>
        /// <param name="field">The barcode field.</param>
        public WpfBarcodeFieldView(BarcodeField field)
            : base(field)
        {
            BarcodeBrush = Brush;
            Brush = null;
        }

        #endregion



        #region Properties

        Brush _barcodeBrush;
        /// <summary>
        /// Gets or sets the background brush of the recognized barcode.
        /// </summary>     
        public Brush BarcodeBrush
        {
            get
            {
                return _barcodeBrush;
            }
            set
            {
                if (_barcodeBrush != value)
                {
                    _barcodeBrush = value;
                    OnStateChanged(EventArgs.Empty);
                }
            }
        }

        Pen _barcodePen = new Pen(new SolidColorBrush(Color.FromArgb(150, 0, 255, 0)), 1.0);
        /// <summary>
        /// Gets or sets the border pen of the recognized barcode.
        /// </summary>  
        public Pen BarcodePen
        {
            get
            {
                return _barcodePen;
            }
            set
            {
                if (_barcodePen != value)
                {
                    _barcodePen = value;
                    OnStateChanged(EventArgs.Empty);
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Draws the barcode field on the <see cref="Media.DrawingContext"/>
        /// in the coordinate space of barcode field.
        /// </summary>
        /// <param name="g">The <see cref="Media.DrawingContext"/> to draw on.</param>
        protected override void DrawInContentSpace(DrawingContext g)
        {
            // draw the field area
            base.DrawInContentSpace(g);

#if !REMOVE_BARCODE_SDK
            // the recognized barcode field
            BarcodeField field = Field as BarcodeField;
            // the recognized barcode info
            IBarcodeInfo barcodeInfo = field.BarcodeInfo;
            // if barcode is recognized
            if (barcodeInfo != null)
            {
                // get barcode region points, in pixels
                VintasoftPointI[] regionPoints = barcodeInfo.Region.GetPoints();
                // the resolution of the template image
                Resolution imageResolution = field.TemplateResolution;
                // get the location of the bounding box
                System.Drawing.PointF fieldLocation = field.FieldTemplate.BoundingBox.Location;

                // array of points in the DIP space (device-independent pixels, 1/96 of inch)
                Point[] regionPointsInDIP = new Point[regionPoints.Length];
                for (int i = 0; i < regionPoints.Length; i++)
                {
                    // calculate the location of the point in the DIP space

                    VintasoftPointI pointInPixels = regionPoints[i];
                    regionPointsInDIP[i] = new Point(
                        pointInPixels.X * (96 / imageResolution.Horizontal) + fieldLocation.X,
                        pointInPixels.Y * (96 / imageResolution.Vertical) + fieldLocation.Y);
                }


                PathFigure figure = new PathFigure();
                figure.StartPoint = regionPointsInDIP[0];
                for (int i = 0; i < regionPointsInDIP.Length; i++)
                {
                    LineSegment segment = new LineSegment(regionPointsInDIP[i], true);
                    figure.Segments.Add(segment);
                }
                figure.Segments.Add(new LineSegment(regionPointsInDIP[0], true));
                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(figure);
                g.DrawGeometry(BarcodeBrush, BarcodePen, pathGeometry);
            } 
#endif
        }

        #endregion

    }
}
