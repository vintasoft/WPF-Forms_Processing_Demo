using System;

using Vintasoft.Imaging.FormsProcessing.FormRecognition.Formatters;

namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// Custom serialization binder for correct deserialization of custom form field templates.
    /// </summary>
    public class CustomFormFieldTemplateSerializationBinder : FormFieldTemplateSerializationBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFormFieldTemplateSerializationBinder"/> class.
        /// </summary>
        public CustomFormFieldTemplateSerializationBinder()
            : base()
        {
        }

        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of
        /// the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        /// <returns>
        /// The type of the object the formatter creates a new instance of.
        /// </returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName == "FormsProcessingDemo")
                assemblyName = "WpfFormsProcessingDemo";
            if (typeName.StartsWith("FormsProcessingDemo"))
                typeName = "Wpf" + typeName;

            try
            {
                return base.BindToType(assemblyName, typeName);
            }
            catch
            {
                return null;
            }
        }
    }
}
