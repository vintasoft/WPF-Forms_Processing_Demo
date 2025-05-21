using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.FormsProcessing.FormRecognition;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Ocr;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Ocr.Wpf.UI;
#endif
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Omr;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Omr.Wpf.UI;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI;
using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI.VisualTools;
using Vintasoft.Imaging.ImageProcessing;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Ocr;
using Vintasoft.Imaging.Ocr.Tesseract;
#if !REMOVE_OCR_ML_ASSEMBLY
using Vintasoft.Imaging.Ocr.ML.HandwrittenDigits;
#endif
#endif
#if !REMOVE_BARCODE_SDK
using Vintasoft.Barcode;
#endif

using WpfDemosCommonCode;
using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;
using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A window that allows to edit form templates.
    /// </summary>
    public partial class TemplateEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// The form template manager.
        /// </summary>
        FormTemplateManager _templateManager;

        /// <summary>
        /// The current image processing command that is used for image binarization.
        /// </summary>
        ChangePixelFormatToBlackWhiteCommand _binarizeCommand;

        /// <summary>
        /// The current rendering settings, which are used for vector image rendering.
        /// </summary>
        RenderingSettings _renderingSettings;

        /// <summary>
        /// The form field template editor tool.
        /// </summary>
        WpfFormFieldTemplateEditorTool _fieldTemplateEditorTool;

        /// <summary>
        /// Dictionary that binds template images and tree view nodes.
        /// </summary>
        Dictionary<VintasoftImage, TreeViewItem> _imagesToTemplateTreeNodes =
            new Dictionary<VintasoftImage, TreeViewItem>();

        /// <summary>
        /// Dictionary that binds form field templates and tree view nodes.
        /// </summary>
        Dictionary<FormFieldTemplate, TreeViewItem> _formFieldTemplateToTreeNodes =
            new Dictionary<FormFieldTemplate, TreeViewItem>();

        /// <summary>
        /// Root node of the tree view.
        /// </summary>
        TreeViewItem _templatesRootNode;

        /// <summary>
        /// Current form field template view in internal "copy" buffer (field template view to copy).
        /// </summary>
        WpfFormFieldTemplateView _fieldTemplateViewCopy = null;

        OpenFileDialog _openImageFileDialog = new OpenFileDialog();

        bool _isSelectingTreeViewItem = false;


        /// <summary>
        /// A value indicating whether text recognition must be executed in multiple threads.
        /// </summary>
        bool _recognizeTextInMultipleThreads = false;

        /// <summary>
        /// The maximum count of threads, which can be used for text recognition.
        /// </summary>
        int _maxOcrThreads = Environment.ProcessorCount;

        /// <summary>
        /// The directory, where Tesseract5.Vintasoft.xXX.dll is located.
        /// </summary>
        string _tesseractOcrDllDirectory = string.Empty;

        /// <summary>
        /// The table inital size.
        /// </summary>
        Size _tableInitalSize = new Size(5, 5);

        Microsoft.Win32.OpenFileDialog _openDocumentDialog = new Microsoft.Win32.OpenFileDialog();

        Microsoft.Win32.OpenFileDialog _openImageOrDocumentDialog = new Microsoft.Win32.OpenFileDialog();


        /// <summary>
        /// A value indicating whether information about the field templates automatic detection was shown.
        /// </summary>
        static bool IsInfoAboutFieldTemplatesAutomaticDetectionShown = false;


        #region File Dialog Filters

        /// <summary>
        /// File extensions for template images.
        /// </summary>
        static readonly string ImageFileExtensions = "*.bmp;*.tif;*.tiff;*.png;*.jpg;*.jpeg;*.pdf;*.xps";

        /// <summary>
        /// File extensions for document templates.
        /// </summary>
        static readonly string FormDocumentTemplateExtensions = "*.fdt";

        /// <summary>
        /// File extensions for page templates.
        /// </summary>
        static readonly string FormPageTemplateExtensions = "*.fpt";

        /// <summary>
        /// File dialog filter for all template images.
        /// </summary>
        static readonly string ImageFilesFilter;

        /// <summary>
        /// File dialog filter for all document templates.
        /// </summary>
        static readonly string FormDocumentTemplatesFilter;

        /// <summary>
        /// File dialog filter for all page templates.
        /// </summary>
        static readonly string FormPageTemplatesFilter;

        /// <summary>
        /// File dialog filter for all supported template files.
        /// </summary>
        static readonly string AllSupportedTemplateFilesFilter;

        #endregion


        #region Hot keys

        public static RoutedCommand _cutCommand = new RoutedCommand();
        public static RoutedCommand _copyCommand = new RoutedCommand();
        public static RoutedCommand _pasteCommand = new RoutedCommand();
        public static RoutedCommand _deleteAllCommand = new RoutedCommand();

        #endregion

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes the <see cref="TemplateEditorWindow"/> class.
        /// </summary>
        static TemplateEditorWindow()
        {
            ImageFilesFilter = string.Format(
                "Image Files|{0}",
                ImageFileExtensions);
            FormDocumentTemplatesFilter = string.Format(
                "Form Document Template Files|{0}",
                FormDocumentTemplateExtensions);
            FormPageTemplatesFilter = string.Format(
                "Form Page Template Files|{0}",
                FormPageTemplateExtensions);
            AllSupportedTemplateFilesFilter = string.Format(
                "All Template Files|{0};{1}",
                ImageFileExtensions,
                FormDocumentTemplateExtensions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEditorWindow"/> class.
        /// </summary>
        public TemplateEditorWindow()
        {
            InitializeComponent();

            imageViewer1.InputGestureCopy = null;
            imageViewer1.InputGestureCut = null;
            imageViewer1.InputGestureDelete = null;
            imageViewer1.InputGestureInsert = null;

            imageViewerToolBar.ImageViewer = imageViewer1;
            thumbnailViewer1.MasterViewer = imageViewer1;

            propertyGridGroupBox.Header = "<No selected object>";

            int count = imageViewerToolBar.Items.Count;
            RegisterName("omrRectangleButton", imageViewerToolBar.Items[count - 6]);
            RegisterName("omrEllipseButton", imageViewerToolBar.Items[count - 5]);
            RegisterName("tableOfOmrRectanglesButton", imageViewerToolBar.Items[count - 4]);
            RegisterName("tableOfOmrEllipsesButton", imageViewerToolBar.Items[count - 3]);
            RegisterName("ocrButton", imageViewerToolBar.Items[count - 2]);
            RegisterName("barcodeButton", imageViewerToolBar.Items[count - 1]);

#if REMOVE_OCR_PLUGIN
            ocrMenuItem.Visibility = Visibility.Collapsed;
            foreach(object item in imageViewerToolBar.Items)
            {
                UIElement addOcrFieldButton = (UIElement)item;
                if (addOcrFieldButton.Uid == "addOcrFieldMenuItem")
                {
                    addOcrFieldButton.Visibility = Visibility.Collapsed;
                    break;
                }
            }
            addOCRFieldMenuItem.Visibility = Visibility.Collapsed;
            defaultOcrSettingsMenuItem.Visibility = Visibility.Collapsed;
#endif

            automaticallyCompensateForAllPagesMenuItem.Checked += new RoutedEventHandler(automaticallyCompensateForAllPagesMenuItem_CheckedChanged);
            automaticallyCompensateForAllPagesMenuItem.Unchecked += new RoutedEventHandler(automaticallyCompensateForAllPagesMenuItem_CheckedChanged);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEditorWindow"/> class.
        /// </summary>
        /// <param name="templateManager">The template manager.</param>
        /// <param name="binarizeCommand">The binarize command.</param>
        /// <param name="renderingSettings">The rendering settings.</param>
        /// <param name="tesseractOcrDllDirectory">The directory,
        /// where Tesseract5.Vintasoft.xXX.dll is located.</param>
        public TemplateEditorWindow(
            FormTemplateManager templateManager,
            ChangePixelFormatToBlackWhiteCommand binarizeCommand,
            RenderingSettings renderingSettings,
            string tesseractOcrDllDirectory)
            : this()
        {
            _tesseractOcrDllDirectory = tesseractOcrDllDirectory;
            _templateManager = templateManager;
            _binarizeCommand = binarizeCommand;
            _renderingSettings = renderingSettings;

            templateManager.TemplateImages.ImageCollectionChanged +=
               new EventHandler<ImageCollectionChangeEventArgs>(TemplateImages_ImageCollectionChanged);

            propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(propertyGrid1_PropertyValueChanged);

            // load XPS codec
            DemosTools.LoadXpsCodec();
            // set XPS rendering requirement
            DemosTools.SetXpsRenderingRequirement(imageViewer1, 0f);
            CodecsFileFilters.SetFilters(_openImageFileDialog);

            // create a visual tool for editing form field templates
            _fieldTemplateEditorTool = new WpfFormFieldTemplateEditorTool();
            _fieldTemplateEditorTool.FocusedFieldTemplateViewChanged += fieldTemplateEditorTool_FocusedFieldTemplateViewChanged;
            _fieldTemplateEditorTool.MouseDoubleClick += fieldTemplateEditorTool_MouseDoubleClick;
            _fieldTemplateEditorTool.FieldTemplateAdding += FieldTemplateEditorTool_FieldTemplateAdded;


            // set the template images as the images of the viewer
            imageViewer1.Images = templateManager.TemplateImages;
            // set the visual tool
            imageViewer1.VisualTool = new WpfCompositeVisualTool(_fieldTemplateEditorTool, _fieldTemplateEditorTool.FieldTemplateWizardTool);
            imageViewer1.FocusedIndexChanged +=
                new PropertyChangedEventHandler<int>(imageViewer1_FocusedIndexChanged);

            _templatesRootNode = (TreeViewItem)templatesTreeView.Items[0];
            templatesTreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(templatesTreeView_SelectedItemChanged);
            templatesTreeView.MouseDoubleClick += new MouseButtonEventHandler(templatesTreeView_MouseDoubleClick);
            // update the UI
            UpdateUI();

            _openDocumentDialog.Multiselect = false;
            _openDocumentDialog.Filter = FormDocumentTemplatesFilter;

            _openImageOrDocumentDialog.Multiselect = false;
            _openImageOrDocumentDialog.Filter = string.Format(
                "{0}|{1}|{2}",
                AllSupportedTemplateFilesFilter,
                ImageFilesFilter,
                FormDocumentTemplatesFilter);
            DemosTools.SetTestFilesFolder(_openImageOrDocumentDialog);

            _openImageFileDialog.Multiselect = true;
        }

        #endregion



        #region Properties

#if !REMOVE_OCR_PLUGIN
        OcrEngineSettings _defaultOcrEngineSettings;
        /// <summary>
        /// Gets or sets the default OCR engine settings for newly created OCR fields.
        /// </summary>
        public OcrEngineSettings DefaultOcrEngineSettings
        {
            get
            {
                return _defaultOcrEngineSettings;
            }
            set
            {
                _defaultOcrEngineSettings = value;
            }
        }

        OcrRecognitionRegionSplittingSettings _defaultOcrRecognitionRegionSplittingSettings;
        /// <summary>
        /// Gets or sets the default OCR recognition region splitting settings.
        /// </summary>
        public OcrRecognitionRegionSplittingSettings DefaultOcrRecognitionRegionSplittingSettings
        {
            get
            {
                return _defaultOcrRecognitionRegionSplittingSettings;
            }
            set
            {
                _defaultOcrRecognitionRegionSplittingSettings = value;
            }
        }

#if !REMOVE_OCR_ML_ASSEMBLY
        HandwrittenDigitsOcrSettings _defaultHandwritingDigitsOcrSettings;
        /// <summary>
        /// Gets or sets the default handwiting digits OCR engine settings for newly created OCR fields.
        /// </summary>
        public HandwrittenDigitsOcrSettings DefaultHandwritingDigitsOcrSettings
        {
            get
            {
                return _defaultHandwritingDigitsOcrSettings;
            }
            set
            {
                _defaultHandwritingDigitsOcrSettings = value;
            }
        }
#endif
#endif

#if !REMOVE_BARCODE_SDK
        ReaderSettings _defaultBarcodeReaderSettings;
        /// <summary>
        /// Gets or sets the default barcode reader settings for newly created barcode fields.
        /// </summary>
        public ReaderSettings DefaultBarcodeReaderSettings
        {
            get
            {
                return _defaultBarcodeReaderSettings;
            }
            set
            {
                _defaultBarcodeReaderSettings = value;
            }
        }
#endif

        /// <summary>
        /// Gets a value indicating whether image background compensation 
        /// must be automatically executed for all pages.
        /// </summary>
        public bool AutomaticallyImageBackgroundCompensation
        {
            get
            {
                return automaticallyCompensateForAllPagesMenuItem.IsChecked;
            }
        }

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Gets a value indicating whether OCR engine is available.
        /// </summary>
        public bool IsOcrEngineAvailable
        {
            get
            {
                return OcrFieldTemplate.OcrEngineManager != null;
            }
        }
#endif

        #endregion



        #region Methods

        #region Template Editor Form

        /// <summary>
        /// Form is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _templatesRootNode.IsExpanded = true;

            // update the UI
            UpdateUI();
        }

        #endregion


        #region UI state

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            int templateCount = _templateManager.TemplateImages.Count;
            bool isImageLoaded = imageViewer1.Image != null || imageViewer1.Images.Count > 0;
            bool isFieldTemplateSelected = _fieldTemplateEditorTool.FocusedFieldTemplateView != null;
            bool containsFieldTemplates = isImageLoaded &&
                _fieldTemplateEditorTool.FieldTemplateCollection != null &&
                _fieldTemplateEditorTool.FieldTemplateCollection.Count > 0;
#if !REMOVE_OCR_PLUGIN
            bool isOcrEngineAvailable = IsOcrEngineAvailable;
#endif

            // "File" menu
            //
            saveDocumentAsMenuItem.IsEnabled = templateCount > 0;

            // "Page" menu
            //
            loadPageFieldTemplatesMenuItem.IsEnabled = templateCount > 0;
            savePageFieldTemplatesMenuItem.IsEnabled = templateCount > 0;

            // "View" menu
            //
            imageViewerToolBar.SaveButtonEnabled = templateCount > 0;

            // "Field templates" menu
            //
            templateImageBackgroundCompensatMenuItem.IsEnabled = templateCount > 0;
            bool automaticallyCompensation = AutomaticallyImageBackgroundCompensation;
            compensateForAllPagesMenuItem.IsEnabled = !automaticallyCompensation;
            compensateForCurrentPageMenuItem.IsEnabled = !automaticallyCompensation;
            ignoreForAllPagesMenuItem.IsEnabled = !automaticallyCompensation;
            ignoreForCurrentPageMenuItem.IsEnabled = !automaticallyCompensation;
            cutMenuItem.IsEnabled = isFieldTemplateSelected;
            copyMenuItem.IsEnabled = isFieldTemplateSelected;
            pasteMenuItem.IsEnabled = _fieldTemplateViewCopy != null;
            deleteMenuItem.IsEnabled = isFieldTemplateSelected;
            deleteAllMenuItem.IsEnabled = containsFieldTemplates;
            groupMenuItem.IsEnabled = containsFieldTemplates;
            ungroupMenuItem.IsEnabled = containsFieldTemplates;


            // "OMR" menu
            //
            addOmrRectangleMenuItem.IsEnabled = isImageLoaded;
            addOmrEllipseMenuItem.IsEnabled = isImageLoaded;
            addTableOfOmrRectanglesMenuItem.IsEnabled = isImageLoaded;
            addTableOfOmrEllipsesMenuItem.IsEnabled = isImageLoaded;

#if !REMOVE_OCR_PLUGIN
            // "OCR" menu
            //
            addOCRFieldMenuItem.IsEnabled = isImageLoaded && isOcrEngineAvailable;
            defaultOcrSettingsMenuItem.IsEnabled = isOcrEngineAvailable;
#endif

            // "Barcode" menu
            //
            addBarcodeFieldMenuItem.IsEnabled = isImageLoaded;

            // Toolstrip
            string[] buttons = new string[] {
                "omrRectangleButton",
                "omrEllipseButton",
                "tableOfOmrRectanglesButton",
                "tableOfOmrEllipsesButton",
#if !REMOVE_OCR_PLUGIN
                "ocrButton", 
#endif
                "barcodeButton" };
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = (Button)FindName(buttons[i]);
                button.IsEnabled = isImageLoaded;
            }

#if !REMOVE_OCR_PLUGIN
            if (!isOcrEngineAvailable && isImageLoaded)
            {
                ((Button)FindName("ocrButton")).IsEnabled = false;
            }
#endif
        }

        /// <summary>
        /// Updates information about focused image.
        /// </summary>
        private void UpdateImageInfo()
        {
            VintasoftImage image = imageViewer1.Image;
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

        /// <summary>
        /// Sets the selected object in property grid.
        /// </summary>
        /// <param name="selectedObject">The selected object.</param>
        private void SetSelectedObjectInPropertyGrid(object selectedObject)
        {
            if (selectedObject != null)
            {
                propertyGrid1.SelectedObject = selectedObject;
                propertyGridGroupBox.Header = selectedObject.GetType().Name;
            }
            else
            {
                propertyGrid1.SelectedObject = null;
                propertyGridGroupBox.Header = "<No selected object>";
            }
        }

        /// <summary>
        /// Creates a name for a template image.
        /// </summary>
        /// <param name="image">The template image.</param>
        private string GetTemplateName(VintasoftImage image)
        {
            switch (image.SourceInfo.SourceType)
            {
                case ImageSourceType.File:
                    if (image.SourceInfo.PageCount == 1)
                        return System.IO.Path.GetFileNameWithoutExtension(image.SourceInfo.Filename);

                    return string.Format(
                        "{0}, Page {1}",
                        System.IO.Path.GetFileName(image.SourceInfo.Filename),
                        image.SourceInfo.PageIndex + 1);

                default:
                    return "Template";
            }
        }

        #endregion


        #region 'File' menu

        /// <summary>
        /// Clears image collection of the image viewer and adds template image(s)
        /// to the image collection of the image viewer.
        /// </summary>
        private void openTemplateImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenImages(false);
        }

        /// <summary>
        /// Clears image collection of the image viewer and adds template document(s)
        /// to the image collection of the image viewer.
        /// </summary>
        private void openDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenDocument(false);
        }

        /// <summary>
        /// Adds template image(s) to the image collection of the image viewer.
        /// </summary>
        private void addTemplateImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenImages(true);
        }

        /// <summary>
        /// Adds template document(s) to the image collection of the image viewer.
        /// </summary>
        private void addDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenDocument(true);
        }

        /// <summary>
        /// Handles the SaveFile event of imageViewerToolBar object.
        /// </summary>
        private void imageViewerToolBar_SaveFile(object sender, EventArgs e)
        {
            saveDocumentAsMenuItem_Click(this, null);
        }

        /// <summary>
        /// Saves current template document to new source.
        /// </summary>
        private void saveDocumentAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string documentName = (string)_templatesRootNode.Header;
            if (documentName == "Untitled")
                documentName = string.Empty;
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();

            saveDialog.FileName = documentName;
            saveDialog.Filter = FormDocumentTemplatesFilter;
            if (saveDialog.ShowDialog().Value)
            {
                _templateManager.SaveToDocument(saveDialog.FileName);
            }
        }

        /// <summary>
        /// Closes the current template document.
        /// </summary>
        private void closeDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CloseDocument();
        }

        /// <summary>
        /// Closes the form.
        /// </summary>
        private void closeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        #endregion


        #region 'Page' menu

        /// <summary>
        /// Loads form field template collection of the current page template from a file.
        /// </summary>
        private void loadPageFieldTemplatesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = FormPageTemplatesFilter;

            if (openDialog.ShowDialog().Value)
            {
                // load page template from file
                FormPageTemplate pageTemplate = FormPageTemplate.Deserialize(openDialog.FileName);
                VintasoftImage templateImage = imageViewer1.Image;
                // set the page template to the current image
                _templateManager.SetPageTemplate(templateImage, pageTemplate);
                // set items of page template as current items of the visual tool
                SetCurrentFormFieldTemplates(pageTemplate.Items);
                // add field templates to the tree view
                foreach (FormFieldTemplate formFieldTemplate in pageTemplate.Items)
                    AddFormFieldTemplateTreeNode(templateImage, formFieldTemplate);

                // check OCR availability
                CheckOcrAvailability();
            }
        }

        /// <summary>
        /// Saves form field template collection of the current page template to a file.
        /// </summary>
        private void savePageFieldTemplatesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.Filter = FormPageTemplatesFilter;
            if (saveDialog.ShowDialog().Value)
            {
                // get the page template
                FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(imageViewer1.Image);
                // save to a file
                FormPageTemplate.Serialize(saveDialog.FileName, pageTemplate);
            }
        }

        #endregion


        #region 'View' menu

        /// <summary>
        /// Shows the image viewer settings.
        /// </summary>
        private void imageViewerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImageViewerSettingsWindow viewerSettingsDialog = new ImageViewerSettingsWindow(imageViewer1);
            viewerSettingsDialog.ShowDialog();
        }

        #endregion


        #region 'Form field templates' menu

        /// <summary>
        /// Enables/disables Automatically Compensation.
        /// </summary>
        private void automaticallyCompensateForAllPagesMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Compensates the background for all template images.
        /// </summary>
        private void compensateForAllPagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int imageCount = _templateManager.TemplateImages.Count;
            for (int i = 0; i < imageCount; i++)
            {
                currentActionLabel.Content = string.Format("Processing image {0} of {1}...", i + 1, imageCount);
                VintasoftImage templateImage = _templateManager.TemplateImages[i];
                FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(templateImage);
                _templateManager.CompensateTemplateImageBackground(templateImage, pageTemplate);
            }
            currentActionLabel.Content = "";
            DemosTools.ShowInfoMessage("Template images background compensation is done.");
        }

        /// <summary>
        /// Compensates the background for current template image.
        /// </summary>
        private void compensateForCurrentPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VintasoftImage currentImage = imageViewer1.Image;
            FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(currentImage);
            _templateManager.CompensateTemplateImageBackground(currentImage, pageTemplate);
            DemosTools.ShowInfoMessage("Template image background compensation is done.");
        }

        /// <summary>
        /// Ignores the background compensation for all template images.
        /// </summary>
        private void ignoreForAllPagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int imageCount = _templateManager.TemplateImages.Count;
            // for each template images
            for (int i = 0; i < imageCount; i++)
            {
                currentActionLabel.Content = string.Format("Processing image {0} of {1}...", i + 1, imageCount);
                // get reference to the template image
                VintasoftImage templateImage = _templateManager.TemplateImages[i];
                // get reference to the form definition associated with template image
                FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(templateImage);
                // remove information about compensation of background of template image
                _templateManager.CompensateTemplateImageBackground(null, pageTemplate);
            }
            currentActionLabel.Content = "";
            DemosTools.ShowInfoMessage("Template images background compensation is done.");
        }

        /// <summary>
        /// Ignores the background compensation for current template image.
        /// </summary>
        private void ignoreForCurrentPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(imageViewer1.Image);
            _templateManager.CompensateTemplateImageBackground(null, pageTemplate);
            DemosTools.ShowInfoMessage("Template image background compensation is done.");
        }

        /// <summary>
        /// Cuts selected form field template.
        /// </summary>
        private void cutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_fieldTemplateViewCopy != null)
                _fieldTemplateViewCopy.Dispose();
            _fieldTemplateViewCopy = GetFocusedFieldTemplateCopy();
            DeleteFocusedFieldTemplate();
        }

        /// <summary>
        /// Copies selected form field template.
        /// </summary>
        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_fieldTemplateViewCopy != null)
                _fieldTemplateViewCopy.Dispose();
            _fieldTemplateViewCopy = GetFocusedFieldTemplateCopy();

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Pastes form field template from "internal" buffer and makes it active.
        /// </summary>
        private void pasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.RectangleF boundingBox = _fieldTemplateViewCopy.FieldTemplate.BoundingBox;
            boundingBox.Offset(20f, 20f);
            _fieldTemplateViewCopy.FieldTemplate.BoundingBox = boundingBox;
            WpfFormFieldTemplateView fieldTemplateViewCopy =
                (WpfFormFieldTemplateView)_fieldTemplateViewCopy.Clone();

            AddFormFieldTemplateTreeNode(imageViewer1.Image, fieldTemplateViewCopy.FieldTemplate);
            _fieldTemplateEditorTool.FieldTemplateViewCollection.Add(fieldTemplateViewCopy);
            _fieldTemplateEditorTool.FocusedFieldTemplateView = fieldTemplateViewCopy;

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Removes selected form field template from collection.
        /// </summary>
        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteFocusedFieldTemplate();
        }

        /// <summary>
        /// Removes all form field templates from collection.
        /// </summary>
        private void deleteAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FormPageTemplate templatePage = _templateManager.GetPageTemplate(imageViewer1.Image);
            foreach (FormFieldTemplate item in templatePage.Items)
                RemoveFormFieldTemplateTreeNode(item);
            templatePage.Items.Clear();

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Shows a form that allows to select some of the form field templates
        /// and combine selected templates in a group.
        /// </summary>
        private void groupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // check available field template count
            if (_fieldTemplateEditorTool.FieldTemplateCollection.Count < 2)
            {
                DemosTools.ShowInfoMessage("There shall be at least 2 field templates on a form.");
                return;
            }

            // open a form that allows to select some of the form field templates
            FieldTemplatesSelectionWindow selectionWindow = new FieldTemplatesSelectionWindow(
                _fieldTemplateEditorTool.FieldTemplateCollection);
            selectionWindow.Owner = this;

            if (selectionWindow.ShowDialog().Value)
            {
                // get selected field templates
                ICollection<FormFieldTemplate> selectedTemplates = selectionWindow.SelectedFieldTemplates;
                // if it is possible to make a group
                if (selectedTemplates.Count > 1)
                {
                    // create a group
                    FormFieldTemplateGroup templateGroup = new FormFieldTemplateGroup();
                    // for each selected template
                    foreach (FormFieldTemplate selectedTemplate in selectedTemplates)
                    {
                        // remove the template from the collection
                        _fieldTemplateEditorTool.FieldTemplateCollection.Remove(selectedTemplate);
                        RemoveFormFieldTemplateTreeNode(selectedTemplate);
                        // add the template to the group
                        templateGroup.Items.Add(selectedTemplate);
                    }
                    // add the group to the collection
                    _fieldTemplateEditorTool.FieldTemplateCollection.Add(templateGroup);
                    AddFormFieldTemplateTreeNode(imageViewer1.Image, templateGroup);
                    // subscribe to the PropertyChanged event
                    templateGroup.PropertyChanged += new EventHandler<ObjectPropertyChangedEventArgs>(templateGroup_PropertyChanged);
                }
                else
                {
                    DemosTools.ShowWarningMessage("You should select at least 2 form field templates to create a group.");
                }
            }
        }

        /// <summary>
        /// Shows a form that allows to select some of the form field template groups
        /// and split selected groups into separate form field templates.
        /// </summary>
        private void ungroupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<FormFieldTemplate> templateGroups = new List<FormFieldTemplate>();

            foreach (FormFieldTemplate fieldTemplate in _fieldTemplateEditorTool.FieldTemplateCollection)
            {
                if (fieldTemplate is FormFieldTemplateGroup)
                    templateGroups.Add(fieldTemplate);
            }

            // check available field template group count
            if (templateGroups.Count == 0)
            {
                DemosTools.ShowInfoMessage("No field template groups found.");
                return;
            }

            // open a form that allows to select some of the form field templates
            FieldTemplatesSelectionWindow selectionWindow = new FieldTemplatesSelectionWindow(templateGroups);
            selectionWindow.Owner = this;

            if (selectionWindow.ShowDialog().Value)
            {
                // get selected field templates
                ICollection<FormFieldTemplate> selectedTemplates = selectionWindow.SelectedFieldTemplates;
                // if at least one template group is selected
                if (selectedTemplates.Count > 0)
                {
                    // for each selected template group
                    foreach (FormFieldTemplate selectedTemplate in selectedTemplates)
                    {
                        // get as template group
                        FormFieldTemplateGroup selectedTemplateGroup = (FormFieldTemplateGroup)selectedTemplate;
                        // unsubscribe from the PropertyChanged event
                        selectedTemplateGroup.PropertyChanged -=
                            new EventHandler<ObjectPropertyChangedEventArgs>(templateGroup_PropertyChanged);
                        // remove the template group from the collection
                        _fieldTemplateEditorTool.FieldTemplateCollection.Remove(selectedTemplateGroup);
                        RemoveFormFieldTemplateTreeNode(selectedTemplateGroup);
                        VintasoftImage image = imageViewer1.Image;
                        // add items of the group to the collection
                        foreach (FormFieldTemplate nestedTemplate in selectedTemplateGroup.Items)
                        {
                            _fieldTemplateEditorTool.FieldTemplateCollection.Add(nestedTemplate);
                            AddFormFieldTemplateTreeNode(image, nestedTemplate);
                        }
                    }
                }
                else
                {
                    DemosTools.ShowWarningMessage("You should select at least 1 form field template group to ungroup.");
                }
            }
        }

        #endregion


        #region 'OCR' menu

        /// <summary>
        /// Adds an OCR field template to an image and starts building of it.
        /// </summary>
        private void addOCRFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            AddFormFieldTemplate(new OcrFieldTemplate((OcrEngineSettings)_defaultOcrEngineSettings.Clone(),
                (OcrRecognitionRegionSplittingSettings)_defaultOcrRecognitionRegionSplittingSettings.Clone()));
#endif
        }

        /// <summary>
        /// Adds an handwrited digits OCR field template to an image and starts building of it.
        /// </summary>
        private void addHandwritedDigitsOCRFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN && !REMOVE_OCR_ML_ASSEMBLY
            AddFormFieldTemplate(new OcrFieldTemplate((OcrEngineSettings)_defaultHandwritingDigitsOcrSettings.Clone(),
                (OcrRecognitionRegionSplittingSettings)_defaultOcrRecognitionRegionSplittingSettings.Clone()));
#endif
        }

        /// <summary>
        /// Shows the default OCR settings for newly added OCR fields.
        /// </summary>
        private void defaultOcrSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            OcrSettingsWindow settingsWindow = new OcrSettingsWindow((TesseractOcrSettings)_defaultOcrEngineSettings,
                OcrFieldTemplate.OcrEngineManager.SupportedLanguages, OcrBinarizationMode.Global, false,
                _defaultOcrRecognitionRegionSplittingSettings, _recognizeTextInMultipleThreads, _maxOcrThreads);
            settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settingsWindow.Owner = this;
            settingsWindow.CanChooseBinarization = false;
            settingsWindow.ShowHighlightLowConfidenceWordsCheckBox = false;
            if (settingsWindow.ShowDialog() == true)
            {
                OcrFieldTemplate.OcrEngineManager = CreateManager(
                    _tesseractOcrDllDirectory,
                    settingsWindow.UseMultithreading,
                    settingsWindow.MaxThreads);
            }
#endif
        }

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Creates the OCR engine manager for the specified settings.
        /// </summary>
        /// <param name="tesseractOcrDllDirectory">The directory,
        /// where Tesseract5.Vintasoft.xXX.dll is located.</param>
        /// <param name="recognizeTextInMultipleThreads">Indicates that
        /// text recognition must be executed in multiple threads.</param>
        /// <param name="maxOcrThreads">The maximum count of threads,
        /// which can be used for text recognition.</param>
        private OcrEngineManager CreateManager(
            string tesseractOcrDllDirectory,
            bool recognizeTextInMultipleThreads,
            int maxOcrThreads)
        {
            OcrEngineManager manager = OcrFieldTemplate.OcrEngineManager;

            if (_recognizeTextInMultipleThreads != recognizeTextInMultipleThreads ||
                _maxOcrThreads != maxOcrThreads)
            {
                TesseractOcr ocrEngine = new TesseractOcr(tesseractOcrDllDirectory);
                TesseractOcr[] additionalOcrEngines = null;

                if (recognizeTextInMultipleThreads && maxOcrThreads > 1)
                {
                    additionalOcrEngines = new TesseractOcr[maxOcrThreads - 1];

                    for (int i = 0; i < additionalOcrEngines.Length; i++)
                        additionalOcrEngines[i] = new TesseractOcr(tesseractOcrDllDirectory);
                }

                manager = new OcrEngineManager(ocrEngine, additionalOcrEngines);
                manager.RecognitionRegionSplittingSettings = _defaultOcrRecognitionRegionSplittingSettings;

                _recognizeTextInMultipleThreads = recognizeTextInMultipleThreads;
                _maxOcrThreads = maxOcrThreads;
            }

            return manager;
        }
#endif

        #endregion


        #region 'OMR' menu

        /// <summary>
        /// Adds a rectangular OMR field template to an image and starts building of it.
        /// </summary>
        private void addRectangularOmrMarkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddFormFieldTemplate(new OmrRectangularFieldTemplate());
        }

        /// <summary>
        /// Adds an elliptical OMR field template to an image and starts building of it.
        /// </summary>
        private void addEllipticalOmrMarkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddFormFieldTemplate(new OmrEllipticalFieldTemplate());
        }

        /// <summary>
        /// Adds a table with rectangular OMR field templates to an image and starts building of it.
        /// </summary>
        private void addTableWithRectangularOmrMarksMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddOmrTemplateTable(new OmrRectangularFieldTemplate());
        }

        /// <summary>
        /// Adds a table with elliptical OMR field templates to an image and starts building of it.
        /// </summary>
        private void addTableWithEllipticalOmrMarksMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddOmrTemplateTable(new OmrEllipticalFieldTemplate());
        }

        #endregion


        #region 'Barcode' menu

        /// <summary>
        /// Adds a barcode field template to an image and starts building of it.
        /// </summary>
        private void addBarcodeFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_BARCODE_SDK
            AddFormFieldTemplate(new BarcodeFieldTemplate(_defaultBarcodeReaderSettings.Clone()));
#endif
        }

        /// <summary>
        /// Shows the default barcode reading settings for newly added barcode fields.
        /// </summary>
        private void barcodeReaderDefaultSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_BARCODE_SDK
            BarcodeReaderSettingsWindow settingsForm = new BarcodeReaderSettingsWindow(_defaultBarcodeReaderSettings);
            settingsForm.ShowDialog();
#endif
        }
        #endregion


        #region Image viewer toolstrip

        /// <summary>
        /// Clears image collection of the image viewer and adds template image or document
        /// to the image collection of the image viewer.
        /// </summary>
        private void imageViewerToolBar_OpenFile(object sender, EventArgs e)
        {
            OpenImageOrDocument();
        }

        #endregion


        #region Image viewer

        /// <summary>
        /// Handles the KeyDown event of imageViewer1 object.
        /// </summary>
        private void imageViewer1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
                DeleteFocusedFieldTemplate();
        }

        /// <summary>
        /// Index of focused image in viewer is changed.
        /// </summary>
        private void imageViewer1_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            VintasoftImage image = imageViewer1.Image;
            if (image == null)
            {
                _fieldTemplateEditorTool.FieldTemplateCollection = null;
            }
            else
            {
                // get the form page template
                FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(image);
                // set items of page template as current items of the visual tool
                SetCurrentFormFieldTemplates(pageTemplate.Items);
                // if image has a tree view node
                if (_imagesToTemplateTreeNodes.ContainsKey(image))
                {
                    // select the node in the tree view
                    _imagesToTemplateTreeNodes[image].IsSelected = true;
                }
            }
            UpdateImageInfo();
        }

        #endregion


        #region Image collection

        /// <summary>
        /// Collection of template images is changed.
        /// </summary>
        private void TemplateImages_ImageCollectionChanged(object sender, ImageCollectionChangeEventArgs e)
        {
            // synchronize the tree view of the template document
            switch (e.Action)
            {
                case ImageCollectionChangeAction.Clear:
                case ImageCollectionChangeAction.RemoveImages:
                    VintasoftImage[] images = e.Images;
                    for (int i = 0; i < images.Length; i++)
                    {
                        VintasoftImage removedImage = images[i];

                        TreeViewItem removedImageNode = _imagesToTemplateTreeNodes[removedImage];
                        for (int j = removedImageNode.Items.Count - 1; j >= 0; j--)
                            RemoveFormFieldTemplateTreeNode(removedImageNode.Items[j] as TreeViewItem);
                        _imagesToTemplateTreeNodes.Remove(removedImage);
                        removedImageNode.Tag = null;
                        _templatesRootNode.Items.Remove(removedImageNode);
                    }
                    break;
            }

            // update the UI
            UpdateUI();
        }

        #endregion


        #region File manipulation

        /// <summary>
        /// Opens image files and adds to the image collection of image viewer.
        /// Binarization is applied if necessary.
        /// </summary>
        /// <param name="append">Determines whether to append images to existing images in the collection.
        /// </param>
        private void OpenImages(bool append)
        {
            if (_openImageFileDialog.ShowDialog(this).Value)
            {
                if (!append)
                {
                    // close existing template document
                    CloseDocument();
                }

                foreach (string filename in _openImageFileDialog.FileNames)
                {
                    if (!AddImage(filename))
                        break;
                }
            }
        }

        /// <summary>
        /// Adds images from specified file path to the image collection.
        /// </summary>
        /// <param name="filename">Path to the file.</param>
        private bool AddImage(string filename)
        {
            ImageCollection templateImages = _templateManager.TemplateImages;
            // temporary image collection for all images in current file
            ImageCollection addingImages = new ImageCollection();
            DocumentPasswordWindow.EnableAuthentication(addingImages);
            try
            {
                try
                {
                    addingImages.Add(filename);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex, filename);
                    return true;
                }

                bool canceled = false;
                bool applyForAll = false;

                foreach (VintasoftImage image in addingImages)
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
                    templateImages.Add(image);
                    FormPageTemplate templatePage = _templateManager.GetPageTemplate(image);
                    templatePage.Name = GetTemplateName(image);
                    AddPageTreeNode(image, templatePage.Name);
                }
                addingImages.Clear();

                // if information about the field templates automatic detection was not shown
                if (!IsInfoAboutFieldTemplatesAutomaticDetectionShown)
                {
                    // specify that information about the field templates automatic detection was shown
                    IsInfoAboutFieldTemplatesAutomaticDetectionShown = true;
                    // show information about the field templates automatic detection
                    MessageBox.Show(
                        "The visual tool WpfFormFieldTemplateEditorTool can automatically detect circular and quadratical OMR marks on image. Also visual tool can detect circular and quadratical OMR marks combined into table.\n" +
                        "If you do not want to manually select region for template of each OMR mark and want to create templates for OMR marks automatically, please do the following steps:\n" +
                        "1. Select rectangular region on template image. Region must contain one or several circular or quadratical OMR marks.\n" +
                        "2. Click button ('Rectangular OMR mark', 'Elliptical OMR mark', 'Table of rectangular OMR marks', 'Table of elliptical OMR marks') in toolbar to select the type of OMR marks, which should be detected. Specify the table settings if table with OMR marks must be detected.\n" +
                        "\n" +
                        "The visual tool WpfFormFieldTemplateEditorTool will automatically create templates for detected OMR marks if OMR marks are detected in image region.\n" +
                        "The visual tool WpfFormFieldTemplateEditorTool will create 'standard' template for OMR marks of selected type if OMR marks are not detected in image region.",
                        "The field templates automatic detection", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                return !canceled;
            }
            finally
            {
                DocumentPasswordWindow.DisableAuthentication(addingImages);
            }
        }

        /// <summary>
        /// Opens image file or template document and adds to the image collection of image viewer.
        /// Binarization is applied if necessary.
        /// </summary>
        private void OpenImageOrDocument()
        {
            if (_openImageOrDocumentDialog.ShowDialog().Value)
            {
                string filename = _openImageOrDocumentDialog.FileName;
                switch (System.IO.Path.GetExtension(filename).ToLowerInvariant())
                {
                    case ".fdt":
                        AddDocument(filename, false);
                        break;

                    default:
                        // close existing template document
                        CloseDocument();
                        AddImage(filename);
                        break;
                }
            }
        }

        /// <summary>
        /// Opens template document and adds to the image collection of image viewer.
        /// Binarization is applied if necessary.
        /// </summary>
        /// <param name="append">Determines whether to append images to existing images in the collection.
        /// </param>
        private void OpenDocument(bool append)
        {
            if (_openDocumentDialog.ShowDialog().Value)
                AddDocument(_openDocumentDialog.FileName, append);
        }

        /// <summary>
        /// Adds template document from specified file path to the image collection.
        /// Binarization is applied if necessary.
        /// </summary>
        /// <param name="filename">Path to the file.</param>
        /// <param name="append">Determines whether to append images to existing images in the collection.
        /// </param>
        internal void AddDocument(string filename, bool append)
        {
            FormDocumentTemplate templateDocument;
            try
            {
                templateDocument = FormDocumentTemplate.Deserialize(filename);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
                return;
            }
            try
            {
                if (append)
                {
                    _templateManager.AddPageTemplatesFromDocument(templateDocument);
                }
                else
                {
                    _templateManager.LoadFromDocument(templateDocument);
                    if (string.IsNullOrEmpty(templateDocument.Name))
                        SetDefaultRootNode();
                    else
                        _templatesRootNode.Header = templateDocument.Name;
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }

            // images to remove from template manager
            List<VintasoftImage> imagesToRemove = new List<VintasoftImage>();
            bool canceled = false;
            bool applyForAll = false;

            for (int i = 0; i < templateDocument.Pages.Count; i++)
            {
                // current page template
                FormPageTemplate templatePage = templateDocument.Pages[i];
                if (!_templateManager.ContainsPageTemplate(templatePage))
                    continue;
                // get corresponding template image
                VintasoftImage templateImage = _templateManager.GetTemplateImage(templatePage);
                // if binarization is canceled
                if (canceled)
                {
                    imagesToRemove.Add(templateImage);
                    continue;
                }

                if (templateImage.PixelFormat != Vintasoft.Imaging.PixelFormat.BlackWhite)
                {
                    ProcessingCommandBase processingCommand = null;
                    // if settings shall be applied for all remaining images or
                    // settings are approved
                    if (applyForAll)
                    {
                        templateImage.RenderingSettings = _renderingSettings;
                        processingCommand = _binarizeCommand;
                    }
                    else
                    {
                        // create binarization form
                        ImageBinarizationWindow binarizationForm = new ImageBinarizationWindow(
                            _binarizeCommand, _renderingSettings);
                        binarizationForm.Owner = this;
                        if (binarizationForm.ShowDialog(templateImage))
                        {
                            templateImage.RenderingSettings = binarizationForm.GetRenderingSettings();
                            processingCommand = binarizationForm.GetProcessingCommand();

                            if (binarizationForm.ApplyForAll)
                                applyForAll = true;
                        }
                        // if binarization is canceled
                        if (binarizationForm.Cancel || binarizationForm.Skip)
                        {
                            canceled = binarizationForm.Cancel;
                            imagesToRemove.Add(templateImage);
                            continue;
                        }
                    }
                    // if processing command is set
                    if (processingCommand != null)
                    {
                        bool processingError = false;
                        try
                        {
                            processingCommand.ExecuteInPlace(templateImage);
                        }
                        catch (Exception ex)
                        {
                            DemosTools.ShowErrorMessage(ex);
                            templateImage.Dispose();
                            processingError = true;
                        }
                        if (processingError)
                            continue;
                    }
                }

                // add a node to the tree
                string name = templatePage.Name;
                if (string.IsNullOrEmpty(name))
                    name = templatePage.ImageFileName;
                AddPageTreeNode(templateImage, name);
                foreach (FormFieldTemplate formFieldTemplate in templatePage.Items)
                    AddFormFieldTemplateTreeNode(templateImage, formFieldTemplate);
            }

            if (imagesToRemove.Count > 0)
            {
                // remove images from template manager
                foreach (VintasoftImage templateImage in imagesToRemove)
                {
                    _templateManager.RemovePage(templateImage);
                    templateImage.Dispose();
                }
                imagesToRemove.Clear();
            }

            // check OCR availability
            CheckOcrAvailability();
        }

        /// <summary>
        /// Closes the current template document.
        /// </summary>
        private void CloseDocument()
        {
            // remove and dispose existing template images
            _templateManager.TemplateImages.ClearAndDisposeItems();
            SetDefaultRootNode();
        }


        /// <summary>
        /// Checks availability of the OCR field recognition.
        /// </summary>
        private void CheckOcrAvailability()
        {
#if !REMOVE_OCR_PLUGIN
            if (!IsOcrEngineAvailable && ContainsOcrFields())
            {
                if (MessageBox.Show(
                    "Page templates contain at least one OCR field template, but OCR engine is not found.\r\n" +
                    "Would you like to remove all OCR field templates from the document template?",
                    "OCR field templates detected",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RemoveAllOcrFields();
                    DemosTools.ShowInfoMessage("All OCR field templates were removed.");
                }
            }
#endif
        }

        /// <summary>
        /// Determines whether template manager contains OCR fields.
        /// </summary>
        /// <returns><b>true</b> if the template manager contains at least one OCR field template;
        /// otherwise, <b>false</b>.</returns>
        private bool ContainsOcrFields()
        {
            foreach (VintasoftImage templateImage in _templateManager.TemplateImages)
            {
                FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(templateImage);
                if (ContainsOcrFields(pageTemplate))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified form field template contains OCR fields.
        /// </summary>
        /// <param name="template">The form field template.</param>
        /// <returns><b>true</b> if the template contains at least one OCR field template;
        /// otherwise, <b>false</b>.</returns>
        private bool ContainsOcrFields(FormFieldTemplate template)
        {
            FormFieldTemplateGroup templateGroup = template as FormFieldTemplateGroup;
            if (templateGroup == null)
                return false;

#if !REMOVE_OCR_PLUGIN
            foreach (FormFieldTemplate item in templateGroup.Items)
            {
                if (item is OcrFieldTemplate)
                    return true;

                if (ContainsOcrFields(item))
                    return true;
            }
#endif

            return false;
        }

        /// <summary>
        /// Removes all OCR fields from template manager.
        /// </summary>
        private void RemoveAllOcrFields()
        {
#if !REMOVE_OCR_PLUGIN
            foreach (VintasoftImage templateImage in _templateManager.TemplateImages)
            {
                FormPageTemplate pageTemplate = _templateManager.GetPageTemplate(templateImage);
                RemoveAllOcrFields(pageTemplate);
            }
#endif
        }

        /// <summary>
        /// Removes all OCR fields from the specified form field template.
        /// </summary>
        /// <param name="template">The template.</param>
        private void RemoveAllOcrFields(FormFieldTemplate template)
        {
            FormFieldTemplateGroup templateGroup = template as FormFieldTemplateGroup;
#if !REMOVE_OCR_PLUGIN
            if (templateGroup != null)
            {
                List<FormFieldTemplate> ocrFieldTemplates = new List<FormFieldTemplate>();
                foreach (FormFieldTemplate item in templateGroup.Items)
                {
                    if (item is OcrFieldTemplate)
                        ocrFieldTemplates.Add(item);
                    else if (ContainsOcrFields(item))
                        RemoveAllOcrFields(item);
                }

                foreach (FormFieldTemplate item in ocrFieldTemplates)
                {
                    templateGroup.Items.Remove(item);
                    RemoveFormFieldTemplateTreeNode(item);
                }
            }
#endif
        }

        #endregion


        #region Field template editor tool


        /// <summary>
        /// Handles the FieldTemplateAdded event of the FieldTemplateEditorTool.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WpfFormFieldTemplateViewAddingEventArgs"/> instance containing the event data.</param>
        private void FieldTemplateEditorTool_FieldTemplateAdded(object sender, WpfFormFieldTemplateViewAddingEventArgs e)
        {
            SetTemplateFieldAppearance(e.FormFieldTemplateView);
            if (string.IsNullOrEmpty(e.FormFieldTemplateView.FieldTemplate.Name))
            {
                e.FormFieldTemplateView.FieldTemplate.Name = GenerateNewFieldName();
            }
            AddFormFieldTemplateTreeNode(imageViewer1.Image, e.FormFieldTemplateView.FieldTemplate);
        }

        /// <summary>
        /// Generates the new name of the field.
        /// </summary>
        private string GenerateNewFieldName()
        {
            string nameTemplate = "Field{0}";
            int number = 1;
            while (number < int.MaxValue)
            {
                string newName = string.Format(nameTemplate, number);
                bool nameFound = false;
                foreach (FormFieldTemplate field in _fieldTemplateEditorTool.FieldTemplateCollection)
                {
                    if (field.Name == newName)
                    {
                        nameFound = true;
                        break;
                    }
                }
                if (!nameFound)
                    return newName;
                number++;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Sets the current form field templates and adjusts the appearances of the views.
        /// </summary>
        /// <param name="fieldTemplates">The field templates.</param>
        private void SetCurrentFormFieldTemplates(FormFieldTemplateCollection fieldTemplates)
        {
            // set the collection of field templates as a current collection of the visual tool
            // (this will generate views for the field templates)
            _fieldTemplateEditorTool.FieldTemplateCollection = fieldTemplates;
            // if generated view collection exists
            if (_fieldTemplateEditorTool.FieldTemplateViewCollection != null)
                // set the appearances of generated views
                SetTemplateFieldsAppearance(_fieldTemplateEditorTool.FieldTemplateViewCollection);
            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Mouse button is double clicked when field template editor tool is active.
        /// </summary>
        void fieldTemplateEditorTool_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WpfFormFieldTemplateView fieldTemplateView = _fieldTemplateEditorTool.FocusedFieldTemplateView;
            // if form template view is found
            if (fieldTemplateView != null)
            {
                // if form template is OMR template table
                if (fieldTemplateView.FieldTemplate is OmrFieldTemplateTable)
                {
                    // get reference to the OMR template table
                    OmrFieldTemplateTable omrTemplateTable =
                        (OmrFieldTemplateTable)fieldTemplateView.FieldTemplate;
                    // create a form for editing the OMR template table cell values.
                    OmrTableCellValuesEditorWindow cellValuesEditorForm =
                        new OmrTableCellValuesEditorWindow(
                            omrTemplateTable.CellValues,
                            omrTemplateTable.Orientation);
                    cellValuesEditorForm.Owner = this;

                    // show the dialog
                    if (cellValuesEditorForm.ShowDialog().Value)
                    {
                        // set the orientation of OMR template table
                        omrTemplateTable.Orientation = cellValuesEditorForm.Orientation;
                        // refresh the property grid
                        propertyGrid1.Refresh();
                    }
                }
#if !REMOVE_BARCODE_SDK
                else if (fieldTemplateView.FieldTemplate is BarcodeFieldTemplate)
                {
                    // Barcode Field
                    BarcodeFieldTemplate barcodeFieldTemplate = (BarcodeFieldTemplate)fieldTemplateView.FieldTemplate;
                    BarcodeReaderSettingsWindow settingsWindow = new BarcodeReaderSettingsWindow(barcodeFieldTemplate.ReaderSettings);
                    settingsWindow.Owner = this;
                    // show the dialog
                    if (settingsWindow.ShowDialog() == true)
                    {
                        // refresh the property grid
                        propertyGrid1.Refresh();
                    }
                }
#endif
#if !REMOVE_OCR_PLUGIN
                else if (fieldTemplateView.FieldTemplate is OcrFieldTemplate)
                {
                    // OCR Field
                    OcrFieldTemplate ocrFieldTemplate = (OcrFieldTemplate)fieldTemplateView.FieldTemplate;
                    OcrSettingsWindow settingsWindow = new OcrSettingsWindow((TesseractOcrSettings)ocrFieldTemplate.OcrEngineSettings,
                                    OcrFieldTemplate.OcrEngineManager.SupportedLanguages, OcrBinarizationMode.Global, false,
                                    ocrFieldTemplate.OcrRecognitionRegionSplittingSettings, _recognizeTextInMultipleThreads, _maxOcrThreads);
                    settingsWindow.Owner = this;
                    settingsWindow.CanChooseBinarization = false;
                    settingsWindow.ShowHighlightLowConfidenceWordsCheckBox = false;
                    // show the dialog
                    if (settingsWindow.ShowDialog() == true)
                    {
                        OcrFieldTemplate.OcrEngineManager = CreateManager(
                            _tesseractOcrDllDirectory,
                            settingsWindow.UseMultithreading,
                            settingsWindow.MaxThreads);

                        // refresh the property grid
                        propertyGrid1.Refresh();
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Focused field template view is changed.
        /// </summary>
        void fieldTemplateEditorTool_FocusedFieldTemplateViewChanged(object sender, EventArgs e)
        {
            if (_isSelectingTreeViewItem)
                return;

            WpfFormFieldTemplateView focusedFieldTemplateView = _fieldTemplateEditorTool.FocusedFieldTemplateView;
            if (focusedFieldTemplateView != null)
            {
                SetSelectedObjectInPropertyGrid(focusedFieldTemplateView.FieldTemplate);

                TreeViewItem item = _formFieldTemplateToTreeNodes[focusedFieldTemplateView.FieldTemplate];
                _isSelectingTreeViewItem = true;
                item.IsSelected = true;
                _isSelectingTreeViewItem = false;
            }
            else
            {
                SetSelectedObjectInPropertyGrid(null);

                VintasoftImage image = imageViewer1.Image;
                if (image != null)
                    _imagesToTemplateTreeNodes[imageViewer1.Image].IsSelected = true;
            }

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Adds specified form field template to an image and starts building of it.
        /// </summary>
        /// <param name="fieldTemplate">A form field template to add.</param>
        private void AddFormFieldTemplate(FormFieldTemplate fieldTemplate)
        {
            WpfFormFieldTemplateView fieldTemplateView = WpfFormFieldTemplateViewFactory.CreateView(fieldTemplate);
            SetTemplateFieldAppearance(fieldTemplateView);
            _fieldTemplateEditorTool.AddAndBuild(fieldTemplateView);
        }

        /// <summary>
        /// Adds a table of OMR field templates to an image and starts building of it.
        /// </summary>
        /// <param name="cellTemplate">A template of table cell.</param>
        private void AddOmrTemplateTable(OmrFieldTemplate cellTemplate)
        {
            NewTableSetupWindow tableSetupForm = new NewTableSetupWindow((int)_tableInitalSize.Height, (int)_tableInitalSize.Width);
            tableSetupForm.Owner = this;
            if (tableSetupForm.ShowDialog().Value)
            {
                _tableInitalSize.Width = tableSetupForm.ColumnCount;
                _tableInitalSize.Height = tableSetupForm.RowCount;
                OmrFieldTemplateTable templateTable = new OmrFieldTemplateTable(
                    cellTemplate,
                    tableSetupForm.RowCount,
                    tableSetupForm.ColumnCount,
                    OmrTableOrientation.Horizontal);
                // set default distance between columns
                templateTable.DistanceBetweenColumns = 0.2f;
                // set default distance between rows
                templateTable.DistanceBetweenRows = 0.2f;
                templateTable.BuildingFinished += new EventHandler<EventArgs>(
                    templateTable_BuildingFinished);

                // create a form for editing the OMR template table cell values.
                OmrTableCellValuesEditorWindow cellValuesEditorForm =
                    new OmrTableCellValuesEditorWindow(
                        templateTable.CellValues,
                        templateTable.Orientation);
                cellValuesEditorForm.Owner = this;

                // show the dialog
                if (cellValuesEditorForm.ShowDialog().Value)
                {
                    // set the orientation of OMR template table
                    templateTable.Orientation = cellValuesEditorForm.Orientation;
                }

                // create view for table
                WpfFormFieldTemplateView templateTableView =
                    WpfFormFieldTemplateViewFactory.CreateView(templateTable);
                SetTemplateFieldAppearance(templateTableView);

                // build the table
                _fieldTemplateEditorTool.AddAndBuild(templateTableView);
            }
        }

        /// <summary>
        /// Rebuilding of items of a table of OMR field templates is finished.
        /// </summary>
        private void templateTable_BuildingFinished(object sender, EventArgs e)
        {
            OmrFieldTemplateTable templateTable = sender as OmrFieldTemplateTable;
            if (templateTable != null)
            {
                WpfFormFieldTemplateView templateTableView =
                    _fieldTemplateEditorTool.FieldTemplateViewCollection.FindView(templateTable);
                if (templateTableView != null)
                    SetTemplateFieldAppearance(templateTableView);
            }
        }

        /// <summary>
        /// Deletes the focused form field template view from image.
        /// </summary>
        private bool DeleteFocusedFieldTemplate()
        {
            bool deleted = false;
            WpfFormFieldTemplateView focusedFieldTemplateView =
                _fieldTemplateEditorTool.FocusedFieldTemplateView;
            if (focusedFieldTemplateView != null)
            {
                FormPageTemplate templatePage = _templateManager.GetPageTemplate(imageViewer1.Image);
                FormFieldTemplate fieldTemplate = focusedFieldTemplateView.FieldTemplate;
                focusedFieldTemplateView.IsVisible = false;
                DeleteFieldTemplate(templatePage.Items, fieldTemplate);
                deleted = true;
            }

            // update the UI
            UpdateUI();

            return deleted;
        }

        /// <summary>
        /// Deletes the field template.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="item">The item.</param>
        private bool DeleteFieldTemplate(FormFieldTemplateCollection collection, FormFieldTemplate item)
        {
            if (collection.Contains(item))
            {
                collection.Remove(item);
                RemoveFormFieldTemplateTreeNode(item);
                return true;
            }

            for (int i = 0; i < collection.Count; i++)
            {
                FormFieldTemplate currentTemplate = collection[i];
                if (collection[i] is FormFieldTemplateGroup)
                {
                    FormFieldTemplateGroup group = collection[i] as FormFieldTemplateGroup;
                    if (DeleteFieldTemplate(group.Items, item))
                    {
                        if (group.Items.Count == 0)
                        {
                            collection.Remove(currentTemplate);
                            RemoveFormFieldTemplateTreeNode(currentTemplate);
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        #endregion


        #region Form field view

        /// <summary>
        /// Sets the appearance of form field template view collection.
        /// </summary>
        /// <param name="fieldTemplateViews">A collection of views of the form field templates.</param>
        private void SetTemplateFieldsAppearance(WpfFormFieldTemplateViewCollection fieldTemplateViews)
        {
            foreach (WpfFormFieldTemplateView fieldTemplateView in fieldTemplateViews)
            {
                SetTemplateFieldAppearance(fieldTemplateView);
            }
        }

        /// <summary>
        /// Sets the appearance of form field template view.
        /// </summary>
        /// <param name="fieldTemplateView">The form field template view.</param>
        private void SetTemplateFieldAppearance(WpfFormFieldTemplateView fieldTemplateView)
        {
            if (fieldTemplateView is WpfFormFieldTemplateGroupView)
                foreach (WpfFormFieldTemplateView nestedFieldView in ((WpfFormFieldTemplateGroupView)fieldTemplateView).ViewItems)
                    SetTemplateFieldAppearance(nestedFieldView);

            if (fieldTemplateView.Pen != null)
            {
                SolidColorBrush brush = ((SolidColorBrush)fieldTemplateView.Pen.Brush);
                brush.Color = Color.FromArgb(brush.Color.A, 255, 0, 0);
            }
        }

        /// <summary>
        /// Sets OMR rectangular field template appearance.
        /// </summary>
        /// <param name="omrRectangularFieldTemplateView">View of the OMR rectagular field template.</param>
        private void SetOmrRectangularFieldTemplateAppearance(
            WpfOmrRectangularFieldTemplateView omrRectangularFieldTemplateView)
        {
            omrRectangularFieldTemplateView.Brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 144, 238, 144));

            omrRectangularFieldTemplateView.Pen = new System.Windows.Media.Pen(
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 255, 0, 0)), 1.0);
        }

        /// <summary>
        /// Sets OMR elliptical field template appearance.
        /// </summary>
        /// <param name="omrEllipticalFieldTemplateView">View of the OMR elliptical field template.</param>
        private void SetOmrEllipticalFieldTemplateAppearance(
            WpfOmrEllipticalFieldTemplateView omrEllipticalFieldTemplateView)
        {
            omrEllipticalFieldTemplateView.Brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 144, 238, 144));

            omrEllipticalFieldTemplateView.Pen = new System.Windows.Media.Pen(
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 255, 0, 0)), 1.0);
        }

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Sets OCR field template appearance.
        /// </summary>
        /// <param name="ocrFieldTemplateView">View of the OCR field template.</param>
        private void SetOcrFieldTemplateAppearance(WpfOcrFieldTemplateView ocrFieldTemplateView)
        {
            ocrFieldTemplateView.Brush = new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(50, 0, 200, 255));

            ocrFieldTemplateView.Pen = new System.Windows.Media.Pen(new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(150, 255, 0, 0)), 1f);
        }
#endif

        /// <summary>
        /// Returns a copy of focused form field template view.
        /// </summary>
        private WpfFormFieldTemplateView GetFocusedFieldTemplateCopy()
        {
            WpfFormFieldTemplateView focusedFieldTemplateView = _fieldTemplateEditorTool.FocusedFieldTemplateView;
            if (focusedFieldTemplateView == null)
                return null;

            return (WpfFormFieldTemplateView)focusedFieldTemplateView.Clone();
        }

        #endregion


        #region Template tree view

        /// <summary>
        /// Adds a node in the pages tree.
        /// </summary>
        /// <param name="addedImage">Image of the page template.</param>
        /// <param name="templateName">Name of the page template.</param>
        private void AddPageTreeNode(VintasoftImage addedImage, string templateName)
        {
            TreeViewItem addedImageNode = new TreeViewItem();
            addedImageNode.Tag = addedImage;
            addedImageNode.Header = templateName;
            _imagesToTemplateTreeNodes.Add(addedImage, addedImageNode);
            _templatesRootNode.Items.Add(addedImageNode);
            _templatesRootNode.IsExpanded = true;
        }

        private void AddFormFieldTemplateTreeNode(VintasoftImage image, FormFieldTemplate fieldTemplate)
        {
            TreeViewItem rootNode = _imagesToTemplateTreeNodes[image];
            AddFormFieldTemplateTreeNode(rootNode, fieldTemplate);
        }

        private void AddFormFieldTemplateTreeNode(TreeViewItem root, FormFieldTemplate fieldTemplate)
        {
            TreeViewItem node = new TreeViewItem();
            // set the text of the tree node
            SetFormFieldTemplateTreeNodeText(node, fieldTemplate);
            _formFieldTemplateToTreeNodes.Add(fieldTemplate, node);
            node.Tag = fieldTemplate;
            root.Items.Add(node);
            root.IsExpanded = true;

            if (fieldTemplate is FormFieldTemplateGroup &&
                !(fieldTemplate is OmrFieldTemplateTable))
            {
                FormFieldTemplateGroup group = fieldTemplate as FormFieldTemplateGroup;
                foreach (FormFieldTemplate item in group.Items)
                    AddFormFieldTemplateTreeNode(node, item);
            }
        }

        private void RemoveFormFieldTemplateTreeNode(FormFieldTemplate fieldTemplate)
        {
            TreeViewItem node = _formFieldTemplateToTreeNodes[fieldTemplate];
            node.Tag = null;
            TreeViewItem parent = (TreeViewItem)node.Parent;
            parent.Items.Remove(node);
            _formFieldTemplateToTreeNodes.Remove(fieldTemplate);

            if (fieldTemplate is FormFieldTemplateGroup &&
              !(fieldTemplate is OmrFieldTemplateTable))
            {
                FormFieldTemplateGroup group = fieldTemplate as FormFieldTemplateGroup;
                foreach (FormFieldTemplate item in group.Items)
                    RemoveFormFieldTemplateTreeNode(item);
            }
        }

        private void RemoveFormFieldTemplateTreeNode(TreeViewItem node)
        {
            for (int i = node.Items.Count - 1; i >= 0; i--)
                RemoveFormFieldTemplateTreeNode(node.Items[i] as TreeViewItem);
            TreeViewItem parent = (TreeViewItem)node.Parent;
            parent.Items.Remove(node);
            FormFieldTemplate fieldTemplate = node.Tag as FormFieldTemplate;
            _formFieldTemplateToTreeNodes.Remove(fieldTemplate);
        }

        /// <summary>
        /// Sets default root node of the pages tree.
        /// </summary>
        private void SetDefaultRootNode()
        {
            _templatesRootNode.Header = "Untitled";
        }

        /// <summary>
        /// Updates names of tree nodes of subtree.
        /// </summary>
        /// <param name="node">The root node of the subtree.</param>
        private void UpdateNames(TreeViewItem node)
        {
            if (node.Tag is FormFieldTemplate)
            {
                FormFieldTemplate fieldTemplate = (FormFieldTemplate)node.Tag;
                // update the text of the tree node and 
                SetFormFieldTemplateTreeNodeText(node, fieldTemplate);
                // for each subnode
                foreach (object subnode in node.Items)
                    // update names of the subnode
                    UpdateNames((TreeViewItem)subnode);
            }
        }

        /// <summary>
        /// Sets the text of form field template tree node.
        /// </summary>
        /// <param name="node">The tree node of the form field template.</param>
        /// <param name="fieldTemplate">The form field template.</param>
        private void SetFormFieldTemplateTreeNodeText(TreeViewItem node, FormFieldTemplate fieldTemplate)
        {
            string text;
            if (string.IsNullOrEmpty(fieldTemplate.Name))
                // get name of the type
                text = fieldTemplate.GetType().Name;
            else
                // get name of the template
                text = fieldTemplate.Name;
            // if text is different
            if (!node.HasHeader || node.Header.ToString() != text)
                // set the text
                node.Header = text;
        }

        /// <summary>
        /// Handles the PropertyChanged event of the group of form field templates.
        /// </summary>
        private void templateGroup_PropertyChanged(object sender, ObjectPropertyChangedEventArgs e)
        {
            // if items are changed
            if (e.PropertyName == "Items")
            {
                // get the template group
                FormFieldTemplate templateGroup = sender as FormFieldTemplate;
                // get the tree node of the group
                TreeViewItem node = _formFieldTemplateToTreeNodes[templateGroup];
                // update the names
                UpdateNames(node);
            }
        }

        /// <summary>
        /// Handles the PropertyValueChanged event of propertyGrid1 object.
        /// </summary>
        void propertyGrid1_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label != "Name" && e.ChangedItem.Label != "DocumentName")
                return;

            TreeViewItem item = null;
            string name = (string)e.ChangedItem.Value;

            if (propertyGrid1.SelectedObject is FormPageTemplate)
            {
                item = _imagesToTemplateTreeNodes[imageViewer1.Image];
                if (string.IsNullOrEmpty(name))
                {
                    FormPageTemplate template = (FormPageTemplate)propertyGrid1.SelectedObject;
                    name = template.ImageFileName;
                }
                item.Header = name;
            }
            else if (propertyGrid1.SelectedObject is FormFieldTemplate)
            {
                item = _formFieldTemplateToTreeNodes[(FormFieldTemplate)propertyGrid1.SelectedObject];
                if (string.IsNullOrEmpty(name))
                    name = propertyGrid1.SelectedObject.GetType().Name;
                item.Header = name;

            }
            else if (propertyGrid1.SelectedObject is FormTemplateManager)
            {
                item = _templatesRootNode;
                if (string.IsNullOrEmpty(name))
                    SetDefaultRootNode();
                else
                    item.Header = name;
            }
        }

        /// <summary>
        /// Handles the SelectedItemChanged event of templatesTreeView object.
        /// </summary>
        void templatesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = (TreeViewItem)e.NewValue;
            if (item == _templatesRootNode)
            {
                SetSelectedObjectInPropertyGrid(_templateManager);
            }
            // if current node is a node of an image
            else if (item.Tag is VintasoftImage)
            {
                // set it as a focused image

                VintasoftImage image = item.Tag as VintasoftImage;
                int indexOfImage = imageViewer1.Images.IndexOf(image);
                if (indexOfImage >= 0)
                {
                    if (imageViewer1.FocusedIndex != indexOfImage)
                        imageViewer1.FocusedIndex = indexOfImage;

                    FormPageTemplate template = _templateManager.GetPageTemplate(image);
                    SetSelectedObjectInPropertyGrid(template);
                }
            }
            else if (item.Tag is FormFieldTemplate)
            {
                TreeViewItem node = (TreeViewItem)item.Parent;
                while (node != null)
                {
                    if (node.Tag is VintasoftImage)
                    {
                        VintasoftImage image = node.Tag as VintasoftImage;
                        int indexOfImage = imageViewer1.Images.IndexOf(image);
                        if (imageViewer1.FocusedIndex != indexOfImage)
                            imageViewer1.FocusedIndex = indexOfImage;
                        break;
                    }
                    node = (TreeViewItem)node.Parent;
                }

                FormFieldTemplate fieldTemplate = item.Tag as FormFieldTemplate;
                if (_fieldTemplateEditorTool.FieldTemplateViewCollection != null)
                    _fieldTemplateEditorTool.FocusedFieldTemplateView =
                        _fieldTemplateEditorTool.FieldTemplateViewCollection.FindView(fieldTemplate);
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of templatesTreeView object.
        /// </summary>
        void templatesTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (templatesTreeView.SelectedItem != null)
            {
                TreeViewItem item = templatesTreeView.SelectedItem as TreeViewItem;
                if (item.Tag is FormFieldTemplate)
                    fieldTemplateEditorTool_MouseDoubleClick(sender, null);
            }
        }

        #endregion


        #region Hot keys

        /// <summary>
        /// Handles the CanExecute event of cutCommandBinding object.
        /// </summary>
        private void cutCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = cutMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of copyCommandBinding object.
        /// </summary>
        private void copyCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = copyMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of pasteCommandBinding object.
        /// </summary>
        private void pasteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = pasteMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of deleteAllCommandBinding object.
        /// </summary>
        private void deleteAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = deleteAllMenuItem.IsEnabled;
        }

        #endregion

        #endregion

    }
}
