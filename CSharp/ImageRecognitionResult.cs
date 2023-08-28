using System.Collections.Generic;
using System.Text;

using Vintasoft.Imaging;
using Vintasoft.Imaging.FormsProcessing.FormRecognition;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI;
using Vintasoft.Imaging.FormsProcessing.TemplateMatching;
using Vintasoft.Imaging.Utils;


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// Stores the result of template matching and forms recognition of an image
    /// and some additional information.
    /// </summary>
    class ImageRecognitionResult
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRecognitionResult"/> class.
        /// </summary>
        internal ImageRecognitionResult()
        {
        }

        #endregion



        #region Properties

        ImageImprintCompareResult _compareResult;
        /// <summary>
        /// Gets or sets the result of image comparison.
        /// </summary>
        internal ImageImprintCompareResult CompareResult
        {
            get
            {
                return _compareResult;
            }
            set
            {
                _compareResult = value;
            }
        }

        VintasoftImage _cachedImage;
        /// <summary>
        /// Gets or sets the aligned recognized image.
        /// </summary>
        internal VintasoftImage AlignedImage
        {
            get
            {
                return _cachedImage;
            }
            set
            {
                _cachedImage = value;
            }
        }

        WpfFormFieldView _recognizedPageView;
        /// <summary>
        /// Gets or sets the recognized form page view.
        /// </summary>
        internal WpfFormFieldView RecognizedPageView
        {
            get
            {
                return _recognizedPageView;
            }
            set
            {
                _recognizedPageView = value;
            }
        }

        string _templateName;
        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        internal string TemplateName
        {
            get
            {
                return _templateName;
            }
            set
            {
                _templateName = value;
            }
        }

        Dictionary<WpfFormFieldView, WpfFormFieldView> _parentsTable;
        /// <summary>
        /// Gets or sets a dictionary that binds recognized field views and their parents' views.
        /// </summary>
        internal Dictionary<WpfFormFieldView, WpfFormFieldView> ParentsTable
        {
            get
            {
                return _parentsTable;
            }
            set
            {
                _parentsTable = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Returns the value of the <see cref="ImageRecognitionResult"/> as a string.
        /// </summary>
        public override string ToString()
        {
            ImageImprintCompareResult imprintCompareResult = _compareResult;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Template recognition:");
            stringBuilder.AppendLine();

            AffineMatrix resultTransform = imprintCompareResult.TransformMatrix;
            string transform = "null";
            if (resultTransform != null)
                transform = string.Format(
                    "({0:F3};{1:F3};{2:F3};{3:F3};{4:F3};{5:F3})",
                    resultTransform.M11,
                    resultTransform.M12,
                    resultTransform.M21,
                    resultTransform.M22,
                    resultTransform.OffsetX,
                    resultTransform.OffsetY);

            stringBuilder.AppendLine(string.Format(
                "Confidence: {0:F1}%",
                imprintCompareResult.Confidence * 100));
            stringBuilder.AppendLine(string.Format(
                "Transform Matrix: {0}",
                transform));
            stringBuilder.AppendLine(string.Format(
                "Average similarity: {0:F3} ({1} zones)",
                imprintCompareResult.AvgSimilarity,
                imprintCompareResult.CoincidentZoneCount));
            stringBuilder.AppendLine(string.Format(
                "Average location deviation: {0:F3} (X={1:F3}, Y={2:F3})",
                imprintCompareResult.AvgLocationDeviation,
                imprintCompareResult.AvgLocationDeviationX,
                imprintCompareResult.AvgLocationDeviationY));
            stringBuilder.AppendLine(string.Format(
                "Missing source zone count: {0}",
                imprintCompareResult.NotCoincidentSourceZoneCount));
            stringBuilder.AppendLine(string.Format(
                "Missing dest zone count: {0}",
                imprintCompareResult.NotCoincidentDestZoneCount));

            if (_recognizedPageView != null)
            {
                FormFieldGroup recognizedFieldGroup = _recognizedPageView.Field as FormFieldGroup;
                if (recognizedFieldGroup != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("Form recognition:");

                    int itemCount = recognizedFieldGroup.Items.Count;
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(string.Format("Total {0} recognized form fields:", itemCount));

                    for (int i = 0; i < itemCount; i++)
                    {
                        stringBuilder.AppendLine();

                        FormField recognizedField = recognizedFieldGroup.Items[i];
                        stringBuilder.AppendLine(string.Format(
                            "Field type: \"{0}\"", recognizedField.GetType().Name));
                        stringBuilder.AppendLine(string.Format(
                            "Field name: \"{0}\"", recognizedField.Name));
                        if (recognizedField.Value == null)
                            stringBuilder.AppendLine("Field value: <null>");
                        else
                            stringBuilder.AppendLine(string.Format(
                                "Field value: \"{0}\"", recognizedField.Value.ToString().Replace('\n',' ').Trim()));
                        stringBuilder.AppendLine(string.Format(
                            "Confidence: {0:F1}%", recognizedField.Confidence * 100));
                    }
                }
            }

            return stringBuilder.ToString();
        }

        #endregion

    }
}
