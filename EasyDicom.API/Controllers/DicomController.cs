using EasyDicom.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EasyDicom.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DicomController : ControllerBase
    {
        private readonly IDicomService _dicomService;
        public DicomController(IDicomService dicomService)
        {
            _dicomService = dicomService;
        }

        [HttpGet("structured-report")]
        public async Task<IActionResult> GetDicomStructuredReportAsync()
        {
            try
            {
                // Convert the DICOM SR to PDF
                byte[] pdfBytes = _dicomService.ConvertDicomSrToPdf();

                // Return the PDF as a download
                return File(pdfBytes, "application/pdf", "DicomReport.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error converting DICOM SR to PDF: {ex.Message}");
            }
        }
    }
}
