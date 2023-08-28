using Vintasoft.Imaging.FormsProcessing.FormRecognition.Wpf.UI;

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// Determines how to display a barcode field template
    /// and how user can interact with it.
    /// </summary>
    class WpfBarcodeFieldTemplateView: WpfFormFieldTemplateView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WpfBarcodeFieldTemplateView"/> class.
        /// </summary>
        /// <param name="fieldTemplate">Barcode field template.</param>
        public WpfBarcodeFieldTemplateView(BarcodeFieldTemplate fieldTemplate)
            : base(fieldTemplate)
        {
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new WpfBarcodeFieldTemplateView((BarcodeFieldTemplate)FieldTemplate.Clone());
        }
    }
}
