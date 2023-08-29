# VintaSoft WPF Forms Processing Demo

This C# project uses <a href="https://www.vintasoft.com/vsimaging-dotnet-index.html">VintaSoft Imaging .NET SDK</a> and demonstrates how to recognize the form documents:
* Create and edit a form template using the mouse.
* Save a form template to a file, load a form template from a file.
* Recognize form using form template.
* Recognize form in multiple threads.
* Preview the results of form alignment and form recognition.


## Screenshot
<img src="vintasoft-wpf-forms-processing-demo.png" title="VintaSoft WPF Forms Processing Demo">


## Usage
1. Get the 30 day free evaluation license for <a href="https://www.vintasoft.com/vsimaging-dotnet-index.html" target="_blank">VintaSoft Imaging .NET SDK</a> as described here: <a href="https://www.vintasoft.com/docs/vsimaging-dotnet/Licensing-Evaluation.html" target="_blank">https://www.vintasoft.com/docs/vsimaging-dotnet/Licensing-Evaluation.html</a>

2. Update the evaluation license in "CSharp\MainForm.cs" file:
   ```
   Vintasoft.Imaging.ImagingGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");
   ```

3. Build the project ("WpfFormsProcessingDemo.Net7.csproj" file) in Visual Studio or using .NET CLI:
   ```
   dotnet build WpfFormsProcessingDemo.Net7.csproj
   ```

4. Run compiled application and try to how to recognize the form documents.


## Documentation
VintaSoft Imaging .NET SDK on-line User Guide and API Reference for .NET developer is available here: https://www.vintasoft.com/docs/vsimaging-dotnet/


## Support
Please visit our <a href="https://myaccount.vintasoft.com/">online support center</a> if you have any question or problem.
