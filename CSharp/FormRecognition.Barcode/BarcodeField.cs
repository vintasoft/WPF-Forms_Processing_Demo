using System.ComponentModel;

using Vintasoft.Imaging;
using Vintasoft.Imaging.FormsProcessing.FormRecognition;
using Vintasoft.Imaging.TypeConverters;

#if !REMOVE_BARCODE_SDK
using Vintasoft.Barcode; 
#endif

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// Contains information about recognized <see cref="BarcodeFieldTemplate"/>.
    /// </summary>
    public class BarcodeField : FormField
    {

        #region Constructors

#if !REMOVE_BARCODE_SDK
        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeField"/> class.
        /// </summary>
        /// <param name="fieldTemplate">Template of the barcode field.</param>
        /// <param name="barcodeInfo">The recognized barcode information.</param>
        /// <param name="templateResolution">The template resolution.</param>
        public BarcodeField(BarcodeFieldTemplate fieldTemplate, IBarcodeInfo barcodeInfo,
            Resolution templateResolution)
            : base(fieldTemplate)
        {
            _barcodeInfo = barcodeInfo;
            _templateResolution = templateResolution;
        } 
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeField"/> class.
        /// </summary>
        /// <param name="fieldTemplate">Template of the barcode field.</param>
        public BarcodeField(BarcodeFieldTemplate fieldTemplate)
            : base(fieldTemplate)
        {
        }
#endif

        #endregion



        #region Properties

#if !REMOVE_BARCODE_SDK
        IBarcodeInfo _barcodeInfo;
        /// <summary>
        /// Gets the recognized barcode information.
        /// </summary>
        [Description("The recognized barcode information.")]
        [TypeConverter(typeof(SimpleTypeConverter))]
        public IBarcodeInfo BarcodeInfo
        {
            get
            {
                return _barcodeInfo;
            }
        } 
#endif

        Resolution _templateResolution;
        /// <summary>
        /// Gets the template resolution.
        /// </summary>
        [Description("The template resolution.")]
        public Resolution TemplateResolution
        {
            get
            {
                return _templateResolution;
            }
        }

        /// <summary>
        /// Gets the value of the barcode field in a text form.
        /// </summary>
        [Description("The value of the barcode field in a text form.")]
        public override string Value
        {
            get
            {
#if !REMOVE_BARCODE_SDK
                if (BarcodeInfo == null)
                    return null;
                return BarcodeInfo.Value; 
#else
                return string.Empty;
#endif
            }
        }

        /// <summary>
        /// Gets the confidence of the recognition result.
        /// </summary>
        /// <value>
        /// Possible values are from 0.0 (0%) to 1.0 (100%).
        /// </value>
        public override float Confidence
        {
            get
            {
#if !REMOVE_BARCODE_SDK
                if (BarcodeInfo == null)
                    return 0f;

                if (BarcodeInfo.BarcodeInfoClass == BarcodeInfoClass.Barcode1D)
                    return (float)BarcodeInfo.ReadingQuality;

                if (BarcodeInfo.Confidence > 100)
                    return 1f;

                return (float)(BarcodeInfo.Confidence / 100.0); 
#else
                return 0;
#endif
            }
        }

        #endregion

    }
}
