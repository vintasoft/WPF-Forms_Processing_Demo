using System.Collections.Generic;
using System.Windows.Media;

#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Ocr.Wpf.UI;
#endif
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Omr.Wpf.UI;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI;

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// Stores and allows to apply settings of OMR and OCR form field views.
    /// </summary>
    public class FormFieldViewSettings
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFieldViewSettings"/> class.
        /// </summary>
        public FormFieldViewSettings()
        {
        }

        #endregion



        #region Properties

        #region OCR form field

        float _ocrConfidenceThreshold = 0.75f;
        /// <summary>
        /// Gets or sets the confidence threshold for OCR form field.
        /// </summary>
        public float OcrConfidenceThreshold
        {
            get
            {
                return _ocrConfidenceThreshold;
            }
            set
            {
                _ocrConfidenceThreshold = value;
            }
        }

        Brush _ocrCertainObjectsBrush = new SolidColorBrush(Color.FromArgb(150, 144, 238, 144));
        /// <summary>
        /// Gets or sets the background brush of recognized OCR object
        /// which confidence is greater than ConfidenceThreshold.
        /// </summary>
        public Brush OcrCertainObjectsBrush
        {
            get
            {
                return _ocrCertainObjectsBrush;
            }
            set
            {
                _ocrCertainObjectsBrush = value;
            }
        }

        Pen _ocrCertainObjectsPen = new Pen(new SolidColorBrush(Color.FromArgb(150, 0, 128, 0)), 1f);
        /// <summary>
        /// Gets or sets the border pen of recognized OCR object
        /// which confidence is greater than ConfidenceThreshold.
        /// </summary>
        public Pen OcrCertainObjectsPen
        {
            get
            {
                return _ocrCertainObjectsPen;
            }
            set
            {
                _ocrCertainObjectsPen = value;
            }
        }

        Brush _ocrUncertainObjectsBrush = new SolidColorBrush(Color.FromArgb(48, 255, 0, 0));
        /// <summary>
        /// Gets or sets the background brush of recognized OCR object
        /// which confidence is lower than ConfidenceThreshold.
        /// </summary>
        public Brush OcrUncertainObjectsBrush
        {
            get
            {
                return _ocrUncertainObjectsBrush;
            }
            set
            {
                _ocrUncertainObjectsBrush = value;
            }
        }

        Pen _ocrUncertainObjectsPen = new Pen(new SolidColorBrush(Color.FromArgb(192, 255, 0, 0)), 1f);
        /// <summary>
        /// Gets or sets the border pen of recognized OCR object
        /// which confidence is lower than ConfidenceThreshold.
        /// </summary>
        public Pen OcrUncertainObjectsPen
        {
            get
            {
                return _ocrUncertainObjectsPen;
            }
            set
            {
                _ocrUncertainObjectsPen = value;
            }
        }

        #endregion


        #region OMR form field

        Pen _omrPen = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), 1f);
        /// <summary>
        /// Gets or sets the border pen for OMR form field.
        /// </summary>
        public Pen OmrPen
        {
            get
            {
                return _omrPen;
            }
            set
            {
                _omrPen = value;
            }
        }

        Brush _omrFilledBrush = new SolidColorBrush(Color.FromArgb(150, 144, 238, 144));
        /// <summary>
        /// Gets or sets the background brush for filled OMR form field.
        /// </summary>
        public Brush OmrFilledBrush
        {
            get
            {
                return _omrFilledBrush;
            }
            set
            {
                _omrFilledBrush = value;
            }
        }

        Brush _omrUndefinedBrush = null;
        /// <summary>
        /// Gets or sets the background brush for undefined OMR form field.
        /// </summary>
        public Brush OmrUndefinedBrush
        {
            get
            {
                return _omrUndefinedBrush;
            }
            set
            {
                _omrUndefinedBrush = value;
            }
        }


        Brush _omrUnfilledBrush = null;
        /// <summary>
        /// Gets or sets the background brush for unfilled OMR form field.
        /// </summary>
        public Brush OmrUnfilledBrush
        {
            get
            {
                return _omrUnfilledBrush;
            }
            set
            {
                _omrUnfilledBrush = value;
            }
        }

        #endregion

        #endregion



        #region Methods

        /// <summary>
        /// Sets settings for form field view.
        /// </summary>
        /// <param name="view">The form field view.</param>
        public void SetSettings(WpfFormFieldView view)
        {
            WpfOmrFieldView omrView = view as WpfOmrFieldView;
            // if view is OMR form field view
            if (omrView != null)
            {
                // set OMR form field settings
                omrView.Pen = OmrPen;
                omrView.FilledBrush = OmrFilledBrush;
                omrView.UndefinedBrush = OmrUndefinedBrush;
                omrView.UnfilledBrush = OmrUnfilledBrush;
            }

#if !REMOVE_OCR_PLUGIN
            WpfOcrFieldView ocrView = view as WpfOcrFieldView;
            // if view is OCR form field view
            if (ocrView != null)
            {
                // set OCR form field settings
                ocrView.ConfidenceThreshold = OcrConfidenceThreshold;
                ocrView.CertainObjectsBrush = OcrCertainObjectsBrush;
                ocrView.CertainObjectsPen = OcrCertainObjectsPen;
                ocrView.UncertainObjectsBrush = OcrUncertainObjectsBrush;
                ocrView.UncertainObjectsPen = OcrUncertainObjectsPen;
            }
#endif

            WpfFormFieldGroupView groupView = view as WpfFormFieldGroupView;
            // if view is group view
            if (groupView != null)
            {
                // set form field settings for all single form field view
                SetSettings(groupView.ViewItems);
            }

        }

        /// <summary>
        /// Sets settings for all getted form field views.
        /// </summary>
        /// <param name="items">The collection of form field views.</param>
        public void SetSettings(ICollection<WpfFormFieldView> items)
        {
            // for each form field view in collection
            foreach (WpfFormFieldView view in items)
            {
                SetSettings(view);
            }
        }

        #endregion

    }
}
