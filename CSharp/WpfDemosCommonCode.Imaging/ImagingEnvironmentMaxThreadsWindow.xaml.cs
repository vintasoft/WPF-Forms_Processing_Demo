using System;
using System.Windows;

using Vintasoft.Imaging;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Represents the editor of ImagingEnvironment.MaxThreads property.
    /// </summary>
    public partial class ImagingEnvironmentMaxThreadsWindow : Window
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagingEnvironmentMaxThreadsWindow"/> class.
        /// </summary>
        public ImagingEnvironmentMaxThreadsWindow()
        {
            InitializeComponent();

            maxThreadsSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(maxThreadsSlider_ValueChanged);
            maxThreadsNumericUpDown.ValueChanged += new EventHandler<EventArgs>(maxThreadsNumericUpDown_ValueChanged);
            
            // set max threads value
            maxThreadsSlider.Value = ImagingEnvironment.MaxThreads;
        }

        #endregion



        #region Methods

        /// <summary>
        /// Sets curent settings to ImagingEnvironment.MaxThreads.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            ImagingEnvironment.MaxThreads = (int)maxThreadsSlider.Value;
            DialogResult = true;
        }

        /// <summary>
        /// Close the dialog.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Reset ImagingEnvironment.MaxThreads value.
        /// </summary>
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            maxThreadsSlider.Value = ImagingEnvironment.MaxThreads;
        }

        /// <summary>
        /// Slider is changed.
        /// </summary>
        private void maxThreadsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (maxThreadsNumericUpDown.Value != maxThreadsSlider.Value)
                maxThreadsNumericUpDown.Value = (int)maxThreadsSlider.Value;
        }

        /// <summary>
        /// NumericUpDown is changed.
        /// </summary>
        private void maxThreadsNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (maxThreadsNumericUpDown.Value != maxThreadsSlider.Value)
                maxThreadsSlider.Value = (int)maxThreadsNumericUpDown.Value;
        }

        #endregion

    }
}
