using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.ImageProcessing;

using WpfDemosCommonCode;
using WpfDemosCommonCode.Imaging;

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A window that allows to binarize an image.
    /// </summary>
    public partial class ImageBinarizationWindow : Window
    {

        #region Fields

        /// <summary>
        /// Current image.
        /// </summary>
        VintasoftImage _image;

        /// <summary>
        /// Current image processing command which is used for image binarization.
        /// </summary>
        ChangePixelFormatToBlackWhiteCommand _processingCommand;

        /// <summary>
        /// Current rendering settings which are used for vector image rendering.
        /// </summary>
        RenderingSettings _renderingSettings;

        /// <summary>
        /// String key for threshold binarization.
        /// </summary>
        string THRESHOLD_BINARIZATION = "Threshold";

        /// <summary>
        /// String key for global binarization.
        /// </summary>
        string GLOBAL_BINARIZATION = "Global";

        /// <summary>
        /// String key for adaptive binarization.
        /// </summary>
        string ADAPTIVE_BINARIZATION = "Adaptive";

        /// <summary>
        /// String key for halftone binarization.
        /// </summary>
        string HALFTONE_BINARIZATION = "Halftone";

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBinarizationWindow"/> class.
        /// </summary>
        public ImageBinarizationWindow()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBinarizationWindow"/> class.
        /// </summary>
        /// <param name="binarizeCommand">Current image processing command
        /// which is used for image binarization.</param>
        /// <param name="renderingSettings">Current rendering settings
        /// which are used for vector image rendering.</param>
        public ImageBinarizationWindow(
            ChangePixelFormatToBlackWhiteCommand binarizeCommand,
            RenderingSettings renderingSettings)
            : this()
        {
            imageViewerToolBar1.ImageViewer = imageViewer1;

            binarizationTypeComboBox.Items.Add(THRESHOLD_BINARIZATION);
            binarizationTypeComboBox.Items.Add(GLOBAL_BINARIZATION);
            binarizationTypeComboBox.Items.Add(ADAPTIVE_BINARIZATION);
            binarizationTypeComboBox.Items.Add(HALFTONE_BINARIZATION);

            if (binarizeCommand != null)
                _processingCommand = binarizeCommand;
            else
                _processingCommand = new ChangePixelFormatToBlackWhiteCommand(BinarizationMode.Global);
            if (renderingSettings != null)
                _renderingSettings = renderingSettings;
            else
                _renderingSettings = new RenderingSettings(300, 300);
            UpdateBinarizationModeComboBox();
        }

        #endregion



        #region Properties

        bool _applyForAll = false;
        /// <summary>
        /// Gets a value that indicates whether current settings 
        /// shall be applied to all remaining images.
        /// </summary>
        public bool ApplyForAll
        {
            get
            {
                return _applyForAll;
            }
        }

        bool _cancel = false;
        /// <summary>
        /// Gets a value that indicates whether image binarization was canceled.
        /// </summary>
        public bool Cancel
        {
            get
            {
                return _cancel;
            }
        }

        bool _skip = false;
        /// <summary>
        /// Gets a value that indicates whether current image shall be skipped.
        /// </summary>
        public bool Skip
        {
            get
            {
                return _skip;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Show dialog for an image binarization.
        /// </summary>
        /// <param name="image">Image to binarize.</param>
        /// <returns>
        /// <b>true</b> if image should be binarized;
        /// <b>false</b> if image should NOT be binarized.
        /// </returns>
        public bool ShowDialog(VintasoftImage image)
        {
            _image = image;
            _image.RenderingSettings = _renderingSettings;
            bool? dialogResult = ShowDialog();
            if (dialogResult == true)
                return true;
            if (!Skip)
                _cancel = true;
            return false;
        }

        /// <summary>
        /// Returns the current image processing command which is used for image binarization.
        /// </summary>
        /// <returns>The current image processing command which is used for image binarization.</returns>
        public ProcessingCommandBase GetProcessingCommand()
        {
            return _processingCommand;
        }

        /// <summary>
        /// Returns the current rendering settings which are used for vector image rendering.
        /// </summary>
        /// <returns>The current rendering settings which are used for vector image rendering.</returns>
        public RenderingSettings GetRenderingSettings()
        {
            return _renderingSettings;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Handles the Loaded event of Window object.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _skip = false;
            try
            {
                ExecuteProcessing();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
                _skip = true;
                Hide();
            }
        }

        /// <summary>
        /// Handles the Closed event of Window object.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            imageViewer1.Images.ClearAndDisposeItems();
        }

        /// <summary>
        /// Executes binarization with current settings
        /// and shows result in the viewer.
        /// </summary>
        private void ExecuteProcessing()
        {
            ProcessingCommandBase command = GetProcessingCommand();
            if (command != null)
            {
                VintasoftImage currentImage = imageViewer1.Image;
                _image.RenderingSettings = _renderingSettings;
                imageViewer1.Image = command.Execute(_image);
                if (currentImage != null)
                {
                    currentImage.Dispose();
                    currentImage = null;
                }
            }
        }

        /// <summary>
        /// Updates binarization mode and updates binarized image.
        /// </summary>
        private void binarizationTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_image != null)
            {
                UpdateBinarizationCommandMode();
                ExecuteProcessing();
            }
        }

        #endregion

        /// <summary>
        /// Shows binarization settings and updates binarized image.
        /// </summary>
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            PropertyGridWindow binarizationPropertiesForm = new PropertyGridWindow(_processingCommand, _processingCommand.Name);
            binarizationPropertiesForm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            binarizationPropertiesForm.Owner = this;
            binarizationPropertiesForm.ShowDialog();
            if (!UpdateBinarizationModeComboBox())
                ExecuteProcessing();
        }

        /// <summary>
        /// Shows rendering settings and updates binarized image.
        /// </summary>
        private void renderingSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            RenderingSettingsWindow renderingSettingsForm = new RenderingSettingsWindow(_renderingSettings);
            renderingSettingsForm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            renderingSettingsForm.Owner = this;
            if (renderingSettingsForm.ShowDialog().Value)
            {
                _renderingSettings = renderingSettingsForm.RenderingSettings;
                ExecuteProcessing();
            }
        }

        /// <summary>
        /// Closes form with DialogResult.OK.
        /// </summary>
        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Sets the <see cref="ApplyToAll"/> to <b>true</b> and closes form with DialogResult.OK.
        /// </summary>
        private void applyToAllButton_Click(object sender, RoutedEventArgs e)
        {
            _applyForAll = true;
            DialogResult = true;
        }

        /// <summary>
        /// Sets the <see cref="Cancel"/> to <b>true</b> and closes form with DialogResult.Cancel.
        /// </summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            _cancel = true;
            DialogResult = false;
        }

        /// <summary> 
        /// Updates binarization mode of the binarization command from combo box value.
        /// </summary>
        private bool UpdateBinarizationCommandMode()
        {
            BinarizationMode oldBinarizationMode = _processingCommand.BinarizationMode;
            string selectedItem = (string)binarizationTypeComboBox.SelectedItem;
            if (selectedItem == THRESHOLD_BINARIZATION)
            {
                _processingCommand.BinarizationMode = BinarizationMode.Threshold;
            }
            else if (selectedItem == GLOBAL_BINARIZATION)
            {
                _processingCommand.BinarizationMode = BinarizationMode.Global;
            }
            else if (selectedItem == ADAPTIVE_BINARIZATION)
            {
                _processingCommand.BinarizationMode = BinarizationMode.Adaptive;
            }
            else if (selectedItem == HALFTONE_BINARIZATION)
            {
                _processingCommand.BinarizationMode = BinarizationMode.Halftone;
            }

            return _processingCommand.BinarizationMode != oldBinarizationMode;
        }

        /// <summary>
        /// Updates combo box value from binarization mode of the binarization command.
        /// </summary>
        private bool UpdateBinarizationModeComboBox()
        {
            object oldSelectedItem = binarizationTypeComboBox.SelectedItem;
            switch (_processingCommand.BinarizationMode)
            {
                case BinarizationMode.Threshold:
                    binarizationTypeComboBox.SelectedItem = THRESHOLD_BINARIZATION;
                    break;

                case BinarizationMode.Global:
                    binarizationTypeComboBox.SelectedItem = GLOBAL_BINARIZATION;
                    break;

                case BinarizationMode.Adaptive:
                    binarizationTypeComboBox.SelectedItem = ADAPTIVE_BINARIZATION;
                    break;

                case BinarizationMode.Halftone:
                    binarizationTypeComboBox.SelectedItem = HALFTONE_BINARIZATION;
                    break;
            }
            if (oldSelectedItem == binarizationTypeComboBox.SelectedItem)
                return false;
            return true;
        }

        #endregion

    }
}
