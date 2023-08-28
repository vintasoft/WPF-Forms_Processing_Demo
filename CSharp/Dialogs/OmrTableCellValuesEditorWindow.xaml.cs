using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.FormsProcessing.FormRecognition.Omr;

using WpfDemosCommonCode;


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A window that allows to edit OMR table cell values.
    /// </summary>
    public partial class OmrTableCellValuesEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// Row count in a table.
        /// </summary>
        int _rowCount;

        /// <summary>
        /// Column count in a table.
        /// </summary>
        int _columnCount;

        /// <summary>
        /// Two dimensional source array of values.
        /// </summary>
        string[,] _cellValues;

        TextBox[,] _grid;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OmrTableCellValuesEditorWindow"/> class.
        /// </summary>
        public OmrTableCellValuesEditorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OmrTableCellValuesEditorWindow"/> class.
        /// </summary>
        /// <param name="cellValues">Two dimensional source array of values.</param>
        /// <param name="orientation">Orientation of OMR table.</param>
        public OmrTableCellValuesEditorWindow(string[,] cellValues, OmrTableOrientation orientation)
            : this()
        {
            _rowCount = cellValues.GetLength(0);
            _columnCount = cellValues.GetLength(1);
            _cellValues = cellValues;
            Orientation = orientation;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets orientation of OMR table.
        /// </summary>
        public OmrTableOrientation Orientation
        {
            get
            {
                return horizontalRadioButton.IsChecked.Value ?
                    OmrTableOrientation.Horizontal :
                    OmrTableOrientation.Vertical;
            }
            set
            {
                horizontalRadioButton.IsChecked = value == OmrTableOrientation.Horizontal;
                verticalRadioButton.IsChecked = !horizontalRadioButton.IsChecked;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Fills the cells with current values from source.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitTable(_columnCount, _rowCount);

            for (int row = 0; row < _rowCount; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                {
                    _grid[row, col].Text = _cellValues[row, col];
                }
            }
        }

        /// <summary>
        /// Fills cells with capital letters of Roman alphabet along the rows or columns.
        /// </summary>
        private void fillValuesAbcButton_Click(object sender, RoutedEventArgs e)
        {
            int letterCount;
            byte aLetterCode = (byte)'A';
            if (Orientation == OmrTableOrientation.Horizontal)
            {
                letterCount = Math.Min(_columnCount, 26);
                for (int r = 0; r < _rowCount; r++)
                {
                    for (int c = 0; c < letterCount; c++)
                    {
                        _grid[r, c].Text = Convert.ToChar((byte)(aLetterCode + c)).ToString();
                    }
                }

                if (_columnCount > 26)
                {
                    for (int r = 0; r < _rowCount; r++)
                    {
                        for (int c = 26; c < _columnCount; c++)
                        {
                            _grid[r, c].Text = "";
                        }
                    }

                    DemosTools.ShowWarningMessage("Column count exceeds number of capital letters." +
                        " Remaining columns are unfilled.");
                }
            }
            else
            {
                letterCount = Math.Min(_rowCount, 26);
                for (int r = 0; r < letterCount; r++)
                {
                    string rowValue = Convert.ToChar((byte)(aLetterCode + r)).ToString();
                    for (int c = 0; c < _columnCount; c++)
                    {
                        _grid[r, c].Text = rowValue;
                    }
                }

                if (_rowCount > 26)
                {
                    for (int r = 26; r < _rowCount; r++)
                    {
                        for (int c = 0; c < _columnCount; c++)
                        {
                            _grid[r, c].Text = "";
                        }
                    }

                    DemosTools.ShowWarningMessage("Row count exceeds number of capital letters." +
                        " Remaining rows are unfilled.");
                }
            }
        }

        /// <summary>
        /// Fills cells with numbers starting from zero along the rows or columns.
        /// </summary>
        private void fillValues123Button_Click(object sender, RoutedEventArgs e)
        {
            if (Orientation == OmrTableOrientation.Horizontal)
            {
                for (int r = 0; r < _rowCount; r++)
                {
                    for (int c = 0; c < _columnCount; c++)
                    {
                        _grid[r, c].Text = c.ToString();
                    }
                }
            }
            else
            {
                for (int r = 0; r < _rowCount; r++)
                {
                    string rowValue = r.ToString();
                    for (int c = 0; c < _columnCount; c++)
                    {
                        _grid[r, c].Text = rowValue;
                    }
                }
            }
        }

        /// <summary>
        /// Copies actual values of cells to source array and closes form.
        /// </summary>
        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            for (int row = 0; row < _rowCount; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                {
                    _cellValues[row, col] = _grid[row, col].Text;
                }
            }

            DialogResult = true;
        }

        /// <summary>
        /// Closes form without copying cell values.
        /// </summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void InitTable(int columnCount, int rowCount)
        {
            Grid grid = table;
            Grid.SetColumn(grid, 0);
            Grid.SetRow(grid, 0);
            grid.BeginInit();

            double rowWidth = 50.0;
            double rowMinWidth = rowWidth / 2;
            double columnHeight = 23.0;
            double columnMinHeight = columnHeight / 2;

            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            ColumnDefinition colDef;
            for (int i = 0; i < columnCount + 1; i++)
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(rowWidth, GridUnitType.Pixel);
                colDef.MinWidth = rowMinWidth;
                grid.ColumnDefinitions.Add(colDef);
            }
            colDef = new ColumnDefinition();
            colDef.Width = new GridLength(1, GridUnitType.Star);
            colDef.MinWidth = rowMinWidth;
            grid.ColumnDefinitions.Add(colDef);

            RowDefinition rowDef;
            for (int j = 0; j < rowCount + 1; j++)
            {
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(columnHeight, GridUnitType.Pixel);
                rowDef.MinHeight = columnMinHeight;
                grid.RowDefinitions.Add(rowDef);
            }
            rowDef = new RowDefinition();
            rowDef.Height = new GridLength(1, GridUnitType.Star);
            rowDef.MinHeight = columnMinHeight;
            grid.RowDefinitions.Add(rowDef);

            double spliterWidth = 2.0;
            double spliterHeight = 2.0;

            _grid = new TextBox[rowCount, columnCount];
            grid.Children.Clear();
            for (int col = 0; col < columnCount; col++)
            {
                Label label = new Label();
                label.VerticalAlignment = VerticalAlignment.Center;
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.Content = string.Format(string.Format("Col.{0}", col + 1));
                Grid.SetColumn(label, col + 1);
                Grid.SetRow(label, 0);
                grid.Children.Add(label);
                for (int row = 0; row < rowCount; row++)
                {
                    TextBox textBox = new TextBox();
                    textBox.Margin = new Thickness(0, 0, spliterWidth, spliterHeight);
                    textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    textBox.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetColumn(textBox, col + 1);
                    Grid.SetRow(textBox, row + 1);
                    grid.Children.Add(textBox);
                    _grid[row, col] = textBox;
                }
            }

            for (int i = 1; i < columnCount + 1; i++)
            {
                GridSplitter spliter = new GridSplitter();
                spliter.VerticalAlignment = VerticalAlignment.Stretch;
                spliter.HorizontalAlignment = HorizontalAlignment.Right;
                spliter.Width = spliterWidth;
                Grid.SetColumn(spliter, i);
                Grid.SetRow(spliter, 0);
                grid.Children.Add(spliter);
            }

            for (int j = 1; j < rowCount + 1; j++)
            {
                GridSplitter spliter = new GridSplitter();
                spliter.VerticalAlignment = VerticalAlignment.Bottom;
                spliter.HorizontalAlignment = HorizontalAlignment.Stretch;
                spliter.Height = spliterHeight;
                Grid.SetRow(spliter, j);
                Grid.SetColumn(spliter, 0);
                grid.Children.Add(spliter);
            }
            grid.EndInit();
        }

        #endregion

    }
}
