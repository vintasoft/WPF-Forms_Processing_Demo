using System;
using System.Windows;


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A window that allows to specify parameters of new OMR table.
    /// </summary>
    public partial class NewTableSetupWindow : Window
    {

        public NewTableSetupWindow(int initialRowCount, int initialColumnCount)
        {
            InitializeComponent();

            rowCountNumericUpDown.Value = Math.Min(initialRowCount, rowCountNumericUpDown.Maximum);
            columnCountNumericUpDown.Value = Math.Min(initialColumnCount, columnCountNumericUpDown.Maximum);      
        }



        public int RowCount
        {
            get
            {
                return (int)rowCountNumericUpDown.Value;
            }
        }

        public int ColumnCount
        {
            get
            {
                return (int)columnCountNumericUpDown.Value;
            }
        }



        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}
