using System;
using System.Collections.Generic;
using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.UI;


namespace WpfDemosCommonCode
{
    /// <summary>
    /// A form that allows to:
    /// <ul>
    /// <li>view and select the image processing commands</li>
    /// <li>view source image</li>
    /// <li>apply selected image processing command to the source image and view the processed image</li>
    /// </ul>
    /// </summary>
    public partial class ImageProcessingWindow : Window
    {

        #region Fields

        /// <summary>
        /// The source image for processing.
        /// </summary>
        VintasoftImage _sourceImage;

        /// <summary>
        /// The current image in viewer.
        /// </summary>
        VintasoftImage _currentImage;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessingWindow"/> class.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        private ImageProcessingWindow(VintasoftImage sourceImage)
        {
            InitializeComponent();

            _sourceImage = sourceImage;
            imageViewer1.Image = sourceImage;
            imageViewerToolStrip1.ImageViewer = imageViewer1;
            imageViewer1.SizeMode = ImageSizeMode.PixelToPixel;

            this.Activated += new EventHandler(ImageProcessingWindow_Activated);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessingWindow"/> class.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="availableImageProcessingCommands">The available processing commands.</param>
        /// <param name="selectedImageProcessingCommands">The image processing commands,
        /// which must be applied to an image by default.</param>
        public ImageProcessingWindow(
            VintasoftImage sourceImage,
            ProcessingCommandBase[] availableImageProcessingCommands,
            params ProcessingCommandBase[] selectedImageProcessingCommands)
            : this(sourceImage)
        {
            Dictionary<string, ProcessingCommandBase[]> availableProcessingCommandsDictionary =
                new Dictionary<string, ProcessingCommandBase[]>();
            availableProcessingCommandsDictionary.Add(string.Empty, availableImageProcessingCommands);

            imageProcessingCommandSelectionControl1.AvailableProcessingCommands = availableProcessingCommandsDictionary;
            imageProcessingCommandSelectionControl1.SelectedCommands = selectedImageProcessingCommands;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessingWindow"/> class.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="availableImageProcessingCommands">The available processing commands.</param>
        /// <param name="selectedImageProcessingCommands">The image processing commands,
        /// which must be applied to an image by default.</param>
        public ImageProcessingWindow(
            VintasoftImage sourceImage,
            Dictionary<string, ProcessingCommandBase[]> availableImageProcessingCommands,
            params ProcessingCommandBase[] selectedImageProcessingCommands)
            : this(sourceImage)
        {
            imageProcessingCommandSelectionControl1.AvailableProcessingCommands = availableImageProcessingCommands;
            imageProcessingCommandSelectionControl1.SelectedCommands = selectedImageProcessingCommands;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets the selected commands.
        /// </summary>
        public ProcessingCommandBase[] SelectedCommands
        {
            get
            {
                if (enableImageProcessingCheckBox.IsChecked.Value == true)
                    return imageProcessingCommandSelectionControl1.SelectedCommands;
                else
                    return null;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// The window is activated.
        /// </summary>
        private void ImageProcessingWindow_Activated(object sender, EventArgs e)
        {
            if (imageProcessingCommandSelectionControl1.SelectedCommands != null &&
                imageProcessingCommandSelectionControl1.SelectedCommands.Length > 0)
                enableImageProcessingCheckBox.IsChecked = true;

            this.Activated -= ImageProcessingWindow_Activated;
        }

        /// <summary>
        /// Enables/disables the image processing.
        /// </summary>
        private void enableImageProcessingCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (enableImageProcessingCheckBox.IsChecked.Value == true)
                imageProcessingCommandSelectionControl1.IsEnabled = true;
            else
                imageProcessingCommandSelectionControl1.IsEnabled = false;

            ProcessImage();
        }

        /// <summary>
        /// Selected processing commands is changed.
        /// </summary>
        private void imageProcessingCommandsEditor1_ProcessingCommandsChanged(
            object sender,
            System.EventArgs e)
        {
            ProcessImage();
        }

        /// <summary>
        /// Processes the image.
        /// </summary>
        private void ProcessImage()
        {
            if (SelectedCommands == null ||
                SelectedCommands.Length == 0)
            {
                UpdateImage(_sourceImage);
            }
            else
            {
                CompositeCommand processingCommand = new CompositeCommand(SelectedCommands);

                imageProcessingCommandSelectionControl1.IsEnabled = false;
                try
                {
                    processingCommand.Started += new EventHandler<ImageProcessingEventArgs>(processingCommand_Started);
                    processingCommand.Progress += new EventHandler<ImageProcessingProgressEventArgs>(processingCommand_Progress);
                    processingCommand.Finished += new EventHandler<ImageProcessedEventArgs>(processingCommand_Finished);

                    VintasoftImage processedImage;
                    try
                    {
                        processedImage = processingCommand.Execute(_sourceImage);
                    }
                    finally
                    {
                        processingCommand.Started -= processingCommand_Started;
                        processingCommand.Progress -= processingCommand_Progress;
                        processingCommand.Finished -= processingCommand_Finished;
                    }

                    UpdateImage(processedImage);
                }
                catch (Exception exc)
                {
                    DemosTools.ShowErrorMessage(exc);
                    return;
                }
                finally
                {
                    imageProcessingCommandSelectionControl1.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// The processing command is started.
        /// </summary>
        private void processingCommand_Started(object sender, ImageProcessingEventArgs e)
        {
            processImageProgressBar.Value = 0;
            processImageProgressBar.Visibility = Visibility.Visible;
            imageProcessingCommandSelectionControl1.IsEnabled = false;
            enableImageProcessingCheckBox.IsEnabled = false;
        }

        /// <summary>
        /// The progress, of processing command, is changed.
        /// </summary>
        private void processingCommand_Progress(object sender, ImageProcessingProgressEventArgs e)
        {
            processImageProgressBar.Value = e.Progress;
            DemosTools.DoEvents();
        }

        /// <summary>
        /// The processing command is finished.
        /// </summary>
        private void processingCommand_Finished(object sender, ImageProcessedEventArgs e)
        {
            processImageProgressBar.Visibility = Visibility.Hidden;
            imageProcessingCommandSelectionControl1.IsEnabled = true;
            enableImageProcessingCheckBox.IsEnabled = true;
        }

        /// <summary>
        /// Updates the image in image viewer.
        /// </summary>
        /// <param name="image">The image.</param>
        private void UpdateImage(VintasoftImage image)
        {
            imageViewer1.LoadTemporaryImage(image, true);

            if (_currentImage != null)
            {
                _currentImage.Dispose();
                _currentImage = null;
            }

            if (image != _sourceImage)
                _currentImage = image;
        }

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion

    }
}
