using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A window that allows to edit form field view settings.
    /// </summary>
    public partial class FormFieldViewSettingsEditorWindow : Window
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFieldViewSettingsEditorWindow"/> class.
        /// </summary>
        /// <param name="settings">Form field view settings.</param>
        public FormFieldViewSettingsEditorWindow(FormFieldViewSettings settings)
        {
            InitializeComponent();


            _formFieldSettings = settings;

            // set OCR form field view settings
            confidenceThresholdSlider.Value = (int)(FormFieldSettings.OcrConfidenceThreshold * 100);
            if (FormFieldSettings.OcrCertainObjectsBrush is SolidColorBrush)
                certainObjectsBackgroundColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OcrCertainObjectsBrush).Color;
            if (FormFieldSettings.OcrUncertainObjectsBrush is SolidColorBrush)
                uncertainObjectsBackgroundColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OcrUncertainObjectsBrush).Color;
            certainObjectsBorderColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OcrCertainObjectsPen.Brush).Color;
            certainObjectsBorderWidthNumericUpDown.Value = (int)FormFieldSettings.OcrCertainObjectsPen.Thickness;
            uncertainObjectsBorderColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OcrUncertainObjectsPen.Brush).Color;
            uncertainObjectsBorderWidthNumericUpDown.Value = (int)FormFieldSettings.OcrUncertainObjectsPen.Thickness;

            // set OMR form field view settings
            if (FormFieldSettings.OmrFilledBrush is SolidColorBrush)
                filledObjectsBackgroundColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OmrFilledBrush).Color;
            if (FormFieldSettings.OmrUnfilledBrush is SolidColorBrush)
                unfilledObjectsBackgroundColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OmrUnfilledBrush).Color;
            if (FormFieldSettings.OmrUndefinedBrush is SolidColorBrush)
                undefinedObjectsBorderColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OmrUndefinedBrush).Color;
            objectsBorderColorPanelControl.Color = ((SolidColorBrush)FormFieldSettings.OmrPen.Brush).Color;
            objectsBorderWidthNumericUpDown.Value = (int)FormFieldSettings.OmrPen.Thickness;
        }

        #endregion



        #region Properties

        FormFieldViewSettings _formFieldSettings;
        /// <summary>
        /// Gets or sets form field view settings.
        /// </summary>
        public FormFieldViewSettings FormFieldSettings
        {
            get
            {
                return _formFieldSettings;
            }
            set
            {
                _formFieldSettings = value;
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // set OCR form field view settings
            FormFieldSettings.OcrConfidenceThreshold = (float)confidenceThresholdSlider.Value / 100.0f;
            FormFieldSettings.OcrCertainObjectsBrush = new SolidColorBrush(certainObjectsBackgroundColorPanelControl.Color);
            FormFieldSettings.OcrUncertainObjectsBrush = new SolidColorBrush(uncertainObjectsBackgroundColorPanelControl.Color);
            FormFieldSettings.OcrCertainObjectsPen = new Pen(
                new SolidColorBrush(certainObjectsBorderColorPanelControl.Color),
                (float)certainObjectsBorderWidthNumericUpDown.Value);
            FormFieldSettings.OcrUncertainObjectsPen = new Pen(
                new SolidColorBrush(uncertainObjectsBorderColorPanelControl.Color),
                (float)uncertainObjectsBorderWidthNumericUpDown.Value);

            // set OMR form field view settings
            FormFieldSettings.OmrFilledBrush = new SolidColorBrush(filledObjectsBackgroundColorPanelControl.Color);
            FormFieldSettings.OmrUnfilledBrush = new SolidColorBrush(unfilledObjectsBackgroundColorPanelControl.Color);
            FormFieldSettings.OmrUndefinedBrush = new SolidColorBrush(undefinedObjectsBorderColorPanelControl.Color);
            FormFieldSettings.OmrPen = new Pen(
                new SolidColorBrush(objectsBorderColorPanelControl.Color),
                (float)objectsBorderWidthNumericUpDown.Value);

            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
