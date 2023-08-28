namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Specifies available modes of image binarization.
    /// </summary>
    public enum OcrBinarizationMode
    {

        /// <summary>
        /// No binarization.
        /// </summary>
        None,

        /// <summary>
        /// Binarize an image using Otsu algorithm.
        /// </summary>
        Global,

        /// <summary>
        /// Binarize an image using adaptive local-window algorithm.
        /// </summary>
        Adaptive,

    }
}
