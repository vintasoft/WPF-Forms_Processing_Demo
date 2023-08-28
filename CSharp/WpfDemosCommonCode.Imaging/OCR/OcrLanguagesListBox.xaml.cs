using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Ocr;
#endif


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A control that allows to change the selected <see cref="Vintasoft.Imaging.Ocr.OcrLanguage"/>.
    /// </summary>
    public partial class OcrLanguagesListBox : UserControl
    {

        #region Fields

        /// <summary>
        /// Indicates whether the selected languages are changing.
        /// </summary>
        bool _isSelectedLanguagesChanging = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrLanguagesListBox"/> class.
        /// </summary>
        public OcrLanguagesListBox()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

#if !REMOVE_OCR_PLUGIN
        OcrLanguage[] _availableLanguages = null;
        /// <summary>
        /// Gets or sets the available languages. 
        /// </summary>
        public OcrLanguage[] AvailableLanguages
        {
            get
            {
                return _availableLanguages;
            }
            set
            {
                // if languages are changed
                if (_availableLanguages != value)
                {
                    _availableLanguages = value;

                    // begin update the control
                    languagesCheckedListBox.BeginInit();

                    // clear items
                    languagesCheckedListBox.Items.Clear();

                    // if items must be added
                    if (_availableLanguages != null)
                    {
                        OcrLanguage[] sortedLanguages = (OcrLanguage[])_availableLanguages.Clone();
                        Array.Sort(sortedLanguages);

                        // for each language
                        foreach (OcrLanguage language in sortedLanguages)
                        {
                            CheckBox checkBox = new CheckBox();
                            checkBox.Content = language;
                            checkBox.IsChecked = false;
                            checkBox.Checked += new RoutedEventHandler(checkBox_CheckChanged);
                            checkBox.Unchecked += new RoutedEventHandler(checkBox_CheckChanged);

                            languagesCheckedListBox.Items.Add(checkBox);
                        }
                    }

                    // end update the control
                    languagesCheckedListBox.EndInit();

                    // update selected languages
                    UpdateSelectedLanguages();
                }
            }
        }

        OcrLanguage[] _selectedLanguages = null;
        /// <summary>
        /// Gets or sets the selected languages.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <b>value</b> contains the language, which is not supported.
        /// </exception>
        [System.ComponentModel.Browsable(false)]
        public OcrLanguage[] SelectedLanguages
        {
            get
            {
                return _selectedLanguages;
            }
            set
            {
                // if selected languages are changed
                if (_selectedLanguages != value)
                {
                    if (value != null && value.Length > 0)
                    {
                        // for each language
                        foreach (OcrLanguage language in value)
                        {
                            // if language is not found
                            if (Array.IndexOf(AvailableLanguages, language) == -1)
                                throw new InvalidOperationException();
                        }
                    }

                    // update the selected languages
                    _selectedLanguages = value;
                    UpdateSelectedLanguages();
                }

            }
        }
#endif

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Raises the <see cref="E:SelectedLanguagesChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSelectedLanguagesChanged(EventArgs e)
        {
            if (SelectedLanguagesChanged != null)
                SelectedLanguagesChanged(this, e);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Updates the selected languages.
        /// </summary>
        private void UpdateSelectedLanguages()
        {
#if !REMOVE_OCR_PLUGIN
            // if there are no available languages
            if (AvailableLanguages == null ||
                AvailableLanguages.Length == 0)
                return;

            // specify that the updating of selected languages is started
            _isSelectedLanguagesChanging = true;

            // begin update the control
            languagesCheckedListBox.BeginInit();

            // if there are no selected languages
            if (SelectedLanguages == null || SelectedLanguages.Length == 0)
            {
                // uncheck all language items

                for (int index = 0; index < languagesCheckedListBox.Items.Count; index++)
                    SetItemChecked(index, false);
            }
            // if there are selected languages
            else
            {
                // for each available language
                foreach (OcrLanguage language in AvailableLanguages)
                {
                    // indicates whether language item is checked
                    bool isChecked = true;

                    // if language item must be unchecked
                    if (Array.IndexOf(SelectedLanguages, language) == -1)
                        isChecked = false;

                    // get language item index
                    int index = GetItemIndex(language);

                    // update check state of language item
                    SetItemChecked(index, isChecked);
                }
            }

            // end update the control
            languagesCheckedListBox.EndInit();

            // end update selected languages
            _isSelectedLanguagesChanging = false;
#endif
        }

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Returns the item index of specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>
        /// The item index in list box.
        /// </returns>
        private int GetItemIndex(OcrLanguage language)
        {
            for (int i = 0; i < languagesCheckedListBox.Items.Count; i++)
            {
                CheckBox checkBox = (CheckBox)languagesCheckedListBox.Items[i];

                if ((OcrLanguage)checkBox.Content == language)
                    return i;
            }

            return -1;
        }
#endif

        /// <summary>
        /// Updates the check state of the specified item.
        /// </summary>
        /// <param name="index">The item index.</param>
        /// <param name="value">Indicates that item must be checked.</param>
        private void SetItemChecked(int index, bool value)
        {
            CheckBox checkBox = (CheckBox)languagesCheckedListBox.Items[index];

            checkBox.IsChecked = value;
        }

        /// <summary>
        /// Returns a value that indicates whether the item is checked.
        /// </summary>
        /// <param name="index">The item index.</param>
        /// <returns>
        /// <b>True</b> if item is checked; otherwise, <b>false</b>.
        /// </returns>
        private bool GetItemChecked(int index)
        {
            CheckBox checkBox = (CheckBox)languagesCheckedListBox.Items[index];

            return checkBox.IsChecked.Value == true;
        }

        /// <summary>
        /// Updates the selected languages.
        /// </summary>
        private void checkBox_CheckChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            // if selected languages are changing
            if (_isSelectedLanguagesChanging)
                return;

            // the selected languages list
            List<OcrLanguage> selectedLanguages = new List<OcrLanguage>();

            // for each available language
            foreach (OcrLanguage language in AvailableLanguages)
            {
                // get language item index
                int index = GetItemIndex(language);

                // if current item is checked
                if (GetItemChecked(index))
                {
                    selectedLanguages.Add(language);
                }
            }

            // update selected languages array
            _selectedLanguages = selectedLanguages.ToArray();
#endif

            // generate event
            OnSelectedLanguagesChanged(EventArgs.Empty);
        }

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when the selected languages are changed.
        /// </summary>
        public event EventHandler SelectedLanguagesChanged;

        #endregion

    }

}

