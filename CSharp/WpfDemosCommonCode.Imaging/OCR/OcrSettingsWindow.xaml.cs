using System.Diagnostics;
using System.Windows;

using Vintasoft.Imaging.ImageProcessing;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Ocr;
using Vintasoft.Imaging.Ocr.Tesseract;
#endif


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to edit the OCR engine manager settings.
    /// </summary>
    public partial class OcrSettingsWindow : Window
    {

        #region Fields

        /// <summary>
        /// The maximum count of threads, which can be used for text recognition.
        /// </summary>
        int _maxThreads = 0;

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// The selected languages.
        /// </summary>
        OcrLanguage[] _selectedLanguages;

        /// <summary>
        /// The supported languages.
        /// </summary>
        OcrLanguage[] _supportedLanguages;
#endif

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrSettingsWindow"/> class.
        /// </summary>
        public OcrSettingsWindow()
        {
            InitializeComponent();
        }

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Initializes a new instance of the <see cref="OcrSettingsWindow"/> class.
        /// </summary>
        /// <param name="tesseractOcrSettings">The settings of Tesseract OCR engine.</param>
        /// <param name="supportedLanguages">The languages,
        /// which are supported by Tesseract OCR engine.</param>
        /// <param name="imageBinarizationMode">The image binarization mode
        /// of Tesseract OCR engine.</param>
        /// <param name="highlightLowConfidenceWordsAfterRecognition">
        /// Indicates that words with low confidence must be highlighted.
        /// </param>
        public OcrSettingsWindow(
            TesseractOcrSettings tesseractOcrSettings,
            OcrLanguage[] supportedLanguages,
            OcrBinarizationMode imageBinarizationMode,
            bool highlightLowConfidenceWordsAfterRecognition)
            : this(tesseractOcrSettings,
            supportedLanguages,
            imageBinarizationMode,
            highlightLowConfidenceWordsAfterRecognition,
            null,
            false,
            System.Environment.ProcessorCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrSettingsWindow"/> class.
        /// </summary>
        /// <param name="tesseractOcrSettings">The settings of Tesseract OCR engine.</param>
        /// <param name="supportedLanguages">The languages,
        /// which are supported by Tesseract OCR engine.</param>
        /// <param name="imageBinarizationMode">The image binarization mode
        /// of Tesseract OCR engine.</param>
        /// <param name="highlightLowConfidenceWordsAfterRecognition">
        /// Indicates that words with low confidence must be highlighted after text recognition.
        /// </param>
        /// <param name="useMultithreading">Indicates that text recognition must be
        /// executed in multiple threads.</param>
        /// <param name="maxThreads">The maximum count of threads,
        /// which can be used for text recognition.</param>
        public OcrSettingsWindow(
            TesseractOcrSettings tesseractOcrSettings,
            OcrLanguage[] supportedLanguages,
            OcrBinarizationMode imageBinarizationMode,
            bool highlightLowConfidenceWordsAfterRecognition,
            bool useMultithreading,
            int maxThreads)
            : this(tesseractOcrSettings,
            supportedLanguages,
            imageBinarizationMode,
            highlightLowConfidenceWordsAfterRecognition,
            null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrSettingsWindow"/> class.
        /// </summary>
        /// <param name="tesseractOcrSettings">The settings of Tesseract OCR engine.</param>
        /// <param name="supportedLanguages">The languages,
        /// which are supported by Tesseract OCR engine.</param>
        /// <param name="imageBinarizationMode">The image binarization mode
        /// of Tesseract OCR engine.</param>
        /// <param name="highlightLowConfidenceWordsAfterRecognition">
        /// Indicates that words with low confidence must be highlighted.
        /// </param>
        /// <param name="ocrRecognitionRegionSplittingSettings">The OCR recognition region
        /// splitting settings.</param>
        public OcrSettingsWindow(
            TesseractOcrSettings tesseractOcrSettings,
            OcrLanguage[] supportedLanguages,
            OcrBinarizationMode imageBinarizationMode,
            bool highlightLowConfidenceWordsAfterRecognition,
            OcrRecognitionRegionSplittingSettings ocrRecognitionRegionSplittingSettings)
            : this(tesseractOcrSettings, supportedLanguages,
            imageBinarizationMode,
            highlightLowConfidenceWordsAfterRecognition,
            ocrRecognitionRegionSplittingSettings,
            false,
            System.Environment.ProcessorCount)
        {
            useMultithreadingCheckBox.IsChecked = false;
            useMultithreadingGroupBox.IsEnabled = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrSettingsWindow"/> class.
        /// </summary>
        /// <param name="tesseractOcrSettings">The settings of Tesseract OCR engine.</param>
        /// <param name="supportedLanguages">The languages,
        /// which are supported by Tesseract OCR engine.</param>
        /// <param name="imageBinarizationMode">The image binarization mode
        /// of Tesseract OCR engine.</param>
        /// <param name="highlightLowConfidenceWordsAfterRecognition">
        /// Indicates that words with low confidence must be highlighted after text recognition.
        /// </param>
        /// <param name="ocrRecognitionRegionSplittingSettings">The OCR recognition region
        /// splitting settings.</param>
        /// <param name="useMultithreading">Indicates that text recognition must be
        /// executed in multiple threads.</param>
        /// <param name="maxThreads">The maximum count of threads,
        /// which can be used for text recognition.</param>
        public OcrSettingsWindow(
            TesseractOcrSettings tesseractOcrSettings,
            OcrLanguage[] supportedLanguages,
            OcrBinarizationMode imageBinarizationMode,
            bool highlightLowConfidenceWordsAfterRecognition,
            OcrRecognitionRegionSplittingSettings ocrRecognitionRegionSplittingSettings,
            bool useMultithreading,
            int maxThreads)
            : this()
        {
            _tesseractOcrSettings = tesseractOcrSettings;

            if (ocrRecognitionRegionSplittingSettings == OcrRecognitionRegionSplittingSettings.Default)
            {
                ocrRecognitionRegionSplittingSettings =
                    (OcrRecognitionRegionSplittingSettings)OcrRecognitionRegionSplittingSettings.Default.Clone();
            }
            _ocrRecognitionRegionSplittingSettings = ocrRecognitionRegionSplittingSettings;
            if (ocrRecognitionRegionSplittingSettings == null)
                ocrRecognitionRegionSplittingSettings = OcrRecognitionRegionSplittingSettings.Default;

            _supportedLanguages = supportedLanguages;
            _selectedLanguages = tesseractOcrSettings.Languages;

            recognitionModeComboBox.Items.Add(TesseractOcrRecognitionMode.Quality);
            recognitionModeComboBox.Items.Add(TesseractOcrRecognitionMode.Speed);

            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizePageWithPageSegmentationAndOrientationDetection);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizePageWithPageSegmentation);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSingleColumn);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSingleBlockOfVertText);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSingleBlock);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSingleLine);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSingleWord);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeCircleWord);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSingleChar);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSparseTextWithPageOrientationDetection);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.RecognizeSparseText);
            recognitionRegionTypeComboBox.Items.Add(RecognitionRegionType.DetectPageOrientation);

            imageBinarizationModeComboBox.Items.Add(OcrBinarizationMode.None);
            imageBinarizationModeComboBox.Items.Add(OcrBinarizationMode.Adaptive);
            imageBinarizationModeComboBox.Items.Add(OcrBinarizationMode.Global);

            recognitionModeComboBox.SelectedItem = _tesseractOcrSettings.RecognitionMode;
            recognitionRegionTypeComboBox.SelectedItem = _tesseractOcrSettings.RecognitionRegionType;
            imageBinarizationModeComboBox.SelectedItem = imageBinarizationMode;
            useCustomDictionariesCheckBox.IsChecked = _tesseractOcrSettings.UseCustomDictionaries;
            charsWhiteListTextBox.Text = _tesseractOcrSettings.CharWhiteList;
            maxBlobOverlapsNumericUpDown.Value = _tesseractOcrSettings.MaxBlobOverlaps;
            highlightLowConfidenceWordsAfterRecognitionCheckBox.IsChecked = highlightLowConfidenceWordsAfterRecognition;

            maxRegionHeightNumericUpDown.Value = ocrRecognitionRegionSplittingSettings.MaxRegionHeight;
            maxRegionWidthNumericUpDown.Value = ocrRecognitionRegionSplittingSettings.MaxRegionWidth;
            maxWordSizeNumericUpDown.Value = ocrRecognitionRegionSplittingSettings.MaxWordSize;

            useMultithreadingCheckBox.IsChecked = useMultithreading;
            useMultithreadingGroupBox.IsEnabled = useMultithreading;
            _maxThreads = maxThreads;
            maxThreadsNumericUpDown.Value = maxThreads;
        }
#endif

        #endregion



        #region Properties

#if !REMOVE_OCR_PLUGIN
        TesseractOcrSettings _tesseractOcrSettings;
        /// <summary>
        /// Gets the settings of Tesseract OCR engine.
        /// </summary>
        public TesseractOcrSettings TesseractOcrSettings
        {
            get
            {
                return _tesseractOcrSettings;
            }
        }

        OcrRecognitionRegionSplittingSettings _ocrRecognitionRegionSplittingSettings;
        /// <summary>
        /// Gets the OCR recognition region splitting settings.
        /// </summary>
        public OcrRecognitionRegionSplittingSettings OcrRecognitionRegionSplittingSettings
        {
            get
            {
                return _ocrRecognitionRegionSplittingSettings;
            }
        }
#endif

        /// <summary>
        /// Gets the binarization mode of image.
        /// </summary>
        public OcrBinarizationMode ImageBinarizationMode
        {
            get
            {
                return (OcrBinarizationMode)imageBinarizationModeComboBox.SelectedItem;
            }
        }

        /// <summary>
        /// Gets a value indicating whether words with low confidence must be
        /// highlighted after text recognition.
        /// </summary>
        public bool HighlightLowConfidenceWordsAfterRecognition
        {
            get
            {
                return highlightLowConfidenceWordsAfterRecognitionCheckBox.IsChecked.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// the highlightLowConfidenceWordsAfterRecognitionCheckBox control must be visible.
        /// </summary>
        public bool ShowHighlightLowConfidenceWordsCheckBox
        {
            get
            {
                return highlightLowConfidenceWordsAfterRecognitionCheckBox.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                    highlightLowConfidenceWordsAfterRecognitionCheckBox.Visibility = Visibility.Visible;
                else
                    highlightLowConfidenceWordsAfterRecognitionCheckBox.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the image binarization mode can be changed.
        /// </summary>
        public bool CanChooseBinarization
        {
            get
            {
                return imageBinarizationModeComboBox.IsEnabled;
            }
            set
            {
                imageBinarizationModeComboBox.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether multithreading can be used.
        /// </summary>
        public bool UseMultithreading
        {
            get
            {
                if (useMultithreadingCheckBox.IsChecked.Value == true)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets the maximum count of threads, which can be used for text recognition.
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return (int)maxThreadsNumericUpDown.Value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Clicked the "OK" button.
        /// </summary>
        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            _tesseractOcrSettings.Languages = _selectedLanguages;

            if (recognitionModeComboBox.SelectedItem != null)
                _tesseractOcrSettings.RecognitionMode = (TesseractOcrRecognitionMode)recognitionModeComboBox.SelectedItem;
            if (recognitionRegionTypeComboBox.SelectedItem != null)
                _tesseractOcrSettings.RecognitionRegionType = (RecognitionRegionType)recognitionRegionTypeComboBox.SelectedItem;

            if (useCustomDictionariesCheckBox.IsChecked.Value == true)
                _tesseractOcrSettings.UseCustomDictionaries = true;
            else
                _tesseractOcrSettings.UseCustomDictionaries = false;
            _tesseractOcrSettings.CharWhiteList = charsWhiteListTextBox.Text;
            _tesseractOcrSettings.MaxBlobOverlaps = (int)maxBlobOverlapsNumericUpDown.Value;

            if (_ocrRecognitionRegionSplittingSettings == null)
            {
                _ocrRecognitionRegionSplittingSettings = new OcrRecognitionRegionSplittingSettings(
                    (int)maxRegionWidthNumericUpDown.Value,
                    (int)maxRegionHeightNumericUpDown.Value,
                    (int)maxWordSizeNumericUpDown.Value);
            }
            else
            {
                _ocrRecognitionRegionSplittingSettings.MaxRegionWidth = (int)maxRegionWidthNumericUpDown.Value;
                _ocrRecognitionRegionSplittingSettings.MaxRegionHeight = (int)maxRegionHeightNumericUpDown.Value;
                _ocrRecognitionRegionSplittingSettings.MaxWordSize = (int)maxWordSizeNumericUpDown.Value;
            }
#endif
            DialogResult = true;
        }

        /// <summary>
        /// Clicked the "Cancel" button.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// The "Select Languages" button is clicked.
        /// </summary>
        private void selectLanguagesButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            OcrLanguagesWindow dialog = new OcrLanguagesWindow(_selectedLanguages, _supportedLanguages);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
                _selectedLanguages = dialog.SelectedLanguages;
#endif
        }

        /// <summary>
        /// Changed the maximum count of threads, which can be used for text recognition.
        /// </summary>
        private void maxThreadsNumericUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (IsInitialized && maxThreadsTrackBar.Value != maxThreadsNumericUpDown.Value)
                maxThreadsTrackBar.Value = maxThreadsNumericUpDown.Value;
        }

        /// <summary>
        /// Changed the maximum count of threads, which can be used for text recognition.
        /// </summary>
        private void maxThreadsTrackBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsInitialized && maxThreadsTrackBar.Value != maxThreadsNumericUpDown.Value)
                maxThreadsNumericUpDown.Value = (int)maxThreadsTrackBar.Value;
        }

        /// <summary>
        /// Clicked the resetMaxThreads button.
        /// </summary>
        private void resetMaxThreadsButton_Click(object sender, RoutedEventArgs e)
        {
            maxThreadsNumericUpDown.Value = _maxThreads;
        }

        /// <summary>
        /// Enabled/disabled the multithreading for text recognition.
        /// </summary>
        private void useMultithreadingCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (useMultithreadingCheckBox.IsChecked.Value == true)
                useMultithreadingGroupBox.IsEnabled = true;
            else
                useMultithreadingGroupBox.IsEnabled = false;
        }

#endregion

    }
}
