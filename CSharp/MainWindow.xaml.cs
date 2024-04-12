using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.FormsProcessing;
using Vintasoft.Imaging.FormsProcessing.FormRecognition;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Formatters;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Ocr;
#endif
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Omr;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI.VisualTools;
using Vintasoft.Imaging.FormsProcessing.TemplateMatching;
using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.ImageProcessing.Filters;
using Vintasoft.Imaging.ImageProcessing.Document;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Ocr;
using Vintasoft.Imaging.Ocr.Tesseract;
#endif
using Vintasoft.Imaging.UI;
#if !REMOVE_BARCODE_SDK
using Vintasoft.Barcode;
#endif

using WpfDemosCommonCode;
using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;
#if !REMOVE_PDF_PLUGIN
using WpfDemosCommonCode.Pdf;
#endif


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A main window of "Form Processing Demo" application.
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields

        /// <summary>
        /// Template of the application's title.
        /// </summary>
        string _titlePrefix = string.Format("VintaSoft WPF Forms Processing Demo v{0}", ImagingGlobalSettings.ProductVersion);

        /// <summary>
        /// Visual tool for previewing the recognized fields in the viewer.
        /// </summary>
        WpfFormFieldViewerTool _recognizedFieldViewerTool;

        /// <summary>
        /// Window that allows to edit and manage template images and form field templates.
        /// </summary>
        TemplateEditorWindow _templateEditorWindow;

        /// <summary>
        /// Dictionary that binds filled images and recognition results.
        /// </summary>
        Dictionary<VintasoftImage, ImageRecognitionResult> _filledImageToRecognitionResultMap = new Dictionary<VintasoftImage, ImageRecognitionResult>();

        /// <summary>
        /// Dictionary that binds template images and template image imprints.
        /// </summary>
        Dictionary<VintasoftImage, ImageImprint> _templateImageToImageImprintMap = new Dictionary<VintasoftImage, ImageImprint>();

        /// <summary>
        /// Form recognition manager.
        /// </summary>
        FormRecognitionManager _formRecognitionManager;

        /// <summary>
        /// A value indicating whether key line recognizer is enabled.
        /// </summary>
        bool _enabledKeyLineRecognizer = true;

        /// <summary>
        /// A value indicating whether key 'L' mark recognizer is enabled.
        /// </summary>
        bool _enabledKeyLPatternRecognizer = false;

        /// <summary>
        /// Key zone recognizers, which are used in form processing.
        /// </summary>
        KeyZoneRecognizerCommand[] _keyZoneRecognizerCommands;

        /// <summary>
        /// Selected key zone recognizers.
        /// </summary>
        KeyZoneRecognizerCommand[] _selectedKeyZoneRecognizerCommands;

        /// <summary>
        /// Current image processing command which is used for image binarization.
        /// </summary>
        ChangePixelFormatToBlackWhiteCommand _binarizeCommand;

        /// <summary>
        /// Current rendering settings which are used for vector image rendering.
        /// </summary>
        RenderingSettings _renderingSettings;

        /// <summary>
        /// Color of hovered field view pen.
        /// </summary>
        Color _currentRecognizedFieldViewPenColor = Color.FromArgb(0, 0, 0, 0);

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// The OCR engine.
        /// </summary>
        OcrEngine _ocrEngine;

        /// <summary>
        /// The default settings of OCR engine.
        /// </summary>
        OcrEngineSettings _defaultOcrEngineSettings;

        /// <summary>
        /// The default OCR recognition region splitting settings.
        /// </summary>
        OcrRecognitionRegionSplittingSettings _defaultOcrRecognitionRegionSplittingSettings;
#endif

#if !REMOVE_BARCODE_SDK
        /// <summary>
        /// The default settings of barcode reader.
        /// </summary>
        ReaderSettings _defaultBarcodeReaderSettings;
#endif

        /// <summary>
        /// The preprocessing command for key zone recognizer.
        /// </summary>
        ProcessingCommandBase _preprocessingCommandKeyZoneRecognizer = null;

        Stopwatch _recognitionStopwatch = new Stopwatch();

        /// <summary>
        /// The form field view settings.
        /// </summary>
        FormFieldViewSettings _formFieldViewSettings = new FormFieldViewSettings();

        OpenFileDialog _openFileDialog = new OpenFileDialog();


        #region Hot keys

        public static RoutedCommand _openTemplateCommand = new RoutedCommand();
        public static RoutedCommand _openFilledImagesCommand = new RoutedCommand();
        public static RoutedCommand _addFilledImagesCommand = new RoutedCommand();
        public static RoutedCommand _closeAllCommand = new RoutedCommand();
        public static RoutedCommand _recognizeCurrentPageCommand = new RoutedCommand();
        public static RoutedCommand _recognizeAllPagesCommand = new RoutedCommand();
        public static RoutedCommand _aboutCommand = new RoutedCommand();
        public static RoutedCommand _rotateClockwiseCommand = new RoutedCommand();
        public static RoutedCommand _rotateCounterclockwiseCommand = new RoutedCommand();

        #endregion

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            // register the evaluation license for VintaSoft Imaging .NET SDK
            Vintasoft.Imaging.ImagingGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");

            InitializeComponent();

            // init "Template Matching" => "Image Imprint Generator" 
            lineRecognizerMenuItem.Tag = "KeyLine";
            patternRecognizerMenuItem.Tag = "KeyLPattern";
            lineAndPatternRecognizerMenuItem.Tag = "All";
            _keyZoneRecognizerCommands = new KeyZoneRecognizerCommand[] {
                        new KeyLineRecognizerCommand(),
                        new KeyMarkRecognizerCommand()
            };
            _selectedKeyZoneRecognizerCommands = new KeyZoneRecognizerCommand[] {
                _keyZoneRecognizerCommands[0]
            };

            filledImageViewer.InputGestureCopy = null;
            filledImageViewer.InputGestureCut = null;
            filledImageViewer.InputGestureDelete = null;
            filledImageViewer.InputGestureInsert = null;
            recognizedImageViewer.InputGestureCopy = null;
            recognizedImageViewer.InputGestureCut = null;
            recognizedImageViewer.InputGestureDelete = null;
            recognizedImageViewer.InputGestureInsert = null;
            _openFileDialog.Multiselect = true;

            sourceThumbnailViewer.MasterViewer = filledImageViewer;
            filledImageViewerToolStrip.ImageViewer = filledImageViewer;

            // load XPS codec
            DemosTools.LoadXpsCodec();
            // set XPS rendering requirement
            DemosTools.SetXpsRenderingRequirement(filledImageViewer, 0f);
            DemosTools.SetXpsRenderingRequirement(recognizedImageViewer, 0f);
            CodecsFileFilters.SetFilters(_openFileDialog);

            alignImagesByTemplateMenuItem.Checked += new RoutedEventHandler(alignImagesByTemplateMenuItem_CheckedChanged);
            alignImagesByTemplateMenuItem.Unchecked += new RoutedEventHandler(alignImagesByTemplateMenuItem_CheckedChanged);

            InitImageScaleMenu();

            _recognizedFieldViewerTool = new WpfFormFieldViewerTool();
            _recognizedFieldViewerTool.MouseLeftButtonDown += new MouseButtonEventHandler(recognizedFieldViewerTool_MouseLeftButtonDown);
            _recognizedFieldViewerTool.FieldViewMouseEnter += new EventHandler<WpfFormFieldViewEventArgs>(recognizedFieldViewerTool_FieldViewMouseEnter);
            _recognizedFieldViewerTool.FieldViewMouseLeave += new EventHandler<WpfFormFieldViewEventArgs>(recognizedFieldViewerTool_FieldViewMouseLeave);
            recognizedImageViewer.VisualTool = _recognizedFieldViewerTool;

            _formRecognitionManager = new FormRecognitionManager();

            _binarizeCommand = new ChangePixelFormatToBlackWhiteCommand(BinarizationMode.Global);
            _renderingSettings = new RenderingSettings(300, 300);

#if !REMOVE_OCR_PLUGIN
            try
            {
                _ocrEngine = new TesseractOcr(TesseractOcrDllDirectory);
                OcrFieldTemplate.OcrEngineManager = new OcrEngineManager(_ocrEngine);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage("OCR engine error", ex);
            }

            TesseractOcrSettings tesseractSettings = new TesseractOcrSettings(OcrLanguage.English);
            tesseractSettings.MaxBlobOverlaps = 1;
            _defaultOcrEngineSettings = tesseractSettings;

            _defaultOcrRecognitionRegionSplittingSettings =
               (OcrRecognitionRegionSplittingSettings)OcrRecognitionRegionSplittingSettings.Default.Clone();
#endif

#if !REMOVE_BARCODE_SDK
            _defaultBarcodeReaderSettings = new ReaderSettings();
            _defaultBarcodeReaderSettings.ExpectedBarcodes = 1;
            _defaultBarcodeReaderSettings.ScanBarcodeTypes = BarcodeType.Code39 | BarcodeType.Code128;
            _defaultBarcodeReaderSettings.AutomaticRecognition = true;

#endif
            // create a template editor form
            _templateEditorWindow = new TemplateEditorWindow(
                _formRecognitionManager.FormTemplates,
                _binarizeCommand,
                _renderingSettings,
                TesseractOcrDllDirectory);
#if !REMOVE_OCR_PLUGIN
            _templateEditorWindow.DefaultOcrEngineSettings = _defaultOcrEngineSettings;
            _templateEditorWindow.DefaultOcrRecognitionRegionSplittingSettings = _defaultOcrRecognitionRegionSplittingSettings;
#endif
#if !REMOVE_BARCODE_SDK
            _templateEditorWindow.DefaultBarcodeReaderSettings = _defaultBarcodeReaderSettings;
#endif
            _templateEditorWindow.Closing += new CancelEventHandler(_templateEditorForm_Closing);

            filledImageViewer.FocusedIndexChanged += new PropertyChangedEventHandler<int>(filledImageViewer_FocusedIndexChanged);
            sourceThumbnailViewer.Images.ImageCollectionChanged += new EventHandler<ImageCollectionChangeEventArgs>(Images_ImageCollectionChanged);

            // register view for barcode field template
            WpfFormFieldTemplateViewFactory.RegisterViewForFieldTemplate(
                typeof(BarcodeFieldTemplate),
                typeof(WpfBarcodeFieldTemplateView));
            // register view for barcode field
            WpfFormFieldViewFactory.RegisterViewForField(
                typeof(BarcodeField),
                typeof(WpfBarcodeFieldView));

            // set custom serialization binder for correct custom form field templates deserialization
            FormFieldTemplateSerializationBinder.Current = new CustomFormFieldTemplateSerializationBinder();

            // set CustomFontProgramsController for all opened PDF documents
            CustomFontProgramsController.SetDefaultFontProgramsController();

            UpdateUI();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);

            DemosTools.SetTestFilesFolder(_openFileDialog);
        }

        #endregion



        #region Properties

        bool _isClosing = false;
        /// <summary>
        /// Gets or sets a value indicating whether current form is closing.
        /// </summary>
        bool IsClosing
        {
            get
            {
                return _isClosing;
            }
            set
            {
                _isClosing = value;
            }
        }

        string _tesseractOcrDllDirectory = null;
        /// <summary>
        /// Gets a directory where Tesseract5.Vintasoft.xXX.dll is located.
        /// </summary>
        public string TesseractOcrDllDirectory
        {
            get
            {
                if (_tesseractOcrDllDirectory == null)
                {
                    // Tesseract OCR dll filename
                    string dllFilename;
                    // if is 64-bit system then
                    if (IntPtr.Size == 8)
                        dllFilename = "Tesseract5.Vintasoft.x64.dll";
                    else
                        dllFilename = "Tesseract5.Vintasoft.x86.dll";

                    string currentDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);

                    // search directories
                    string[] directories = new string[]
                    {
                        "",
                        @"TesseractOCR\",
                        @"Debug\net6.0-windows\TesseractOCR\",
                        @"Release\net6.0-windows\TesseractOCR\",
                        @"Debug\net7.0-windows\TesseractOCR\",
                        @"Release\net7.0-windows\TesseractOCR\",
                        @"Debug\net8.0-windows\TesseractOCR\",
                        @"Release\net8.0-windows\TesseractOCR\",
                        @"..\..\TesseractOcr\",
                    };

                    // search tesseract dll
                    foreach (string dir in directories)
                    {
                        string dllDirectory = Path.Combine(currentDirectory, dir);
                        if (File.Exists(Path.Combine(dllDirectory, dllFilename)))
                        {
                            _tesseractOcrDllDirectory = dllDirectory;
                            break;
                        }
                    }
                    if (_tesseractOcrDllDirectory == null)
                        _tesseractOcrDllDirectory = currentDirectory;
                    else
                        _tesseractOcrDllDirectory = Path.GetFullPath(_tesseractOcrDllDirectory);
                }
                return _tesseractOcrDllDirectory;
            }
        }

        bool _isRecognizeAllAsync = false;
        public bool IsRecognizeAllAsync
        {
            get
            {
                return _isRecognizeAllAsync;
            }
            set
            {
                _isRecognizeAllAsync = value;
                InvokeUpdateUI();
            }
        }

        #endregion



        #region Methods

        #region Main Form

        /// <summary>
        /// Handles the Loaded event of the MainWindow control.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // open test files
                string testFiles = DemosTools.FindTestFilesFolder();
                if (testFiles != "")
                {
                    string templateFilename = Path.Combine(testFiles, "FormsProcessing_template.fdt");
                    if (File.Exists(templateFilename))
                        _templateEditorWindow.AddDocument(templateFilename, false);

                    string[] filledForms = new string[3];
                    filledForms[0] = "FormsProcessing_filled_100_200_300dpi.tif";
                    filledForms[1] = "FormsProcessing_filled_rotated_100_200_300dpi.tif";
                    filledForms[2] = "FormsProcessing_filled_cropped_damaged_200dpi.tif";
                    for (int i = 0; i < filledForms.Length; i++)
                    {
                        string filledFormFilename = Path.Combine(testFiles, filledForms[i]);
                        if (File.Exists(filledFormFilename))
                            filledImageViewer.Images.Add(filledFormFilename);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Application's form is closing.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            _templateEditorWindow.Close();
            base.OnClosing(e);
            IsClosing = false;
        }

        #endregion


        #region Template Editor Form

        /// <summary>
        /// Template editor form is closing.
        /// </summary>
        void _templateEditorForm_Closing(object sender, CancelEventArgs e)
        {
            if (!this.IsClosing)
            {
                e.Cancel = true;
                _templateEditorWindow.Hide();
                UpdateUI();
            }
        }

        #endregion


        #region UI state

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            UpdateFormTitle();

            int filledImageCount = filledImageViewer.Images.Count;

            openTemplateMenuItem.IsEnabled = !_isRecognizeAllAsync;
            openFilledImagesMenuItem.IsEnabled = !_isRecognizeAllAsync;
            addFilledImagesMenuItem.IsEnabled = !_isRecognizeAllAsync;

            closeAllMenuItem.IsEnabled = filledImageCount > 0 && !_isRecognizeAllAsync;

            recognizeCurrentPageMenuItem.IsEnabled = filledImageCount > 0 && !_isRecognizeAllAsync;
            recognizeAllPagesMenuItem.IsEnabled = filledImageCount > 0 && !_isRecognizeAllAsync;

            imageImprintGeneratorPreprocessingMenuItem.IsEnabled = filledImageCount > 0 && !_isRecognizeAllAsync;

            _templateEditorWindow.IsEnabled = !_isRecognizeAllAsync;
        }

        /// <summary>
        /// Update UI safely.
        /// </summary>
        private void InvokeUpdateUI()
        {
            if (Thread.CurrentThread == Dispatcher.Thread)
                UpdateUI();
            else
                Dispatcher.Invoke(new UpdateUIDelegate(UpdateUI));
        }

        /// <summary>
        /// Updates the caption of this form.
        /// </summary>
        private void UpdateFormTitle()
        {
            this.Title = _titlePrefix;
        }

        /// <summary>
        /// Updates information about focused image.
        /// </summary>
        private void UpdateImageInfo()
        {
            VintasoftImage image = filledImageViewer.Image;
            if (image != null)
            {
                ImageSize size = ImageSize.FromPixels(image.Width, image.Height, image.Resolution);
                imageInfoLabel.Content = string.Format(
                    "Pixel Size: {0}x{1}; Resolution: {2}; Physical Size: {3}x{4} mm",
                    size.WidthInPixels,
                    size.HeightInPixels,
                    size.Resolution,
                    Math.Round(size.WidthInInch * 25.4),
                    Math.Round(size.HeightInInch * 25.4));
            }
            else
            {
                imageInfoLabel.Content = "";
            }
        }

        #endregion


        #region 'File' menu

        /// <summary>
        /// Shows the template editor form.
        /// </summary>
        private void openTemplateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _templateEditorWindow.Show();
            _templateEditorWindow.Activate();
        }

        /// <summary>
        /// Clears image collection of the image viewer and adds image(s) to the image collection
        /// of the image viewer.
        /// </summary>
        private void openFilledImagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFiles(false);
        }

        /// <summary>
        /// Handles the OpenFile event of filledImageViewerToolStrip object.
        /// </summary>
        private void filledImageViewerToolStrip_OpenFile(object sender, EventArgs e)
        {
            if (IsRecognizeAllAsync)
                return;

            OpenFiles(false);
        }

        /// <summary>
        /// Adds image(s) to the image collection of the image viewer.
        /// </summary>
        private void addFilledImagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFiles(true);
        }

        /// <summary>
        /// Clears the image collection.
        /// </summary>
        private void closeAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            filledImageViewer.Images.ClearAndDisposeItems();
            recognizedImageViewer.Images.ClearAndDisposeItems();
            UpdateUI();
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion


        #region 'View' menu

        /// <summary>
        /// Changes the image scale mode of the image viewer.
        /// </summary>
        private void imageScaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem clickedItem = (MenuItem)sender;
            if (!clickedItem.IsChecked)
            {

                if (clickedItem.Tag is ImageSizeMode)
                {
                    SetCurrentImageScaleMenuItem(clickedItem);

                    filledImageViewer.SizeMode = (ImageSizeMode)clickedItem.Tag;
                }
                else
                {
                    int zoomValue = (int)clickedItem.Tag;

                    filledImageViewer.SizeMode = ImageSizeMode.Zoom;
                    filledImageViewer.Zoom = zoomValue;
                }
            }
        }

        /// <summary>
        /// Shows the image viewer settings.
        /// </summary>
        private void imageViewerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImageViewerSettingsWindow viewerSettingsDialog = new ImageViewerSettingsWindow(filledImageViewer);
            viewerSettingsDialog.CanEditMultipageSettings = false;
            viewerSettingsDialog.ShowDialog();

            recognizedImageViewer.ImageAnchor = filledImageViewer.ImageAnchor;
            recognizedImageViewer.RenderingQuality = filledImageViewer.RenderingQuality;
            recognizedImageViewer.FocusPointAnchor = filledImageViewer.FocusPointAnchor;
            recognizedImageViewer.IsFocusPointFixed = filledImageViewer.IsFocusPointFixed;
            recognizedImageViewer.RendererCacheSize = filledImageViewer.RendererCacheSize;
            recognizedImageViewer.ViewerBufferSize = filledImageViewer.ViewerBufferSize;
            recognizedImageViewer.MinImageSizeWhenZoomBufferUsed = filledImageViewer.MinImageSizeWhenZoomBufferUsed;

            recognizedImageViewer.ImageRenderingSettings.InterpolationMode = filledImageViewer.ImageRenderingSettings.InterpolationMode;
            recognizedImageViewer.ImageRenderingSettings.SmoothingMode = filledImageViewer.ImageRenderingSettings.SmoothingMode;
            recognizedImageViewer.ImageRenderingSettings.Resolution = filledImageViewer.ImageRenderingSettings.Resolution;

            recognizedImageViewer.Background = filledImageViewer.Background;
        }

        /// <summary>
        /// Rotates images in image viewers and thumbnail viewer by 90 degrees clockwise.
        /// </summary>
        private void rotateClockwiseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateViewClockwise();
        }

        /// <summary>
        /// Rotates images in image viewers and thumbnail viewer by 90 degrees counterclockwise.
        /// </summary>
        private void rotateCounterclockwiseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateViewCounterClockwise();
        }

        /// <summary>
        /// Shows the form field view settings.
        /// </summary>
        private void formFieldViewSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FormFieldViewSettingsEditorWindow formFieldViewSettingsDialog = new FormFieldViewSettingsEditorWindow(_formFieldViewSettings);
            // if dialog result is true
            if (formFieldViewSettingsDialog.ShowDialog().Value)
            {
                // apply new settings to the form field view collection
                _formFieldViewSettings.SetSettings(_recognizedFieldViewerTool.FieldViewCollection);
                recognizedImageViewer.InvalidateViewer();
            }
        }

        #endregion


        #region 'Forms recognition' menu

        /// <summary>
        /// Recognizes current image.
        /// </summary>
        private void recognizeCurrentPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RecognizeCurrent();
        }

        /// <summary>
        /// Recognizes all images.
        /// </summary>
        private void recognizeAllPagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RecognizeAllAsync();
        }

        /// <summary>
        /// Enables/disables aligning recognized images by template.
        /// </summary>
        private void alignImagesByTemplateMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateRecognizedImageViewer(alignImagesByTemplateMenuItem.IsChecked);
        }

        /// <summary>
        /// Shows the dialog that allows to set count of threads,
        /// which should be used for forms recognition.
        /// </summary>
        private void maxThreadsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImagingEnvironmentMaxThreadsWindow window = new ImagingEnvironmentMaxThreadsWindow();

            window.ShowDialog();
        }

        #endregion


        #region 'Template Matching' menu

        /// <summary>
        /// Edits the MinConfidence property of 
        /// <see cref="Vintasoft.Imaging.FormsProcessing.TemplateMatching.TemplateMatchingCommand"/>
        /// </summary>
        private void templateMatchingConfidenceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TemplateMatchingMinConfidenceEditorWindow templateMatchingMinConfidence = new TemplateMatchingMinConfidenceEditorWindow(_formRecognitionManager.TemplateMatching);
            templateMatchingMinConfidence.ShowDialog();
        }

        /// <summary>
        /// Sets a recognizer to form recognition manager.
        /// </summary>
        private void recognizerToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            lineRecognizerMenuItem.IsChecked = false;
            patternRecognizerMenuItem.IsChecked = false;
            lineAndPatternRecognizerMenuItem.IsChecked = false;

            string recognizer = (string)((MenuItem)sender).Tag;
            _enabledKeyLineRecognizer = false;
            _enabledKeyLPatternRecognizer = false;

            // if recognizer, which is based on lines, must be chosen
            if (recognizer == "KeyLine")
            {
                _enabledKeyLineRecognizer = true;
                _selectedKeyZoneRecognizerCommands = new KeyZoneRecognizerCommand[] {
                    _keyZoneRecognizerCommands[0]
                };
            }
            // if recognizer, which is based on L-patterns, must be chosen
            else if (recognizer == "KeyLPattern")
            {
                _enabledKeyLPatternRecognizer = true;
                _selectedKeyZoneRecognizerCommands = new KeyZoneRecognizerCommand[] {
                    _keyZoneRecognizerCommands[1]
                };
            }
            // if recognizers, which is based on lines and L-patterns, must be chosen
            else if (recognizer == "All")
            {
                _enabledKeyLineRecognizer = true;
                _enabledKeyLPatternRecognizer = true;
                _selectedKeyZoneRecognizerCommands = _keyZoneRecognizerCommands;
            }
            else
            {
                throw new NotImplementedException();
            }

            // create image imprint generator with chosen recognizers
            _formRecognitionManager.TemplateMatching.ImageImprintGenerator = CreateImageImprintGenerator(_preprocessingCommandKeyZoneRecognizer);

            if (_enabledKeyLineRecognizer && _enabledKeyLPatternRecognizer)
            {
                lineAndPatternRecognizerMenuItem.IsChecked = true;
            }
            else
            {
                lineRecognizerMenuItem.IsChecked = _enabledKeyLineRecognizer;
                patternRecognizerMenuItem.IsChecked = _enabledKeyLPatternRecognizer;
            }
        }

        /// <summary>
        /// Shows a dialog that allows to view and change settings of recognizer based on lines.
        /// </summary>
        private void lineRecognizerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get key zone recognizer command
            KeyZoneRecognizerCommand command = (KeyZoneRecognizerCommand)_keyZoneRecognizerCommands[0].Clone();

            PropertyGridWindow propertyGridWindow = new PropertyGridWindow(command, "Key Line Recognizer Properties", true);
            propertyGridWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            propertyGridWindow.Owner = this;

            // if dialog result is true
            if (propertyGridWindow.ShowDialog() == true)
            {
                // save settings of key zone recognizer
                _keyZoneRecognizerCommands[0] = command;

                // if key line recognizer is enabled
                if (_enabledKeyLineRecognizer)
                {
                    // if key 'L' mark recognizer is enabled
                    if (_enabledKeyLPatternRecognizer)
                    {
                        // select both recognizers
                        _selectedKeyZoneRecognizerCommands = _keyZoneRecognizerCommands;
                        // create image imprint generator with selected recognizers
                        _formRecognitionManager.TemplateMatching.ImageImprintGenerator = CreateImageImprintGenerator(_preprocessingCommandKeyZoneRecognizer);
                    }
                    // if key 'L' mark recognizer is disabled
                    else
                    {
                        // select key line recognizer
                        _selectedKeyZoneRecognizerCommands = new KeyZoneRecognizerCommand[] { _keyZoneRecognizerCommands[0] };
                        // create image imprint generator with selected recognizer
                        _formRecognitionManager.TemplateMatching.ImageImprintGenerator = CreateImageImprintGenerator(_preprocessingCommandKeyZoneRecognizer);
                    }
                }
            }
        }

        /// <summary>
        /// Shows a dialog that allows to view and change settings of recognizer based on L-patterns.
        /// </summary>
        private void patternRecognizerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get key zone recognizer command
            KeyZoneRecognizerCommand command = (KeyZoneRecognizerCommand)_keyZoneRecognizerCommands[1].Clone();

            PropertyGridWindow propertyGridWindow = new PropertyGridWindow(command, "Key L-pattern Recognizer Properties", true);
            propertyGridWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            propertyGridWindow.Owner = this;

            // if dialog result is true
            if (propertyGridWindow.ShowDialog() == true)
            {
                // save settings of key zone recognizer
                _keyZoneRecognizerCommands[1] = command;

                // if key 'L' mark recognizer is enabled
                if (_enabledKeyLPatternRecognizer)
                {
                    // if key line recognizer is enabled
                    if (_enabledKeyLineRecognizer)
                    {
                        // select both recognizers
                        _selectedKeyZoneRecognizerCommands = _keyZoneRecognizerCommands;
                        // create image imprint generator with selected recognizers
                        _formRecognitionManager.TemplateMatching.ImageImprintGenerator = CreateImageImprintGenerator(_preprocessingCommandKeyZoneRecognizer);
                    }
                    // if key line recognizer is disabled
                    else
                    {
                        // select key 'L' mark recognizer
                        _selectedKeyZoneRecognizerCommands = new KeyZoneRecognizerCommand[] { _keyZoneRecognizerCommands[1] };
                        // create image imprint generator with selected recognizer
                        _formRecognitionManager.TemplateMatching.ImageImprintGenerator = CreateImageImprintGenerator(_preprocessingCommandKeyZoneRecognizer);
                    }
                }
            }
        }

        /// <summary>
        /// Shows the template matching visualizer.
        /// </summary>
        private void templateMatchingVisualizerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TemplateMatchingVisualizerWindow templateMatchingVisualizerWindow = new TemplateMatchingVisualizerWindow(
                _formRecognitionManager.TemplateMatching,
                _preprocessingCommandKeyZoneRecognizer,
                _keyZoneRecognizerCommands);

            templateMatchingVisualizerWindow.Owner = this;
            templateMatchingVisualizerWindow.Show();
        }

        #endregion


        #region 'Help' menu

        /// <summary>
        /// Shows the "About" dialog.
        /// </summary>
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder description = new StringBuilder();
            description.AppendLine("This project demonstrates the following SDK capabilities:");
            description.AppendLine();
            description.AppendLine("- Create and edit a form template using the mouse.");
            description.AppendLine();
            description.AppendLine("- Save a form template to a file, load a form template from a file.");
            description.AppendLine();
            description.AppendLine("- Recognize form using form template.");
            description.AppendLine();
            description.AppendLine("- Recognize form in multiple threads.");
            description.AppendLine();
            description.AppendLine("- Preview the results of form alignment and form recognition.");
            description.AppendLine();
            description.AppendLine();
            description.AppendLine("The project is available in C# and VB.NET for Visual Studio .NET.");

            WpfAboutBoxBaseWindow dlg = new WpfAboutBoxBaseWindow("vsformsprocessing-dotnet");
            dlg.Description = description.ToString();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        #endregion


        #region Image viewers

        /// <summary>
        /// Index of focused image in viewer is changed.
        /// </summary>
        private void filledImageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            VintasoftImage image = filledImageViewer.Image;
            string filledImageFilenameLabelText;
            if (image == null)
            {
                filledImageFilenameLabelText = "<filled image>";
            }
            else
            {
                if (image.SourceInfo.PageCount == 1)
                    filledImageFilenameLabelText = Path.GetFileName(image.SourceInfo.Filename);
                else
                {
                    filledImageFilenameLabelText = string.Format(
                           "{0}, page {1}",
                           Path.GetFileName(image.SourceInfo.Filename),
                           image.SourceInfo.PageIndex + 1);
                }
            }
            filledImageFilenameLabel.Content = string.Format("Filled image: {0}", filledImageFilenameLabelText);
            UpdateImageInfo();

            UpdateRecognizedImageViewer(alignImagesByTemplateMenuItem.IsChecked);
        }

        /// <summary>
        /// Zoom factor in image viewer is changed.
        /// </summary>
        private void filledImageViewer_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            switch (filledImageViewer.SizeMode)
            {
                case ImageSizeMode.BestFit:
                    SetCurrentImageScaleMenuItem(bestFitMenuItem);
                    break;

                case ImageSizeMode.FitToHeight:
                    SetCurrentImageScaleMenuItem(fitToHeightMenuItem);
                    break;

                case ImageSizeMode.FitToWidth:
                    SetCurrentImageScaleMenuItem(fitToWidthMenuItem);
                    break;

                case ImageSizeMode.Normal:
                    SetCurrentImageScaleMenuItem(normalImageMenuItem);
                    break;

                case ImageSizeMode.PixelToPixel:
                    SetCurrentImageScaleMenuItem(pixelToPixelMenuItem);
                    break;

                case ImageSizeMode.Zoom:
                    SetCurrentImageScaleMenuItem(scaleMenuItem);
                    break;
            }

            recognizedImageViewer.SizeMode = filledImageViewer.SizeMode;
            if (recognizedImageViewer.SizeMode == ImageSizeMode.Zoom)
                recognizedImageViewer.Zoom = filledImageViewer.Zoom;
        }

        /// <summary>
        /// Inits the image scale menu.
        /// </summary>
        private void InitImageScaleMenu()
        {
            normalImageMenuItem.Tag = ImageSizeMode.Normal;
            bestFitMenuItem.Tag = ImageSizeMode.BestFit;
            fitToWidthMenuItem.Tag = ImageSizeMode.FitToWidth;
            fitToHeightMenuItem.Tag = ImageSizeMode.FitToHeight;
            pixelToPixelMenuItem.Tag = ImageSizeMode.PixelToPixel;
            scaleMenuItem.Tag = ImageSizeMode.Zoom;
            scale25MenuItem.Tag = 25;
            scale50MenuItem.Tag = 50;
            scale100MenuItem.Tag = 100;
            scale200MenuItem.Tag = 200;
            scale400MenuItem.Tag = 400;

            SetCurrentImageScaleMenuItem(normalImageMenuItem);
        }

        /// <summary>
        /// Sets current selected image scale menu item.
        /// </summary>
        private void SetCurrentImageScaleMenuItem(MenuItem currentMenuItem)
        {
            normalImageMenuItem.IsChecked = false;
            bestFitMenuItem.IsChecked = false;
            fitToWidthMenuItem.IsChecked = false;
            fitToHeightMenuItem.IsChecked = false;
            pixelToPixelMenuItem.IsChecked = false;
            scaleMenuItem.IsChecked = false;
            scale25MenuItem.IsChecked = false;
            scale50MenuItem.IsChecked = false;
            scale100MenuItem.IsChecked = false;
            scale200MenuItem.IsChecked = false;
            scale400MenuItem.IsChecked = false;

            currentMenuItem.IsChecked = true;
        }

        /// <summary>
        /// Updates the image viewer, which shows recognized image, and related controls.
        /// </summary>
        /// <param name="aligned">Determines whether recognized image must be aligned.</param>
        private void UpdateRecognizedImageViewer(bool aligned)
        {
            string matchingTemplateNameLabelText;

            // get the current image
            VintasoftImage currentImage = filledImageViewer.Image;
            // if current image is present
            if (currentImage != null)
            {
                ImageRecognitionResult imageRecognitionResult = null;
                // if current image has recognition result
                if (_filledImageToRecognitionResultMap.TryGetValue(currentImage, out imageRecognitionResult))
                {
                    // if recognition result is NOT empty
                    if (imageRecognitionResult != null)
                    {
                        VintasoftImage newImage;
                        // if current image should be aligned
                        if (aligned)
                        {
                            // if aligned image is not set
                            if (imageRecognitionResult.AlignedImage == null)
                            {
                                // generate and store aligned image
                                TemplateAligningCommand command = new TemplateAligningCommand(imageRecognitionResult.CompareResult);
                                imageRecognitionResult.AlignedImage = command.Execute(currentImage);
                            }
                            // set aligned image as new image
                            newImage = imageRecognitionResult.AlignedImage;
                            // set empty transform
                            _recognizedFieldViewerTool.SetTransform(null, Resolution.Empty);
                        }
                        // if current image should NOT be aligned
                        else
                        {
                            // if aligned image is present
                            if (imageRecognitionResult.AlignedImage != null)
                            {
                                // dispose the aligned image
                                imageRecognitionResult.AlignedImage.Dispose();
                                imageRecognitionResult.AlignedImage = null;
                            }
                            // set current image as new image
                            newImage = currentImage;
                            // set additional transform to match the unaligned image
                            _recognizedFieldViewerTool.SetTransform(
                                imageRecognitionResult.CompareResult.TransformMatrix,
                                imageRecognitionResult.CompareResult.SourceImprint.ImageSize.Resolution);
                        }

                        // set new image in the image viewer
                        recognizedImageViewer.Images.Insert(0, newImage);
                        if (recognizedImageViewer.Images.Count > 1)
                            recognizedImageViewer.Images.RemoveAt(1);

                        matchingTemplateNameLabelText = imageRecognitionResult.TemplateName;
                        recognitionResultTextBox.Text = imageRecognitionResult.ToString();

                        _recognizedFieldViewerTool.FieldViewCollection.Clear();
                        if (imageRecognitionResult.RecognizedPageView != null)
                        {
                            // set settings for form field view
                            _formFieldViewSettings.SetSettings(imageRecognitionResult.RecognizedPageView);
                            _recognizedFieldViewerTool.FieldViewCollection.Add(imageRecognitionResult.RecognizedPageView);
                        }

                    }
                    else
                    {
                        while (recognizedImageViewer.Images.Count > 0)
                            recognizedImageViewer.Images.RemoveAt(0);
                        matchingTemplateNameLabelText = "<matching template not found>";
                        recognitionResultTextBox.Clear();
                        _recognizedFieldViewerTool.FieldViewCollection.Clear();
                    }
                }
                else
                {
                    while (recognizedImageViewer.Images.Count > 0)
                        recognizedImageViewer.Images.RemoveAt(0);
                    matchingTemplateNameLabelText = "<not recognized yet>";
                    recognitionResultTextBox.Clear();
                    _recognizedFieldViewerTool.FieldViewCollection.Clear();
                }
            }
            else
            {
                while (recognizedImageViewer.Images.Count > 0)
                    recognizedImageViewer.Images.RemoveAt(0);
                matchingTemplateNameLabelText = "<matching template's name>";
                recognitionResultTextBox.Clear();
                _recognizedFieldViewerTool.FieldViewCollection.Clear();
            }

            matchingTemplateNameLabel.Content = string.Format("Matching template's name: {0}", matchingTemplateNameLabelText);
            recognitionResultLabel.Content = string.Format("Matching template's name: {0}", matchingTemplateNameLabelText);
        }

        #endregion


        #region Image collection

        /// <summary>
        /// Image collection of image viewer is changed.
        /// </summary>
        private void Images_ImageCollectionChanged(object sender, ImageCollectionChangeEventArgs e)
        {
            switch (e.Action)
            {
                case ImageCollectionChangeAction.Clear:
                    VintasoftImage[] keys = new VintasoftImage[_filledImageToRecognitionResultMap.Count];
                    _filledImageToRecognitionResultMap.Keys.CopyTo(keys, 0);
                    for (int i = 0; i < keys.Length; i++)
                    {
                        ClearImageRecognitionResult(keys[i]);

                        keys[i] = null;
                    }
                    break;

                case ImageCollectionChangeAction.RemoveImages:
                    VintasoftImage[] images = e.Images;
                    foreach (VintasoftImage filledImage in images)
                    {
                        ClearImageRecognitionResult(filledImage);
                    }
                    break;
            }

            UpdateUI();
        }

        /// <summary>
        /// Clears the recognition result of specified image.
        /// </summary>
        /// <param name="filledImage">The image, whose result must be cleared.</param>
        private void ClearImageRecognitionResult(VintasoftImage filledImage)
        {
            ImageRecognitionResult recognitionResult = null;
            if (_filledImageToRecognitionResultMap.TryGetValue(filledImage, out recognitionResult))
            {
                if (recognitionResult != null)
                {
                    VintasoftImage alignedImage = recognitionResult.AlignedImage;
                    recognitionResult.AlignedImage = null;
                    if (alignedImage != null)
                        alignedImage.Dispose();

                    _filledImageToRecognitionResultMap.Remove(filledImage);
                }
            }
        }

        #endregion


        #region File manipulation

        /// <summary>
        /// Opens image files and adds to the image collection of image viewer.
        /// Binarization is applied if necessary.
        /// </summary>
        /// <param name="append">Determines whether to append images to existing images in the collection.
        /// </param>
        private void OpenFiles(bool append)
        {
            if (_openFileDialog.ShowDialog(this).Value)
            {
                ImageCollection viewerImages = filledImageViewer.Images;

                if (!append)
                    viewerImages.ClearAndDisposeItems();

                bool canceled = false;
                bool applyForAll = false;
                string[] filenames = _openFileDialog.FileNames;
                // for each selected file
                foreach (string filename in filenames)
                {
                    // temporary image collection for all images in current file
                    ImageCollection filledImages = new ImageCollection();
                    DocumentPasswordWindow.EnableAuthentication(filledImages);
                    try
                    {
                        bool imageParsingError = false;
                        try
                        {
                            filledImages.Add(filename);
                        }
                        catch (Exception ex)
                        {
                            DemosTools.ShowErrorMessage(ex, filename);
                            imageParsingError = true;
                        }
                        if (!imageParsingError)
                        {
                            foreach (VintasoftImage image in filledImages)
                            {
                                // if binarization is canceled
                                if (canceled)
                                {
                                    image.Dispose();
                                    continue;
                                }

                                if (image.PixelFormat != Vintasoft.Imaging.PixelFormat.BlackWhite)
                                {
                                    ProcessingCommandBase processingCommand = null;
                                    // if settings shall be applied for all remaining images or
                                    // settings are approved
                                    if (applyForAll)
                                    {
                                        image.RenderingSettings = _renderingSettings;
                                        processingCommand = _binarizeCommand;
                                    }
                                    else
                                    {
                                        // create binarization form
                                        ImageBinarizationWindow binarizationForm = new ImageBinarizationWindow(
                                            _binarizeCommand, _renderingSettings);
                                        binarizationForm.Owner = this;
                                        if (binarizationForm.ShowDialog(image))
                                        {
                                            image.RenderingSettings = binarizationForm.GetRenderingSettings();
                                            processingCommand = binarizationForm.GetProcessingCommand();

                                            if (binarizationForm.ApplyForAll)
                                                applyForAll = true;
                                        }

                                        // if binarization is canceled
                                        if (binarizationForm.Cancel || binarizationForm.Skip)
                                        {
                                            canceled = binarizationForm.Cancel;
                                            image.Dispose();
                                            continue;
                                        }
                                    }
                                    // if processing command is set
                                    if (processingCommand != null)
                                    {
                                        bool processingError = false;
                                        try
                                        {
                                            processingCommand.ExecuteInPlace(image);
                                        }
                                        catch (Exception ex)
                                        {
                                            DemosTools.ShowErrorMessage(ex);
                                            image.Dispose();
                                            processingError = true;
                                        }
                                        if (processingError)
                                            continue;
                                    }
                                }

                                viewerImages.Add(image);
                            }
                            filledImages.Clear();

                            if (canceled)
                                break;
                        }
                    }
                    finally
                    {
                        DocumentPasswordWindow.DisableAuthentication(filledImages);
                    }
                }

                UpdateUI();
            }
        }

        #endregion


        #region Forms recognition

        /// <summary>
        /// Recognizes all images asynchronously.
        /// </summary>
        private void RecognizeAllAsync()
        {
            if (!CheckTemplateImages())
                return;

            tabControl1.SelectedItem = recognitionLogTabPage;
            recognitionLogTextBox.Clear();
            // show progress bar
            recognitionProgressBar.Value = 0;
            recognitionProgressBar.Visibility = Visibility.Visible;

            LogWriteLine(string.Format("Recognition Started... (MaxThreads = {0})", _formRecognitionManager.MaxThreads));
            LogWriteLine();

            // create form recognition task for all filled images
            FormRecognitionTask task = new FormRecognitionTask(sourceThumbnailViewer.Images.ToArray());

            // subscribe to task events
            task.ImageRecognitionStarted += new EventHandler<ImageEventArgs>(FormRecognitionTask_RecognitionStarted);
            task.ImageRecognitionFinished += new EventHandler<FormRecognitionFinishedEventArgs>(FormRecognitionTask_RecognitionFinished);
            task.ImageRecognitionError += new EventHandler<FormRecognitionErrorEventArgs>(FormRecognitionTask_RecognitionError);
            task.Progress += new EventHandler<ProgressEventArgs>(FormRecognitionTask_RecognitionProgress);
            task.Started += new EventHandler(FormRecognitionTask_Started);
            task.Finished += new EventHandler(FormRecognitionTask_Finished);

            // reset and start timer
            _recognitionStopwatch.Reset();
            _recognitionStopwatch.Start();

            // compenate template images background, if need
            if (_templateEditorWindow.AutomaticallyImageBackgroundCompensation)
                CompensateTemplateImagesBackground();

            // start form recognition task
            _formRecognitionManager.RecognizeAsync(task);
        }

        /// <summary>
        /// Next image recognition raised an exception.
        /// </summary>
        private void FormRecognitionTask_RecognitionError(object sender, FormRecognitionErrorEventArgs e)
        {
            // suspend the stopwatch
            bool stopWatchIsRunning = _recognitionStopwatch.IsRunning;
            if (stopWatchIsRunning)
                _recognitionStopwatch.Stop();

            // show error message
            DemosTools.ShowErrorMessage(e.Exception, GetImageName(e.Image));
            LogWriteLine(string.Format("Image recognition failed: {0}:", GetImageName(e.Image)));
            LogWriteLine();

            // resume the stopwatch
            if (stopWatchIsRunning)
                _recognitionStopwatch.Start();
        }

        /// <summary>
        /// Recognizes the current image synchronously.
        /// </summary>
        private void RecognizeCurrent()
        {
            if (!CheckTemplateImages())
                return;

            tabControl1.SelectedItem = recognitionLogTabPage;
            recognitionLogTextBox.Clear();
            // show progress bar
            recognitionProgressBar.Value = 0;
            recognitionProgressBar.Visibility = Visibility.Visible;

            // subscribe to FormRecognitionManager events
            _formRecognitionManager.ImageRecognitionStarted += new EventHandler<ImageEventArgs>(FormRecognitionTask_RecognitionStarted);
            _formRecognitionManager.ImageRecognitionFinished += new EventHandler<FormRecognitionFinishedEventArgs>(FormRecognitionTask_RecognitionFinished);
            _formRecognitionManager.ImageRecognitionError += new EventHandler<FormRecognitionErrorEventArgs>(FormRecognitionTask_RecognitionError);
            _formRecognitionManager.RecognitionProgress += new EventHandler<ProgressEventArgs>(FormRecognitionTask_RecognitionProgress);
            List<FormRecognitionResult> recognitionResults = new List<FormRecognitionResult>();
            try
            {
                // reset and start timer
                _recognitionStopwatch.Reset();
                _recognitionStopwatch.Start();

                // compenate template images background, if need
                if (_templateEditorWindow.AutomaticallyImageBackgroundCompensation)
                    CompensateTemplateImagesBackground();

                // execute recognition of current image
                recognitionResults.Add(_formRecognitionManager.Recognize(filledImageViewer.Image));
            }
            finally
            {
                // unsubscribe from FormRecognitionManager events
                _formRecognitionManager.RecognitionProgress -= new EventHandler<ProgressEventArgs>(FormRecognitionTask_RecognitionProgress);
                _formRecognitionManager.ImageRecognitionError -= new EventHandler<FormRecognitionErrorEventArgs>(FormRecognitionTask_RecognitionError);
                _formRecognitionManager.ImageRecognitionFinished -= new EventHandler<FormRecognitionFinishedEventArgs>(FormRecognitionTask_RecognitionFinished);
                _formRecognitionManager.ImageRecognitionStarted -= new EventHandler<ImageEventArgs>(FormRecognitionTask_RecognitionStarted);
            }

            // show recognition results
            RecognitionFinishedHandler(recognitionResults.ToArray());
        }

        /// <summary>
        /// Compensates template images background.
        /// </summary>
        private void CompensateTemplateImagesBackground()
        {
            LogWrite("Template images background compensation...");
            int imageCount = _formRecognitionManager.TemplateImages.Count;
            for (int i = 0; i < imageCount; i++)
            {
                VintasoftImage templateImage = _formRecognitionManager.TemplateImages[i];
                FormPageTemplate pageTemplate = _formRecognitionManager.FormTemplates.GetPageTemplate(templateImage);
                _formRecognitionManager.FormTemplates.CompensateTemplateImageBackground(templateImage, pageTemplate);
            }
            LogWriteLine("done.");
            LogWriteLine();
        }

        /// <summary>
        /// Checks the template image collection.
        /// </summary>
        private bool CheckTemplateImages()
        {
            ImageCollection templateImages = _formRecognitionManager.FormTemplates.TemplateImages;
            if (templateImages.Count == 0)
            {
                DemosTools.ShowInfoMessage("Templates are not set. Use 'File'->'Manage Templates...' to set templates.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the name of the image.
        /// </summary>
        /// <param name="image">The image.</param>
        private string GetImageName(VintasoftImage image)
        {
            return string.Format("{0}, page {1}",
                Path.GetFileName(image.SourceInfo.Filename),
                image.SourceInfo.PageIndex + 1);
        }

        /// <summary>
        /// Asynchronous form recognition task is started (thread safe handler).
        /// </summary>
        void FormRecognitionTask_Started(object sender, EventArgs e)
        {
            IsRecognizeAllAsync = true;
        }

        /// <summary>
        /// Asynchronous form recognition task is finished (thread safe handler).
        /// </summary>
        private void FormRecognitionTask_Finished(object sender, EventArgs e)
        {
            FormRecognitionTask task = (FormRecognitionTask)sender;
            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.Invoke(new RecognitionFinishedHandlerDelegate(RecognitionFinishedHandler), (object)task.Results);
            else
                RecognitionFinishedHandler(task.Results);
            IsRecognizeAllAsync = false;
        }

        /// <summary>
        /// Asynchronous form recognition task is finished (not thread safe handler).
        /// </summary>
        private void RecognitionFinishedHandler(FormRecognitionResult[] results)
        {
            _recognitionStopwatch.Stop();

            // store recognition results of recognized images in a dictionary
            foreach (FormRecognitionResult recognitionResult in results)
            {
                if (recognitionResult != null)
                    StoreRecognitionResult(recognitionResult);
            }

            // whether alinging of recognized images is required
            bool alignRecognizedImages = alignImagesByTemplateMenuItem.IsChecked;

            recognitionProgressBar.Visibility = Visibility.Collapsed;
            // update the viewer of recognized images
            UpdateRecognizedImageViewer(alignRecognizedImages);

            tabControl1.SelectedItem = recognizedImageTabItem;

            LogWriteLine(string.Format("Recognition finished. ({0} ms)",
                _recognitionStopwatch.ElapsedMilliseconds));
        }


        /// <summary>
        /// Next image recognition is started.
        /// </summary>
        private void FormRecognitionTask_RecognitionStarted(object sender, ImageEventArgs e)
        {
            LogWriteLine(string.Format("Image recognition started: {0}...{1}", GetImageName(e.Image), Environment.NewLine));
        }


        /// <summary>
        /// Next image recognition is finished (thread safe handler).
        /// </summary>
        private void FormRecognitionTask_RecognitionFinished(object sender, FormRecognitionFinishedEventArgs e)
        {
            if (e.RecognitionResult == null)
                return;

            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.Invoke(new FormRecognitionFinishedHandlerDelegate(FormRecognitionFinishedHandler), e);
            else
                FormRecognitionFinishedHandler(e);
        }

        /// <summary>
        /// Next image recognition is finished (not thread safe handler).
        /// </summary>
        private void FormRecognitionFinishedHandler(FormRecognitionFinishedEventArgs e)
        {
            FormRecognitionResult recognitionResult = e.RecognitionResult;
            FormPage recognizedPage = recognitionResult.RecognizedPage;
            LogWriteLine(string.Format("Image recognition finished: {0}:", GetImageName(e.RecognitionResult.TemplateMatchingResult.Image)));
            // if matching page was found
            if (recognizedPage != null)
            {
                // get the page template
                FormPageTemplate pageTemplate = recognizedPage.FieldTemplate as FormPageTemplate;

                // output template info

                LogWriteLine(string.Format(
                    "\tMatching template is found successfully (\"{0}\").",
                    pageTemplate.Name));

                if (pageTemplate.Items.Count == 0)
                {
                    LogWriteLine("\tMatching template does not contain fields.");
                }
                else
                {
                    LogWriteLine(string.Format(
                        "\tForm fields are recognized successfully (count={0}).",
                        recognizedPage.Items.Count));
                }
            }
            else
            {
                LogWriteLine("\tTemplate was not found for the image.");
            }
            LogWriteLine();
        }


        /// <summary>
        /// Progress of recognition process is changed.
        /// </summary>
        private void FormRecognitionTask_RecognitionProgress(object sender, ProgressEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.Invoke(new SetProgressValueDelegate(SetProgressValue), e.Progress);
            else
                SetProgressValue(e.Progress);
        }

        /// <summary>
        /// Sets the progress of recognition process.
        /// </summary>
        private void SetProgressValue(int value)
        {
            recognitionProgressBar.Value = value;
        }


        /// <summary>
        /// Stores the recognition result of current recognized form in a dictionary.
        /// </summary>
        /// <param name="recognitionResult">The recognition result of current recognized form.</param>
        private void StoreRecognitionResult(FormRecognitionResult recognitionResult)
        {
            TemplateMatchingResult templateMatchingResult = recognitionResult.TemplateMatchingResult;
            // the result of image comparison with the template
            ImageImprintCompareResult imageCompareResult = templateMatchingResult.ImageCompareResult;
            // the filled image
            VintasoftImage filledImage = templateMatchingResult.Image;
            // an instance that will store results of image recognition
            ImageRecognitionResult imageRecognitionResult = null;
            // if this image has no image recognition results stored yet
            if (!_filledImageToRecognitionResultMap.TryGetValue(filledImage, out imageRecognitionResult) ||
                imageRecognitionResult == null)
            {
                // create an instance of recognition results
                imageRecognitionResult = new ImageRecognitionResult();
                // add to the dictionary
                _filledImageToRecognitionResultMap[filledImage] = imageRecognitionResult;
            }

            // if result is not reliable
            if (!imageCompareResult.IsReliable)
            {
                // if there is aligned image stored
                if (imageRecognitionResult != null && imageRecognitionResult.AlignedImage != null)
                    // dispose it
                    imageRecognitionResult.AlignedImage.Dispose();
                // set dictionary value to null that denotes unreliable result
                _filledImageToRecognitionResultMap[filledImage] = null;
            }
            else
            {
                // get the template page of the template image
                FormPageTemplate matchingTemplatePage =
                    _formRecognitionManager.FormTemplates.GetPageTemplate(templateMatchingResult.TemplateImage);
                // store the name of the template page
                imageRecognitionResult.TemplateName = matchingTemplatePage.Name;
                // store the result of comparison
                imageRecognitionResult.CompareResult = imageCompareResult;
                // get the recognized page
                FormPage recognizedPage = recognitionResult.RecognizedPage;

                // create a view for the recognized page
                WpfFormFieldView recognizedPageView = WpfFormFieldViewFactory.CreateView(recognizedPage);
                // get as a group
                WpfFormFieldGroupView recognizedFieldGroupView = recognizedPageView as WpfFormFieldGroupView;
                // get and store a mapping from nested to containing views
                imageRecognitionResult.ParentsTable = recognizedFieldGroupView.ViewItems.CreateParentTable();
                // set appearance of the view
                SetRecognizedFieldAppearance(recognizedPageView);
                // store the view
                imageRecognitionResult.RecognizedPageView = recognizedPageView;
            }
        }


        /// <summary>
        /// Appends message to a log.
        /// </summary>
        private void LogWrite(string text)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(new AddTextToLogDelegate(AddTextToLog), text);
            }
            else
            {
                AddTextToLog(text);
            }
        }

        private void AddTextToLog(string text)
        {
            recognitionLogTextBox.AppendText(text);
            recognitionLogTextBox.ScrollToEnd();
        }

        /// <summary>
        /// Appends new line to a log.
        /// </summary>
        private void LogWriteLine()
        {
            LogWrite(Environment.NewLine);
        }

        /// <summary>
        /// Appends message and new line to a log.
        /// </summary>
        private void LogWriteLine(string text)
        {
            LogWrite(text + Environment.NewLine);
        }

        /// <summary>
        /// Creates the
        /// <see cref="Vintasoft.Imaging.FormsProcessing.TemplateMatching.ImageImprintGeneratorCommand"/>.
        /// </summary>
        /// <param name="imagePreprocessing">The image preprocessing.</param>
        /// <returns>The image imprint generator command.</returns>
        private ImageImprintGeneratorCommand CreateImageImprintGenerator(
            ProcessingCommandBase imagePreprocessing)
        {
            // for each recognizer
            foreach (KeyZoneRecognizerCommand recognizerCommand in _selectedKeyZoneRecognizerCommands)
            {
                // set image preprocessing commands
                recognizerCommand.ImagePreprocessing = imagePreprocessing;
            }

            ImageImprintGeneratorCommand imprintGenerator = new ImageImprintGeneratorCommand(_selectedKeyZoneRecognizerCommands);

            // return generator
            return imprintGenerator;
        }

        /// <summary>
        /// Edits the properties of
        /// <see cref="Vintasoft.Imaging.FormsProcessing.TemplateMatching.KeyZoneRecognizerCommand"/>.
        /// </summary>
        private void imageImprintGeneratorPreprocessingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProcessingCommandBase[] preprocessingCommands = null;
            if (_preprocessingCommandKeyZoneRecognizer != null)
            {
                if (_preprocessingCommandKeyZoneRecognizer is CompositeCommand)
                {
                    CompositeCommand compositeCommand = (CompositeCommand)_preprocessingCommandKeyZoneRecognizer;
                    preprocessingCommands = compositeCommand.GetCommands();
                }
                else
                {
                    preprocessingCommands = new ProcessingCommandBase[] {
                        _preprocessingCommandKeyZoneRecognizer
                    };
                }
            }
#if REMOVE_DOCCLEANUP_PLUGIN
            ProcessingCommandBase[] availableProcessingCommands = new ProcessingCommandBase[] {
                    new DilateCommand(),
                    new ErodeCommand(),
            };
#else
            ProcessingCommandBase[] availableProcessingCommands = new ProcessingCommandBase[] {
                    new DespeckleCommand(),
                    new HalftoneRemovalCommand(),
                    new DilateCommand(),
                    new ErodeCommand(),
                    new BorderClearCommand()
            };
#endif
            ImageProcessingWindow window = new ImageProcessingWindow(
                    filledImageViewer.Image,
                    availableProcessingCommands,
                    preprocessingCommands);

            window.Title = "Preprocessing of image imprint generator";
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                preprocessingCommands = window.SelectedCommands;
                ProcessingCommandBase command = null;
                if (preprocessingCommands != null)
                {
                    if (preprocessingCommands.Length == 1)
                        command = preprocessingCommands[0];
                    else
                        command = new CompositeCommand(preprocessingCommands);
                }

                _preprocessingCommandKeyZoneRecognizer = command;

                _formRecognitionManager.TemplateMatching.ImageImprintGenerator =
                    CreateImageImprintGenerator(command);
            }
        }

        #endregion


        #region Form field views

        /// <summary>
        /// Sets recognized field appearance.
        /// </summary>
        /// <param name="fieldView">Recognized form field view.</param>
        private void SetRecognizedFieldAppearance(WpfFormFieldView fieldView)
        {
            if (fieldView is WpfFormFieldGroupView)
                foreach (WpfFormFieldView nestedFieldView in ((WpfFormFieldGroupView)fieldView).ViewItems)
                    SetRecognizedFieldAppearance(nestedFieldView);

            if (fieldView.Pen != null)
            {
                SolidColorBrush brush = ((SolidColorBrush)fieldView.Pen.Brush);
                brush.Color = Color.FromArgb(brush.Color.A, 255, 0, 0);
            }
        }

        /// <summary>
        /// Mouse has entered a recognized form field view.
        /// </summary>
        private void recognizedFieldViewerTool_FieldViewMouseEnter(object sender, WpfFormFieldViewEventArgs e)
        {
            WpfFormFieldView targetFieldView = GetTargetView(e.FormFieldView);
            if (targetFieldView != null)
            {
                if (targetFieldView.Pen != null)
                {
                    SolidColorBrush brush = (targetFieldView.Pen.Brush as SolidColorBrush);
                    _currentRecognizedFieldViewPenColor = brush.Color;
                    targetFieldView.Pen.Brush = new SolidColorBrush(Color.FromArgb(255, _currentRecognizedFieldViewPenColor.R,
                        _currentRecognizedFieldViewPenColor.G, _currentRecognizedFieldViewPenColor.B));
                    WpfFormFieldViewerTool formFieldViewerTool = sender as WpfFormFieldViewerTool;
                    formFieldViewerTool.InvalidateItem(targetFieldView);
                }
            }
        }

        /// <summary>
        /// Mouse has left a recognized form field view.
        /// </summary>
        private void recognizedFieldViewerTool_FieldViewMouseLeave(object sender, WpfFormFieldViewEventArgs e)
        {
            WpfFormFieldView targetFieldView = GetTargetView(e.FormFieldView);
            if (targetFieldView != null)
            {
                if (targetFieldView.Pen != null)
                {
                    targetFieldView.Pen.Brush = new SolidColorBrush(_currentRecognizedFieldViewPenColor);
                    WpfFormFieldViewerTool formFieldViewerTool = sender as WpfFormFieldViewerTool;
                    formFieldViewerTool.InvalidateItem(targetFieldView);
                }
            }
        }

        /// <summary>
        /// Shows properties of clicked recognized fields.
        /// </summary>
        void recognizedFieldViewerTool_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WpfFormFieldViewerTool fieldViewerTool = sender as WpfFormFieldViewerTool;
            if (fieldViewerTool != null)
            {
                Point autoScrollPosition = recognizedImageViewer.ViewerState.AutoScrollPosition;
                Point position = e.GetPosition(recognizedImageViewer);
                position.Offset(autoScrollPosition.X, autoScrollPosition.Y);
                // find field view by cursor position
                WpfFormFieldView currentFieldView = fieldViewerTool.FindFieldView(position.X, position.Y);
                // if field is found
                if (currentFieldView != null)
                {
                    // get the field view
                    WpfFormFieldView targetFieldView = GetTargetView(currentFieldView);
                    // if field view is found
                    if (targetFieldView != null)
                    {
                        FormField field = targetFieldView.Field;
                        // create a title for window
                        string title = string.Format("{0} : {1}", field.Name, field.GetType().Name);
                        // show window with field properties
                        ShowProperties(field, title);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a recognized form field view that should be highlighted
        /// when mouse enters the field view.
        /// </summary>
        /// <param name="view">Current hovered recognized form field view.</param>
        private WpfFormFieldView GetTargetView(WpfFormFieldView view)
        {
            if (view.FieldTemplate is FormPageTemplate)
                return null;

            ImageRecognitionResult currentRecognitionResult = null;
            _filledImageToRecognitionResultMap.TryGetValue(filledImageViewer.Image, out currentRecognitionResult);

            WpfFormFieldView parentFieldView = null;
            if (currentRecognitionResult.ParentsTable.TryGetValue(view, out parentFieldView))
            {
                if (parentFieldView.FieldTemplate is OmrFieldTemplateTable)
                    return parentFieldView;
            }
            return view;
        }

        /// <summary>
        /// Shows a form with properties of specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="formTitle">The title of the form.</param>
        private void ShowProperties(object obj, string formTitle)
        {
            PropertyGridWindow propertyGridForm = new PropertyGridWindow(obj, formTitle);
            propertyGridForm.ShowDialog();
        }

        #endregion


        #region View Rotation

        /// <summary>
        /// Rotates images in image viewers and thumbnail viewer by 90 degrees clockwise.
        /// </summary>
        private void RotateViewClockwise()
        {
            if (filledImageViewer.ImageRotationAngle != 270)
            {
                filledImageViewer.ImageRotationAngle += 90;
                recognizedImageViewer.ImageRotationAngle += 90;
                sourceThumbnailViewer.ImageRotationAngle += 90;
            }
            else
            {
                filledImageViewer.ImageRotationAngle = 0;
                recognizedImageViewer.ImageRotationAngle = 0;
                sourceThumbnailViewer.ImageRotationAngle = 0;
            }
        }

        /// <summary>
        /// Rotates images in image viewers and thumbnail viewer by 90 degrees counterclockwise.
        /// </summary>
        private void RotateViewCounterClockwise()
        {
            if (filledImageViewer.ImageRotationAngle != 0)
            {
                filledImageViewer.ImageRotationAngle -= 90;
                recognizedImageViewer.ImageRotationAngle -= 90;
                sourceThumbnailViewer.ImageRotationAngle -= 90;
            }
            else
            {
                filledImageViewer.ImageRotationAngle = 270;
                recognizedImageViewer.ImageRotationAngle = 270;
                sourceThumbnailViewer.ImageRotationAngle = 270;
            }
        }

        #endregion


        #region Hot keys

        /// <summary>
        /// Handles the CanExecute event of openTemplateCommandBinding object.
        /// </summary>
        private void openTemplateCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = openTemplateMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of openFilledImagesCommandBinding object.
        /// </summary>
        private void openFilledImagesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = openFilledImagesMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of addFilledImagesCommandBinding object.
        /// </summary>
        private void addFilledImagesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = addFilledImagesMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of closeAllCommandBinding object.
        /// </summary>
        private void closeAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = closeAllMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of recognizeCurrentPageCommandBinding object.
        /// </summary>
        private void recognizeCurrentPageCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = recognizeCurrentPageMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of recognizeAllPagesCommandBinding object.
        /// </summary>
        private void recognizeAllPagesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = recognizeAllPagesMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of rotateClockwiseCommandBinding object.
        /// </summary>
        private void rotateClockwiseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rotateClockwiseMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of rotateCounterclockwiseCommandBinding object.
        /// </summary>
        private void rotateCounterclockwiseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rotateCounterclockwiseMenuItem.IsEnabled;
        }

        #endregion

        #endregion



        #region Delegates

        delegate void FormRecognitionFinishedHandlerDelegate(FormRecognitionFinishedEventArgs e);

        delegate void RecognitionFinishedHandlerDelegate(FormRecognitionResult[] results);

        delegate void SetProgressValueDelegate(int value);

        delegate void AddTextToLogDelegate(string text);

        delegate void UpdateUIDelegate();

        #endregion

    }
}
