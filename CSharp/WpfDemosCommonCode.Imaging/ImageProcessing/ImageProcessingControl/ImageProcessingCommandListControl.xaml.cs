using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.ImageProcessing;


namespace WpfDemosCommonCode
{
    /// <summary>
    /// A control that allows to view the available image processing commands.
    /// </summary>
    public partial class ImageProcessingCommandListControl : UserControl
    {

        #region Constants

        /// <summary>
        /// The value, from command type list, that defines that all commands must be shown.
        /// </summary>
        private const string ALL_COMMANDS_KEY = "All";

        #endregion



        #region Fields

        /// <summary>
        /// The commands that must be shown in list box.
        /// </summary>
        List<ProcessingCommandBase> _commandListBoxItems = new List<ProcessingCommandBase>();

        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessingCommandListControl"/> class.
        /// </summary>
        public ImageProcessingCommandListControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        private Dictionary<string, ProcessingCommandBase[]> _availableProcessingCommands = null;
        /// <summary>
        /// Gets or sets the image processing commands, which can be shown in this control.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        [Browsable(false)]
        public Dictionary<string, ProcessingCommandBase[]> AvailableProcessingCommands
        {
            get
            {
                return _availableProcessingCommands;
            }
            set
            {
                if (_availableProcessingCommands != value)
                {
                    _availableProcessingCommands = value;

                    UpdateProcessingCommands();

                    commandListBox.IsEnabled = _availableProcessingCommands != null;
                }
            }
        }

        /// <summary>
        /// Gets the selected image processing command.
        /// </summary>
        [Browsable(false)]
        public ProcessingCommandBase SelectedProcessingCommand
        {
            get
            {
                if (commandListBox.SelectedIndex == -1)
                    return null;
                else
                    return _commandListBoxItems[commandListBox.SelectedIndex];
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the processing commands.
        /// </summary>
        private void UpdateProcessingCommands()
        {
            if (_availableProcessingCommands != null && _availableProcessingCommands.Count > 1)
            {
                string[] commandTypeNames = GetCommandTypeNames();

                InitCommandTypeComboBox(commandTypeNames);
            }
            else
            {
                commandTypesPanel.Visibility = Visibility.Collapsed;

                UpdateCommandListBox(ALL_COMMANDS_KEY);
            }
        }

        /// <summary>
        /// Returns the command type names.
        /// </summary>
        /// <returns>The command type names.</returns>
        private string[] GetCommandTypeNames()
        {
            string[] commandTypeNames = new string[_availableProcessingCommands.Count];
            _availableProcessingCommands.Keys.CopyTo(commandTypeNames, 0);

            for (int i = 0; i < commandTypeNames.Length; i++)
            {
                if (string.IsNullOrEmpty(commandTypeNames[i]))
                    throw new ArgumentException("Command type can not be empty.");
            }

            return commandTypeNames;
        }

        /// <summary>
        /// Inits the command type list box.
        /// </summary>
        /// <param name="commandTypeNames">The command type names.</param>
        private void InitCommandTypeComboBox(string[] commandTypeNames)
        {
            commandTypesPanel.Visibility = Visibility.Visible;

            commandTypeComboBox.BeginInit();
            try
            {
                commandTypeComboBox.Items.Clear();
                commandTypeComboBox.Items.Add(ALL_COMMANDS_KEY);

                foreach (string commandTypeName in commandTypeNames)
                {
                    if (string.IsNullOrEmpty(commandTypeName))
                        throw new ArgumentException("Command type can not be empty.");
                    commandTypeComboBox.Items.Add(commandTypeName);
                }

                commandTypeComboBox.SelectedItem = ALL_COMMANDS_KEY;
            }
            finally
            {
                commandTypeComboBox.EndInit();
            }
        }

        /// <summary>
        /// The type, of image processing commands, is changed.
        /// </summary>
        private void commandTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCommandListBox((string)commandTypeComboBox.SelectedItem);
        }

        /// <summary>
        /// Updates the list box with image processing commands.
        /// </summary>
        /// <param name="commandType">The type of commands.</param>
        private void UpdateCommandListBox(string commandType)
        {
            _commandListBoxItems.Clear();
            commandListBox.BeginInit();
            commandListBox.Items.Clear();

            try
            {
                if (_availableProcessingCommands == null)
                    return;

                // if all commands must be shown in list box
                if (commandType == ALL_COMMANDS_KEY)
                {
                    foreach (string typeName in _availableProcessingCommands.Keys)
                    {
                        ProcessingCommandBase[] processingCommands = _availableProcessingCommands[typeName];
                        AddProcessingCommandToCommandListBox(typeName, processingCommands);
                    }
                }
                // if only commands of specified type must be shown in list box
                else
                {
                    ProcessingCommandBase[] processingCommands = _availableProcessingCommands[commandType];
                    AddProcessingCommandToCommandListBox(commandType, processingCommands);
                }
            }
            finally
            {
                commandListBox.EndInit();
            }
        }

        /// <summary>
        /// Adds the processing command to command ListBox.
        /// </summary>
        /// <param name="commandTypeName">Type of the command.</param>
        /// <param name="processingCommands">The processing commands.</param>
        private void AddProcessingCommandToCommandListBox(
            string commandTypeName,
            params ProcessingCommandBase[] processingCommands)
        {
            string commandNamePrefix = GetCommandPrefix(commandTypeName);

            foreach (ProcessingCommandBase processingCommand in processingCommands)
            {
                string commandName = commandNamePrefix + processingCommand.Name;
                commandListBox.Items.Add(commandName);
                _commandListBoxItems.Add(processingCommand);
            }
        }

        /// <summary>
        /// Returns the command prefix.
        /// </summary>
        /// <param name="commandTypeName">Type of the command.</param>
        private string GetCommandPrefix(string commandTypeName)
        {
            string commandNamePrefix = string.Empty;
            if (_availableProcessingCommands.Count > 1)
            {
                foreach (char symbol in commandTypeName)
                {
                    if (char.IsUpper(symbol))
                        commandNamePrefix += symbol;
                }

                if (!string.IsNullOrEmpty(commandNamePrefix))
                    commandNamePrefix = string.Format("[{0}] ", commandNamePrefix);
            }

            return commandNamePrefix;
        }

        /// <summary>
        /// Selected processing command is changed.
        /// </summary>
        private void commandListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedProcessingCommandChanged != null)
                SelectedProcessingCommandChanged(this, e);
        }

        /// <summary>
        /// Selected processing command is double clicked.
        /// </summary>
        private void commandListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (MouseDoubleClickOnSelectedProcessingCommand != null)
                    MouseDoubleClickOnSelectedProcessingCommand(this, e);
        }

        #endregion



        #region Events

        /// <summary>
        /// Occurs when selected processing command is changed.
        /// </summary>
        public event EventHandler SelectedProcessingCommandChanged;

        /// <summary>
        /// Occurs when selected processing command is double clicked.
        /// </summary>
        public event EventHandler MouseDoubleClickOnSelectedProcessingCommand;

        #endregion

    }
}
