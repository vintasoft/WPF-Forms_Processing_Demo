using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.ImageProcessing;

using WpfDemosCommonCode.Imaging;


namespace WpfDemosCommonCode
{
    /// <summary>
    /// A control that allows to select the image processing commands.
    /// </summary>
    public partial class ImageProcessingCommandSelectionControl : UserControl
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="ImageProcessingCommandSelectionControl"/> class.
        /// </summary>
        public ImageProcessingCommandSelectionControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the available processing commands.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        [Browsable(false)]
        public Dictionary<string, ProcessingCommandBase[]> AvailableProcessingCommands
        {
            get
            {
                return imageProcessingCommandsViewer1.AvailableProcessingCommands;
            }
            set
            {
                imageProcessingCommandsViewer1.AvailableProcessingCommands = value;
                UpdateUI();
            }
        }

        /// <summary>
        /// Gets or sets the selected commands.
        /// </summary>
        [Browsable(false)]
        public ProcessingCommandBase[] SelectedCommands
        {
            get
            {
                if (commandsToProcessListBox.Items.Count == 0)
                    return null;

                ProcessingCommandBase[] commands =
                    new ProcessingCommandBase[commandsToProcessListBox.Items.Count];

                for (int i = 0; i < commands.Length; i++)
                    commands[i] = (ProcessingCommandBase)commandsToProcessListBox.Items[i];

                return commands;
            }
            set
            {
                commandsToProcessListBox.BeginInit();
                commandsToProcessListBox.Items.Clear();
                if (value != null)
                {
                    foreach (ProcessingCommandBase command in value)
                        commandsToProcessListBox.Items.Add(command);
                }
                commandsToProcessListBox.EndInit();

                UpdateUI();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            bool isAddCommandSelected = imageProcessingCommandsViewer1.SelectedProcessingCommand != null;
            bool isCommandToProcessSelected = commandsToProcessListBox.SelectedIndex != -1;
            bool isFirstCommandToProcessSelected = commandsToProcessListBox.SelectedIndex == 0;
            bool isLastCommandToProcessSelected =
                commandsToProcessListBox.SelectedIndex == commandsToProcessListBox.Items.Count - 1;
            bool isCommandsToProcessSpecified = commandsToProcessListBox.Items.Count > 0;

            addCommandToListButton.IsEnabled = isAddCommandSelected;

            removeCommandFromListButton.IsEnabled = isCommandToProcessSelected;
            removeAllCommandsFromList.IsEnabled = isCommandsToProcessSpecified;
            moveUpButton.IsEnabled = isCommandToProcessSelected && !isFirstCommandToProcessSelected;
            moveDownButton.IsEnabled = isCommandToProcessSelected && !isLastCommandToProcessSelected;
            setCommandPropertiesButton.IsEnabled = isCommandToProcessSelected;
        }

        /// <summary>
        /// Selected processing command is changed.
        /// </summary>
        private void imageProcessingCommandsViewer1_SelectedProcessingCommandChanged(
            object sender,
            EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Selected processing command is double clicked.
        /// </summary>
        private void imageProcessingCommandsViewer1_MouseDoubleClickOnSelectedProcessingCommand(
            object sender,
            EventArgs e)
        {
            if (imageProcessingCommandsViewer1.SelectedProcessingCommand != null)
                AddSelectedCommandToProcessList();
        }

        /// <summary>
        /// The processing command is added to the list of image processing commands.
        /// </summary>
        private void addCommandToListButton_Click(object sender, RoutedEventArgs e)
        {
            AddSelectedCommandToProcessList();
        }

        /// <summary>
        /// Adds the selected processing command to the list of image processing commands.
        /// </summary>
        private void AddSelectedCommandToProcessList()
        {
            ProcessingCommandBase command = imageProcessingCommandsViewer1.SelectedProcessingCommand;
            try
            {
                commandsToProcessListBox.Items.Add((ProcessingCommandBase)command.Clone());
                OnProcessingCommandsChanged();
            }
            catch (Exception exc)
            {
                DemosTools.ShowErrorMessage(exc);
            }
            UpdateUI();
        }

        /// <summary>
        /// Removes the selected processing command from the list of image processing commands.
        /// </summary>
        private void removeCommandFromListButton_Click(object sender, RoutedEventArgs e)
        {
            int index = commandsToProcessListBox.SelectedIndex;
            commandsToProcessListBox.Items.RemoveAt(index);

            if (index >= commandsToProcessListBox.Items.Count)
                index--;
            commandsToProcessListBox.SelectedIndex = index;
            OnProcessingCommandsChanged();

            UpdateUI();
        }

        /// <summary>
        /// Removes all processing command from the list of image processing commands.
        /// </summary>
        private void removeAllCommandsFromList_Click(object sender, RoutedEventArgs e)
        {
            commandsToProcessListBox.Items.Clear();
            OnProcessingCommandsChanged();

            UpdateUI();
        }

        /// <summary>
        /// Processing command is moved up in the list of image processing commands.
        /// </summary>
        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveListBoxSelectedItem(commandsToProcessListBox, -1);
            OnProcessingCommandsChanged();

            UpdateUI();
        }

        /// <summary>
        /// Processing command is moved down in the list of image processing commands.
        /// </summary>
        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveListBoxSelectedItem(commandsToProcessListBox, 1);
            OnProcessingCommandsChanged();

            UpdateUI();
        }

        /// <summary>
        /// Moves processing command, in the list of image processing commands, at delta positions.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        /// <param name="delta">The delta.</param>
        private void MoveListBoxSelectedItem(ListBox listBox, int delta)
        {
            object item = listBox.SelectedItem;
            listBox.BeginInit();
            int index = listBox.SelectedIndex;
            listBox.Items.RemoveAt(index);
            int newIndex = index + delta;
            listBox.Items.Insert(newIndex, item);
            listBox.SelectedIndex = newIndex;
            listBox.EndInit();
        }

        /// <summary>
        /// Selected processing command is double clicked.
        /// </summary>
        private void commandsToProcessListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (commandsToProcessListBox.SelectedItem != null)
                SetCommandProperties((ProcessingCommandBase)commandsToProcessListBox.SelectedItem);
        }

        /// <summary>
        /// Sets the processing command properties.
        /// </summary>
        private void setCommandPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            SetCommandProperties((ProcessingCommandBase)commandsToProcessListBox.SelectedItem);
        }

        /// <summary>
        /// Sets the processing command properties.
        /// </summary>
        /// <param name="command">The command.</param>
        private void SetCommandProperties(ProcessingCommandBase command)
        {
            PropertyGridWindow window = new PropertyGridWindow(
                command,
            string.Format("{0} Properties", command.Name));

            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();

            OnProcessingCommandsChanged();
        }

        /// <summary>
        /// Selected processing command, in the list of image processing commands, is changed.
        /// </summary>
        private void commandsToProcessListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Raizes
        /// the <see cref="E:DemosCommonCode.ImageProcessingCommandsEditor.ProcessingCommandsChanged" />
        /// event.
        /// </summary>
        private void OnProcessingCommandsChanged()
        {
            if (ProcessingCommandsChanged != null)
                ProcessingCommandsChanged(this, EventArgs.Empty);
        }

        #endregion



        #region Events

        /// <summary>
        /// Occurs when the processing commands is changed.
        /// </summary>
        public event EventHandler ProcessingCommandsChanged;

        #endregion

    }
}
