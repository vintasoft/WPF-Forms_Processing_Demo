using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

#if !REMOVE_BARCODE_SDK
using Vintasoft.Barcode;
using Vintasoft.Barcode.SymbologySubsets;
#endif


namespace WpfFormsProcessingDemo
{
    /// <summary>
    /// A control that allows to view and edit the barcode reader settings.
    /// </summary>
    public partial class BarcodeReaderSettingsControl : UserControl
    {

        #region Constructors

        public BarcodeReaderSettingsControl()
        {
            InitializeComponent();

            directionAngle45.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionAngle45.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionTB.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionTB.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionBT.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionBT.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionRL.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionRL.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionLR.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionLR.Unchecked += new RoutedEventHandler(direction_CheckedChanged);


#if !REMOVE_BARCODE_SDK
            if (Vintasoft.Barcode.BarcodeGlobalSettings.IsDemoVersion)
            {
                barcodeGs1_128CheckBox.IsEnabled = false;
                barcodeGs1DataBarCheckBox.IsEnabled = false;
                barcodeGs1DataBarExpandedCheckBox.IsEnabled = false;
                barcodeGs1DataBarExpandedStackedCheckBox.IsEnabled = false;
                barcodeGs1DataBarLimitedCheckBox.IsEnabled = false;
                barcodeGs1DataBarStackedCheckBox.IsEnabled = false;
                barcodeGs1QRCheckbox.IsEnabled = false;
                barcodeGs1DataMatrixCheckBox.IsEnabled = false;
                barcodeGs1AztecCheckBox.IsEnabled = false;
                barcodeMailmarkCmdmType7CheckBox.IsEnabled = false;
                barcodeMailmarkCmdmType9CheckBox.IsEnabled = false;
                barcodeMailmarkCmdmType29CheckBox.IsEnabled = false;
                barcodeDeutschePostIdentcodeCheckBox.IsEnabled = false;
                barcodeDeutschePostLeitcodeCheckBox.IsEnabled = false;
                barcodeSwissPostParcelCheckBox.IsEnabled = false;
                barcodeFedExGround96CheckBox.IsEnabled = false;
                barcodeDhlAwbCheckBox.IsEnabled = false;
                barcodePpn.IsEnabled = false;
                barcodeOpcCheckBox.IsEnabled = false;
                barcodeItf14CheckBox.IsEnabled = false;
                barcodeVin.IsEnabled = false;
                barcodePzn.IsEnabled = false;
                barcodeSscc18CheckBox.IsEnabled = false;
                barcodeVicsBolCheckBox.IsEnabled = false;
                barcodeVicsScacProCheckBox.IsEnabled = false;
                barcodeXFAAztec.IsEnabled = false;
                barcodeXFADataMatrix.IsEnabled = false;
                barcodeXFAPDF417.IsEnabled = false;
                barcodeXFAQR.IsEnabled = false;
                barcodePatchCode.IsEnabled = false;
            } 
#endif
        }

        #endregion



        #region Properties

        public bool CanChangeExpectedBarcodes
        {
            get
            {
                return trackBarExpectedBarcodes.IsEnabled;
            }
            set
            {
                trackBarExpectedBarcodes.IsEnabled = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

#if !REMOVE_BARCODE_SDK
        public void SetReaderSettings(ReaderSettings readerSettings)
        {
            readerSettings.AutomaticRecognition = true;
            readerSettings.ScanInterval = (int)trackBarScanInterval.Value;
            readerSettings.ExpectedBarcodes = (int)trackBarExpectedBarcodes.Value;
            readerSettings.MinConfidence = 95;

            // set ScanDicrecion
            ScanDirection scanDirection = ScanDirection.None;
            if (directionLR.IsChecked.Value)
                scanDirection |= ScanDirection.LeftToRight;
            if (directionRL.IsChecked.Value)
                scanDirection |= ScanDirection.RightToLeft;
            if (directionTB.IsChecked.Value)
                scanDirection |= ScanDirection.TopToBottom;
            if (directionBT.IsChecked.Value)
                scanDirection |= ScanDirection.BottomToTop;
            if (directionAngle45.IsChecked.Value)
                scanDirection |= ScanDirection.Angle45and135;
            readerSettings.ScanDirection = scanDirection;

            // set ScanBarcodes
            BarcodeType scanBarcodeTypes = BarcodeType.None;
            if (barcodeCode11.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Code11;
            if (barcodeMSI.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.MSI;
            if (barcodeCode39.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Code39;
            if (barcodeCode93.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Code93;
            if (barcodeCode128.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Code128;
            if (barcodeCodabar.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Codabar;
            if (barcodeEAN.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.EAN8 | BarcodeType.EAN13;
            if (barcodeEANPlus.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Plus2 | BarcodeType.Plus5;
            if (barcodeI25.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Interleaved2of5;
            if (barcodeS25.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Standard2of5;
            if (barcodeUPCA.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.UPCA;
            if (barcodeUPCE.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.UPCE;
            if (barcodeTelepen.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Telepen;
            if (barcodePlanet.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Planet;
            if (barcodeIntelligentMail.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.IntelligentMail;
            if (barcodePostnet.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Postnet;
            if (barcodeRoyalMail.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.RoyalMail;
            if (barcodeDutchKIX.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.DutchKIX;
            if (barcodePatchCode.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.PatchCode;
            if (barcodePharmacode.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Pharmacode;
            if (barcodePDF417.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.PDF417 | BarcodeType.PDF417Compact;
            if (barcodeMicroPDF417.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.MicroPDF417;
            if (barcodeDataMatrix.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.DataMatrix;
            if (barcodeQR.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.QR;
            if (barcodeMicroQR.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.MicroQR;
            if (barcodeMaxiCode.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.MaxiCode;
            if (barcodeRSS14.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.RSS14;
            if (barcodeRSSLimited.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.RSSLimited;
            if (barcodeRSSExpanded.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.RSSExpanded;
            if (barcodeRSSExpandedStacked.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.RSSExpandedStacked;
            if (barcodeRSS14Stacked.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.RSS14Stacked;
            if (barcodeAztec.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Aztec;
            if (barcodeRSS14Stacked.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.RSS14Stacked;
            if (barcodeAustralian.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.AustralianPost;
            if (barcodeMailmark4CCheckBox.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Mailmark4StateC;
            if (barcodeMailmark4LCheckBox.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Mailmark4StateL;
            if (barcodeIata2of5.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.IATA2of5;
            if (barcodeMatrix2of5.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Matrix2of5;
            if (barcodeCode16K.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.Code16K;
            if (barcodeHanXinCodeCheckBox.IsChecked.Value)
                scanBarcodeTypes |= BarcodeType.HanXinCode;
            readerSettings.ScanBarcodeTypes = scanBarcodeTypes;

            List<BarcodeSymbologySubset> scanBarcodeSubsets = readerSettings.ScanBarcodeSubsets;
            scanBarcodeSubsets.Clear();
            if (barcodeCode39.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.Code39Extended);
            if (barcodeGs1_128CheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1_128);
            if (barcodeGs1DataBarCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBar);
            if (barcodeGs1DataBarExpandedCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarExpanded);
            if (barcodeGs1DataBarExpandedStackedCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarExpandedStacked);
            if (barcodeGs1DataBarLimitedCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarLimited);
            if (barcodeGs1DataBarStackedCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarStacked);
            if (barcodeGs1QRCheckbox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1QR);
            if (barcodeGs1DataMatrixCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataMatrix);
            if (barcodeGs1AztecCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1Aztec);
            if (barcodeMailmarkCmdmType7CheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.MailmarkCmdmType7);
            if (barcodeMailmarkCmdmType9CheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.MailmarkCmdmType9);
            if (barcodeMailmarkCmdmType29CheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.MailmarkCmdmType29);
            if (barcodeDeutschePostIdentcodeCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.DeutschePostIdentcode);
            if (barcodeDeutschePostLeitcodeCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.DeutschePostLeitcode);
            if (barcodeSwissPostParcelCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.SwissPostParcel);
            if (barcodeFedExGround96CheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.FedExGround96);
            if (barcodeDhlAwbCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.DhlAwb);
            if (barcodeCode32.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.Code32);
            if (barcodePpn.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.PPN);
            if (barcodeIsxn.IsChecked.Value)
            {
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBN);
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISMN);
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISSN);
                if (barcodeEANPlus.IsChecked.Value)
                {
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBNPlus2);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISMNPlus2);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISSNPlus2);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBNPlus5);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISMNPlus5);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISSNPlus5);
                }
            }
            if (barcodeJanCheckBox.IsChecked.Value)
            {
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN13);
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN8);
                if (barcodeEANPlus.IsChecked.Value)
                {
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN13Plus2);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN8Plus2);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN13Plus5);
                    scanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN8Plus5);
                }
            }
            if (barcodeOpcCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.OPC);
            if (barcodeItf14CheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.ITF14);
            if (barcodeVin.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.VIN);
            if (barcodePzn.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.PZN);
            if (barcodeSscc18CheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.SSCC18);
            if (barcodeVicsBolCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.VicsBol);
            if (barcodeVicsScacProCheckBox.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.VicsScacPro);
            if (barcodeXFAAztec.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedAztec);
            if (barcodeXFADataMatrix.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedDataMatrix);
            if (barcodeXFAPDF417.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedPDF417);
            if (barcodeXFAQR.IsChecked.Value)
                scanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedQRCode);
        }

        public void RestoreSettings(ReaderSettings readerSettings)
        {
            trackBarExpectedBarcodes.Value = readerSettings.ExpectedBarcodes;
            trackBarScanInterval.Value = readerSettings.ScanInterval;

            ScanDirection scanDirection = readerSettings.ScanDirection;
            directionLR.IsChecked = (scanDirection & ScanDirection.LeftToRight) != 0;
            directionRL.IsChecked = (scanDirection & ScanDirection.RightToLeft) != 0;
            directionTB.IsChecked = (scanDirection & ScanDirection.TopToBottom) != 0;
            directionBT.IsChecked = (scanDirection & ScanDirection.BottomToTop) != 0;
            directionAngle45.IsChecked = (scanDirection & ScanDirection.Angle45and135) != 0;

            // barcode types
            BarcodeType scanBarcodeTypes = readerSettings.ScanBarcodeTypes;
            barcodeCode11.IsChecked = (scanBarcodeTypes & BarcodeType.Code11) != 0;
            barcodeMSI.IsChecked = (scanBarcodeTypes & BarcodeType.MSI) != 0;
            barcodeCode39.IsChecked = (scanBarcodeTypes & BarcodeType.Code39) != 0;
            barcodeCode93.IsChecked = (scanBarcodeTypes & BarcodeType.Code93) != 0;
            barcodeCode128.IsChecked = (scanBarcodeTypes & BarcodeType.Code128) != 0;
            barcodeCodabar.IsChecked = (scanBarcodeTypes & BarcodeType.Codabar) != 0;
            barcodeEAN.IsChecked = (scanBarcodeTypes & BarcodeType.EAN13) != 0 || (scanBarcodeTypes & BarcodeType.EAN8) != 0;
            barcodeEANPlus.IsChecked = (scanBarcodeTypes & BarcodeType.Plus5) != 0 || (scanBarcodeTypes & BarcodeType.Plus2) != 0;
            barcodeI25.IsChecked = (scanBarcodeTypes & BarcodeType.Interleaved2of5) != 0;
            barcodeS25.IsChecked = (scanBarcodeTypes & BarcodeType.Standard2of5) != 0;
            barcodeUPCA.IsChecked = (scanBarcodeTypes & BarcodeType.UPCA) != 0;
            barcodeUPCE.IsChecked = (scanBarcodeTypes & BarcodeType.UPCE) != 0;
            barcodeAustralian.IsChecked = (scanBarcodeTypes & BarcodeType.AustralianPost) != 0;
            barcodeTelepen.IsChecked = (scanBarcodeTypes & BarcodeType.Telepen) != 0;
            barcodePlanet.IsChecked = (scanBarcodeTypes & BarcodeType.Planet) != 0;
            barcodeIntelligentMail.IsChecked = (scanBarcodeTypes & BarcodeType.IntelligentMail) != 0;
            barcodePostnet.IsChecked = (scanBarcodeTypes & BarcodeType.Postnet) != 0;
            barcodeRoyalMail.IsChecked = (scanBarcodeTypes & BarcodeType.RoyalMail) != 0;
            barcodeDutchKIX.IsChecked = (scanBarcodeTypes & BarcodeType.DutchKIX) != 0;
            barcodePatchCode.IsChecked = (scanBarcodeTypes & BarcodeType.PatchCode) != 0;
            barcodePDF417.IsChecked = ((scanBarcodeTypes & BarcodeType.PDF417) != 0) || ((scanBarcodeTypes & BarcodeType.PDF417Compact) != 0);
            barcodeMicroPDF417.IsChecked = (scanBarcodeTypes & BarcodeType.MicroPDF417) != 0;
            barcodeDataMatrix.IsChecked = (scanBarcodeTypes & BarcodeType.DataMatrix) != 0;
            barcodeQR.IsChecked = (scanBarcodeTypes & BarcodeType.QR) != 0;
            barcodeMicroQR.IsChecked = (scanBarcodeTypes & BarcodeType.MicroQR) != 0;
            barcodeMaxiCode.IsChecked = (scanBarcodeTypes & BarcodeType.MaxiCode) != 0;
            barcodeRSS14.IsChecked = (scanBarcodeTypes & BarcodeType.RSS14) != 0;
            barcodeRSSLimited.IsChecked = (scanBarcodeTypes & BarcodeType.RSSLimited) != 0;
            barcodeRSSExpanded.IsChecked = (scanBarcodeTypes & BarcodeType.RSSExpanded) != 0;
            barcodeRSS14Stacked.IsChecked = (scanBarcodeTypes & BarcodeType.RSS14Stacked) != 0;
            barcodeRSSExpandedStacked.IsChecked = (scanBarcodeTypes & BarcodeType.RSSExpandedStacked) != 0;
            barcodeAztec.IsChecked = (scanBarcodeTypes & BarcodeType.Aztec) != 0;
            barcodePharmacode.IsChecked = (scanBarcodeTypes & BarcodeType.Pharmacode) != 0;
            barcodeMailmark4CCheckBox.IsChecked = (scanBarcodeTypes & BarcodeType.Mailmark4StateC) != 0;
            barcodeMailmark4LCheckBox.IsChecked = (scanBarcodeTypes & BarcodeType.Mailmark4StateL) != 0;
            barcodeIata2of5.IsChecked = (scanBarcodeTypes & BarcodeType.IATA2of5) != 0;
            barcodeMatrix2of5.IsChecked = (scanBarcodeTypes & BarcodeType.Matrix2of5) != 0;
            barcodeCode16K.IsChecked = (scanBarcodeTypes & BarcodeType.Code16K) != 0;
            barcodeHanXinCodeCheckBox.IsChecked = (scanBarcodeTypes & BarcodeType.HanXinCode) != 0;

            // barcode subsets
            barcodeGs1_128CheckBox.IsChecked = false;
            barcodeGs1DataBarCheckBox.IsChecked = false;
            barcodeGs1DataBarExpandedCheckBox.IsChecked = false;
            barcodeGs1DataBarExpandedStackedCheckBox.IsChecked = false;
            barcodeGs1DataBarLimitedCheckBox.IsChecked = false;
            barcodeGs1DataBarStackedCheckBox.IsChecked = false;
            barcodeGs1QRCheckbox.IsChecked = false;
            barcodeGs1DataMatrixCheckBox.IsChecked = false;
            barcodeGs1AztecCheckBox.IsChecked = false;
            barcodeMailmarkCmdmType7CheckBox.IsChecked = false;
            barcodeMailmarkCmdmType9CheckBox.IsChecked = false;
            barcodeMailmarkCmdmType29CheckBox.IsChecked = false;
            barcodeDeutschePostIdentcodeCheckBox.IsChecked = false;
            barcodeDeutschePostLeitcodeCheckBox.IsChecked = false;
            barcodeSwissPostParcelCheckBox.IsChecked = false;
            barcodeFedExGround96CheckBox.IsChecked = false;
            barcodeDhlAwbCheckBox.IsChecked = false;
            barcodeCode32.IsChecked = false;
            barcodePpn.IsChecked = false;
            barcodeJanCheckBox.IsChecked = false;
            barcodeOpcCheckBox.IsChecked = false;
            barcodeItf14CheckBox.IsChecked = false;
            barcodeVin.IsChecked = false;
            barcodePzn.IsChecked = false;
            barcodeSscc18CheckBox.IsChecked = false;
            barcodeVicsBolCheckBox.IsChecked = false;
            barcodeVicsScacProCheckBox.IsChecked = false;

            List<BarcodeSymbologySubset> scanBarcodeSubsets = readerSettings.ScanBarcodeSubsets;
            foreach (BarcodeSymbologySubset subset in scanBarcodeSubsets)
            {
                if (subset is GS1_128BarcodeSymbology)
                    barcodeGs1_128CheckBox.IsChecked = true;
                if (subset is GS1DataBarBarcodeSymbology)
                    barcodeGs1DataBarCheckBox.IsChecked = true;
                if (subset is GS1DataBarExpandedBarcodeSymbology)
                    barcodeGs1DataBarExpandedCheckBox.IsChecked = true;
                if (subset is GS1DataBarExpandedStackedBarcodeSymbology)
                    barcodeGs1DataBarExpandedStackedCheckBox.IsChecked = true;
                if (subset is GS1DataBarLimitedBarcodeSymbology)
                    barcodeGs1DataBarLimitedCheckBox.IsChecked = true;
                if (subset is GS1DataBarStackedBarcodeSymbology)
                    barcodeGs1DataBarStackedCheckBox.IsChecked = true;
                if (subset is GS1QRBarcodeSymbology)
                    barcodeGs1QRCheckbox.IsChecked = true;
                if (subset is GS1DataMatrixBarcodeSymbology)
                    barcodeGs1DataMatrixCheckBox.IsChecked = true;
                if (subset is GS1AztecBarcodeSymbology)
                    barcodeGs1AztecCheckBox.IsChecked = true;
                if (subset is MailmarkCmdmType7BarcodeSymbology)
                    barcodeMailmarkCmdmType7CheckBox.IsChecked = true;
                if (subset is MailmarkCmdmType9BarcodeSymbology)
                    barcodeMailmarkCmdmType9CheckBox.IsChecked = true;
                if (subset is MailmarkCmdmType29BarcodeSymbology)
                    barcodeMailmarkCmdmType29CheckBox.IsChecked = true;
                if (subset is DeutschePostIdentcodeBarcodeSymbology)
                    barcodeDeutschePostIdentcodeCheckBox.IsChecked = true;
                if (subset is DeutschePostLeitcodeBarcodeSymbology)
                    barcodeDeutschePostLeitcodeCheckBox.IsChecked = true;
                if (subset is SwissPostParcelBarcodeSymbology)
                    barcodeSwissPostParcelCheckBox.IsChecked = true;
                if (subset is FedExGround96BarcodeSymbology)
                    barcodeFedExGround96CheckBox.IsChecked = true;
                if (subset is DhlAwbBarcodeSymbology)
                    barcodeDhlAwbCheckBox.IsChecked = true;
                if (subset is Code32BarcodeSymbology)
                    barcodeCode32.IsChecked = true;
                if (subset is PpnBarcodeSymbology)
                    barcodePpn.IsChecked = true;
                if (subset is JanBarcodeSymbology)
                    barcodeJanCheckBox.IsChecked = true;
                if (subset is IsbnBarcodeSymbology ||
                    subset is IsmnBarcodeSymbology ||
                    subset is IssnBarcodeSymbology)
                    barcodeIsxn.IsChecked = true;
                if (subset is OpcBarcodeSymbology)
                    barcodeOpcCheckBox.IsChecked = true;
                if (subset is Itf14BarcodeSymbology)
                    barcodeItf14CheckBox.IsChecked = true;
                if (subset is VinSymbology)
                    barcodeVin.IsChecked = true;
                if (subset is PznBarcodeSymbology)
                    barcodePzn.IsChecked = true;
                if (subset is Sscc18BarcodeSymbology)
                    barcodeSscc18CheckBox.IsChecked = true;
                if (subset is VicsBolBarcodeSymbology)
                    barcodeVicsBolCheckBox.IsChecked = true;
                if (subset is VicsScacProBarcodeSymbology)
                    barcodeVicsScacProCheckBox.IsChecked = true;
                if (subset is XFACompressedAztecBarcodeSymbology)
                    barcodeXFAAztec.IsChecked = true;
                if (subset is XFACompressedDataMatrixBarcodeSymbology)
                    barcodeXFADataMatrix.IsChecked = true;
                if (subset is XFACompressedPDF417BarcodeSymbology)
                    barcodeXFAPDF417.IsChecked = true;
                if (subset is XFACompressedQRCodeBarcodeSymbology)
                    barcodeXFAQR.IsChecked = true;
            }
        } 
#endif

        #endregion


        #region PRIVATE

        /// <summary>
        /// Handles the Click event of barcodeTypesAllOrClear object.
        /// </summary>
        private void barcodeTypesAllOrClear_Click(object sender, RoutedEventArgs e)
        {
            Grid grid = (Grid)(((Button)sender).Parent);
            bool isChecked = false;

            foreach (UIElement element in grid.Children)
            {
                if (element is Panel)
                    isChecked |= IsCheckedCheckBox((Panel)element);
                else if (element is ContentControl &&
                    ((ContentControl)element).Content is Panel)
                {
                    Panel panel = (Panel)((ContentControl)element).Content;
                    foreach (UIElement panelElement in panel.Children)
                    {
                        if (panelElement.IsEnabled)
                        {
                            isChecked |= IsCheckedCheckBox(panel);
                            break;
                        }
                    }
                }
            }

            foreach (UIElement element in grid.Children)
            {
                CheckCheckBox(element, !isChecked);
            }
        }

        private void CheckCheckBox(UIElement element, bool isChecked)
        {
            if (element is Panel)
            {
                Panel panel = element as Panel;
                foreach (FrameworkElement control in panel.Children)
                {
                    if (control is Panel)
                        CheckCheckBox((Panel)control, isChecked);
                    else if (control is CheckBox)
                    {
                        CheckBox checkBox = (CheckBox)control;
                        if (checkBox.IsEnabled)
                            checkBox.IsChecked = isChecked;
                    }
                }
            }
            else if (element is ContentControl)
            {
                ContentControl contentControl = (ContentControl)element;
                if (contentControl.Content is UIElement)
                    CheckCheckBox((UIElement)contentControl.Content, isChecked);
            }
        }

        private bool IsCheckedCheckBox(Panel panel)
        {
            foreach (FrameworkElement control in panel.Children)
            {
                if (control is Panel && !IsCheckedCheckBox((Panel)control))
                    return false;
                else if (control is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)control;
                    if (checkBox.IsEnabled && !checkBox.IsChecked.Value)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the ValueChanged event of trackBarExpectedBarcodes object.
        /// </summary>
        private void trackBarExpectedBarcodes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelExpectedBarcodes != null)
                labelExpectedBarcodes.Content = trackBarExpectedBarcodes.Value;
        }

        /// <summary>
        /// Handles the ValueChanged event of trackBarScanInterval object.
        /// </summary>
        private void trackBarScanInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelScanInterval != null)
                labelScanInterval.Content = trackBarScanInterval.Value;
        }

        /// <summary>
        /// Update visualization of scan direction 45/135.
        /// </summary>
        private void direction_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Visibility visibility = Visibility.Visible;
            if (!directionAngle45.IsChecked.Value)
                visibility = Visibility.Hidden;
            directionLB_RT.Visibility = directionLT_RB.Visibility = directionRB_LT.Visibility = directionRT_LB.Visibility = visibility;
            if (directionAngle45.IsChecked.Value)
            {
                directionLT_RB.IsChecked = directionLR.IsChecked.Value || directionTB.IsChecked.Value;
                directionRT_LB.IsChecked = directionTB.IsChecked.Value || directionRL.IsChecked.Value;
                directionLB_RT.IsChecked = directionLR.IsChecked.Value || directionBT.IsChecked.Value;
                directionRB_LT.IsChecked = directionBT.IsChecked.Value || directionRL.IsChecked.Value;
            }
        }

        #endregion

        #endregion

    }
}
