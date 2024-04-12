using System.Windows;

#if !REMOVE_BARCODE_SDK
using Vintasoft.Barcode; 
#endif


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A window that allows to view and edit the barcode reader settings.
    /// </summary>
    public partial class BarcodeReaderSettingsWindow : Window
    {

        #region Fields

#if !REMOVE_BARCODE_SDK
        ReaderSettings _readerSettings; 
#endif

        #endregion



        #region Constructors

        public BarcodeReaderSettingsWindow()
        {
            InitializeComponent();
        }

#if !REMOVE_BARCODE_SDK
        public BarcodeReaderSettingsWindow(ReaderSettings readerSettings)
            : this()
        {
            _readerSettings = readerSettings;
            barcodeReaderSettingsControl1.RestoreSettings(readerSettings);
            barcodeReaderSettingsControl1.CanChangeExpectedBarcodes = false;
        } 
#endif

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of btOk object.
        /// </summary>
        private void btOk_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_BARCODE_SDK
            barcodeReaderSettingsControl1.SetReaderSettings(_readerSettings); 
#endif
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of btCancel object.
        /// </summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
