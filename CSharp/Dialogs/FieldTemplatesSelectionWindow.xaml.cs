using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.FormsProcessing.FormRecognition;


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A window that allows to select the form field template.
    /// </summary>
    public partial class FieldTemplatesSelectionWindow : Window
    {

        #region Fields

        /// <summary>
        /// The field templates.
        /// </summary>
        IList<FormFieldTemplate> _fieldTemplates;

        #endregion



        #region Constructors

        private FieldTemplatesSelectionWindow()
        {
            InitializeComponent();
        }

        public FieldTemplatesSelectionWindow(IList<FormFieldTemplate> fieldTemplates)
            : this()
        {
            _fieldTemplates = fieldTemplates;
            for (int i = 0; i < fieldTemplates.Count; i++)
            {
                FormFieldTemplate fieldTemplate = fieldTemplates[i];
                string text;
                if (string.IsNullOrEmpty(fieldTemplate.Name))
                    text = string.Format("{0}, {1}", fieldTemplate.GetType().Name, fieldTemplate.BoundingBox);
                else
                    text = string.Format("{0}, {1}", fieldTemplate.Name, fieldTemplate.BoundingBox);

                CheckBox checkBox = new CheckBox();
                checkBox.Content = text;
                fieldTemplatesCheckedListBox.Items.Add(checkBox);
            }
        }

        #endregion



        #region Properties

        IList<FormFieldTemplate> _selectedFieldTemplates;
        /// <summary>
        /// Gets the selected field templates.
        /// </summary>
        public IList<FormFieldTemplate> SelectedFieldTemplates
        {
            get
            {
                return _selectedFieldTemplates;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of BtOk object.
        /// </summary>
        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            _selectedFieldTemplates = new List<FormFieldTemplate>();

            for (int i = 0; i < _fieldTemplates.Count; i++)
            {
                object item = fieldTemplatesCheckedListBox.Items[i];
                if (item is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)item;
                    if (checkBox.IsChecked.Value)
                        _selectedFieldTemplates.Add(_fieldTemplates[i]);
                }
            }

            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of BtCancel object.
        /// </summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
