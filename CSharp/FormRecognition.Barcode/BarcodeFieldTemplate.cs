using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.FormsProcessing.FormRecognition;
using Vintasoft.Imaging.ImageProcessing.Transforms;
using Vintasoft.Imaging.TypeConverters;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Primitives;

#if !REMOVE_BARCODE_SDK
using Vintasoft.Barcode; 
#endif

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// Contains information about barcode field template.
    /// </summary>
    public class BarcodeFieldTemplate : FormFieldTemplate
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeFieldTemplate"/> class.
        /// </summary>
        public BarcodeFieldTemplate()
            : base()
        {
        }

#if !REMOVE_BARCODE_SDK
        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeFieldTemplate"/> class.
        /// </summary>
        /// <param name="readerSettings">The barcode reader settings.</param>
        public BarcodeFieldTemplate(ReaderSettings readerSettings)
            : this()
        {
            _readerSettings = readerSettings;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeFieldTemplate"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        public BarcodeFieldTemplate(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ReaderSettings = (ReaderSettings)info.GetValue("ReaderSettings", typeof(ReaderSettings));
        } 
#endif

        #endregion



        #region Properties

#if !REMOVE_BARCODE_SDK
        ReaderSettings _readerSettings;
        /// <summary>
        /// Gets or sets the barcode reader settings used for recognition of the field.
        /// </summary>
        /// <value>
        /// The barcode reader settings.
        /// </value>
        [Description("Barcode reader settings used for recognition of the field.")]
        [TypeConverter(typeof(SimpleTypeConverter))]
        public ReaderSettings ReaderSettings
        {
            get
            {
                return _readerSettings;
            }
            set
            {
                _readerSettings = value;
            }
        } 
#endif

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Recognizes a barcode field on the specified image.
        /// </summary>
        /// <param name="formRecognitionParams">Recognition parameters of a form field
        /// that specify a transform from <i>image</i> to the template image.</param>
        /// <returns>Recognized field.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <i>formRecognitionParams</i> is <b>null</b>.
        /// </exception>
        public override FormField Recognize(FormRecognitionParams formRecognitionParams)
        {
#if !REMOVE_BARCODE_SDK
            if (formRecognitionParams == null)
                throw new ArgumentNullException("formRecognitionParams");

            VintasoftImage image = formRecognitionParams.Image;
            RectangleF boundingBox = GetBoundingBoxOnImage(formRecognitionParams.TemplateSize);
            if (boundingBox.Width <= 0 || boundingBox.Height <= 0)
                return new BarcodeField(this, null, image.Resolution);

            using (BarcodeReader reader = new BarcodeReader())
            {
                reader.Settings = _readerSettings;

                // image aligning command
                MatrixTransformCommand aligningCommand = new MatrixTransformCommand(formRecognitionParams.ImageTransformMatrix);

                // calculate bounding box in pixels using resolution of the template image

                float dx = 96 / (float)formRecognitionParams.TemplateSize.Resolution.Horizontal;
                float dy = 96 / (float)formRecognitionParams.TemplateSize.Resolution.Vertical;
                RectangleF rectInPixels = new RectangleF(boundingBox.X / dx, boundingBox.Y / dy,
                    boundingBox.Width / dx, boundingBox.Height / dy);

                // set the rectangle of aligned image to be cropped
                aligningCommand.CropRect = Rectangle.Round(rectInPixels);
                aligningCommand.IsNested = true;

                // get aligned region of the field
                using (VintasoftImage alignedRegion = aligningCommand.Execute(image))
                {
                    // recognize barcodes
                    IBarcodeInfo[] recognizedBarcodes;
                    using (VintasoftBitmap bitmap = alignedRegion.GetAsVintasoftBitmap())
                        recognizedBarcodes = reader.ReadBarcodes(bitmap);

                    IBarcodeInfo recognizedBarcode = null;
                    if (recognizedBarcodes.Length > 0)
                    {
                        recognizedBarcode = recognizedBarcodes[0];

                        // choose a barcode with maximal area

                        double maxArea = GetAreaOfRegion(recognizedBarcode.Region);
                        for (int i = 1; i < recognizedBarcodes.Length; i++)
                        {
                            double area = GetAreaOfRegion(recognizedBarcodes[i].Region);
                            if (area > maxArea)
                            {
                                maxArea = area;
                                recognizedBarcode = recognizedBarcodes[i];
                            }
                        }
                    }

                    return new BarcodeField(this, recognizedBarcode, formRecognitionParams.TemplateSize.Resolution);
                }
            } 
#else
            return null;
#endif
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            BarcodeFieldTemplate result = new BarcodeFieldTemplate();
            result.BoundingBox = this.BoundingBox;
            result.Name = this.Name;
#if !REMOVE_BARCODE_SDK
            if (this.ReaderSettings != null)
                result.ReaderSettings = this.ReaderSettings.Clone(); 
#endif
            return result;
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
#if !REMOVE_BARCODE_SDK
            info.AddValue("ReaderSettings", this.ReaderSettings); 
#endif
        }

        #endregion


        #region PRIVATE

#if !REMOVE_BARCODE_SDK
        /// <summary>
        /// Returns the area of a quadrilateral region.
        /// </summary>
        /// <param name="region">The quadrilateral region.</param>
        /// <returns></returns>
        private static double GetAreaOfRegion(Vintasoft.Barcode.Region region)
        {
            return GetAreaOfQuadrangle(
                ConvertPoint(region.LeftTop), ConvertPoint(region.RightTop),
                ConvertPoint(region.RightBottom), ConvertPoint(region.LeftBottom));
        }

        /// <summary>
        /// Converts the <see cref="VintasoftPointI"/> to <see cref="System.Windows.Point"/>.
        /// </summary>
        /// <param name="point">The point.</param>
        private static System.Windows.Point ConvertPoint(VintasoftPointI point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

#endif

        /// <summary> 
        /// Returns the area of a quadrangle defined by its vertices.
        /// </summary>
        /// <param name="a">The first vertex.</param>
        /// <param name="b">The second vertex.</param>
        /// <param name="c">The third vertex.</param>
        /// <param name="d">The fourth vertex.</param>
        /// <returns>The area of a quadrangle.</returns>
        private static double GetAreaOfQuadrangle(System.Windows.Point a, System.Windows.Point b, System.Windows.Point c, System.Windows.Point d)
        {
            return GetAreaOfTriangle(a, b, c) + GetAreaOfTriangle(a, c, d);
        }

        /// <summary>
        /// Returns the area of a triangle defined by its vertices.
        /// </summary>
        /// <param name="pa">The first vertex.</param>
        /// <param name="pb">The second vertex.</param>
        /// <param name="pc">The third vertex.</param>
        /// <returns>The area of a triangle.</returns>
        private static double GetAreaOfTriangle(System.Windows.Point pa, System.Windows.Point pb, System.Windows.Point pc)
        {
            double a = GetLineLength(pa, pb);
            double b = GetLineLength(pb, pc);
            double c = GetLineLength(pc, pa);
            double p = (a + b + c) / 2;
            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        /// <summary>
        /// Returns the length of a line.
        /// </summary>
        /// <param name="p1">The first end of the line.</param>
        /// <param name="p2">The second end of the line.</param>
        /// <returns>The length of a line.</returns>
        private static float GetLineLength(System.Windows.Point p1, System.Windows.Point p2)
        {
            return (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        #endregion

        #endregion

    }
}
