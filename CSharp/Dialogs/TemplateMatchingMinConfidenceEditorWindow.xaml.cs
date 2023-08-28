using System.Windows;
using Vintasoft.Imaging.FormsProcessing.TemplateMatching;

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A form that allows to change MinConfidence value of template matching command.
    /// </summary>
    public partial class TemplateMatchingMinConfidenceEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// The template matching command.
        /// </summary>
        TemplateMatchingCommand _templateMatching;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateMatchingMinConfidenceEditorWindow"/> class.
        /// </summary>
        public TemplateMatchingMinConfidenceEditorWindow(TemplateMatchingCommand command)
        {
            InitializeComponent();

            _templateMatching = command;
            minConfidenceSlider.Value = (int)(_templateMatching.MinConfidence * 100);
        }

        #endregion



        #region Methods

        /// <summary>
        /// Changes MinConfidence value of template matching command.
        /// </summary>
        private void MinConfidenceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            minConfidenceValueLabel.Content = minConfidenceSlider.Value.ToString() + "%";
        }

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            _templateMatching.MinConfidence = (float)((minConfidenceSlider.Value) / 100);
            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
