using System.Diagnostics;
using System.Windows;

#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Ocr;
using Vintasoft.Imaging.Ocr.Tesseract;
#endif


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to edit the language settings of OCR engine manager.
    /// </summary>
    public partial class OcrLanguagesWindow : Window
    {

        #region Constructors

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Initializes a new instance of the <see cref="OcrLanguagesWindow"/> class.
        /// </summary>
        /// <param name="selectedLanguages">The selected languages.</param>
        /// <param name="supportedLanguages">The languages,
        /// which are supported by Tesseract OCR engine.</param>
        public OcrLanguagesWindow(
            OcrLanguage[] selectedLanguages,
            OcrLanguage[] supportedLanguages)
        {
            InitializeComponent();

            ocrLanguagesListBox1.AvailableLanguages = supportedLanguages;
            ocrLanguagesListBox1.SelectedLanguages = selectedLanguages;
        }
#endif

        #endregion



        #region Properties

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Gets the selected languages.
        /// </summary>
        public OcrLanguage[] SelectedLanguages
        {
            get
            {
                return ocrLanguagesListBox1.SelectedLanguages;
            }
        }
#endif

        #endregion



        #region Methods

        /// <summary>
        /// The "OK" button is clicked.
        /// </summary>
        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            if (SelectedLanguages.Length == 0)
            {
                MessageBox.Show(
                    "The language is not selected.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                DialogResult = true;
            }
#endif
        }

        /// <summary>
        /// The "Download additional language dictionaries..." button is clicked.
        /// </summary>
        private void downloadAdditionalLanguageDictionariesButton_Click(object sender, RoutedEventArgs e)
        {
            DemosTools.OpenBrowser("https://www.vintasoft.com/docs/vsimaging-dotnet/Programming-Ocr-Prepare_OCR_engine_for_text_recognition.html");
        }

        #endregion

    }
}
