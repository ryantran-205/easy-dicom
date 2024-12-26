using EasyDicom.API.Services.Interfaces;
using EasyDicom.API.Settings;
using FellowOakDicom;
using Microsoft.Extensions.Options;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace EasyDicom.API.Services
{
    public class DicomService : IDicomService
    {
        private readonly IOptionsMonitor<DicomSettings> _dicomSettings;

        public DicomService(IOptionsMonitor<DicomSettings> dicomSettings)
        {
            _dicomSettings = dicomSettings;
        }

        public byte[] ConvertDicomSrToPdf()
        {
            // Read the DICOM file path from the configuration
            string dicomFilePath = _dicomSettings.CurrentValue.DicomFilePath;

            if (string.IsNullOrEmpty(dicomFilePath) || !File.Exists(dicomFilePath))
            {
                throw new InvalidOperationException("Invalid DICOM file path.");
            }

            // Open the DICOM SR file
            DicomFile dicomFile = DicomFile.Open(dicomFilePath);
            DicomDataset dicomDataset = dicomFile.Dataset;

            // Access the ContentSequence (0x0040,0xA730) which holds the observation content
            var contentSequence = dicomDataset.GetSequence(DicomTag.ContentSequence);

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 12);
            gfx.DrawString("DICOM Structured Report", font, XBrushes.Black, new XPoint(20, 20));

            // Start positioning the content in the PDF
            int yPosition = 40;

            // Loop through the ContentSequence and process each item
            foreach (var item in contentSequence)
            {
                // Example 1: Extracting text content from an observation
                if (item.TryGetSingleValue(DicomTag.TextValue, out string textValue))
                {
                    gfx.DrawString(textValue, font, XBrushes.Black, new XPoint(20, yPosition));
                    yPosition += 20;
                }

                // Example 2: Handling embedded images (like thumbnails or small images)
                if (item.TryGetSequence(DicomTag.GraphicObjectSequence, out DicomSequence graphicObjectSequence))
                {
                    foreach (var graphicItem in graphicObjectSequence.Items)
                    {
                        if (graphicItem.TryGetValues(DicomTag.GraphicData, out byte[] graphicData))
                        {
                            using (var stream = new MemoryStream(graphicData))
                            {
                                XImage image = XImage.FromStream(() => stream);
                                gfx.DrawImage(image, 20, yPosition, 100, 100); // Draw the image
                                yPosition += 120;
                            }
                        }
                    }
                }

                // Example 3: Handling image references (e.g., from another DICOM object)
                if (item.TryGetSequence(DicomTag.ReferencedImageSequence, out DicomSequence referencedImageSequence))
                {
                    foreach (var referencedItem in referencedImageSequence.Items)
                    {
                        string referencedFilePath = referencedItem.GetSingleValueOrDefault(DicomTag.ReferencedFileID, string.Empty);
                        if (File.Exists(referencedFilePath))
                        {
                            DicomFile imageFile = DicomFile.Open(referencedFilePath);
                            DicomDataset imageDataset = imageFile.Dataset;
                            if (imageDataset.TryGetValues(DicomTag.PixelData, out byte[] pixelData))
                            {
                                using (var stream = new MemoryStream(pixelData))
                                {
                                    XImage image = XImage.FromStream(() => stream);
                                    gfx.DrawImage(image, 20, yPosition, 200, 200); // Draw the referenced image
                                    yPosition += 220;
                                }
                            }
                        }
                    }
                }
            }

            // Save the PDF to a byte array and return it
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                return ms.ToArray();
            }
        }

    }
}
