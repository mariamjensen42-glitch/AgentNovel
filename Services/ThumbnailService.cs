using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace AgentNovel.Services;

public class ThumbnailService
{
    private const int ThumbnailWidth = 150;
    private const int ThumbnailHeight = 200;

    public async Task<Bitmap?> GenerateThumbnailAsync(string pdfPath, int pageNumber)
    {
        try
        {
            // Open with PDFsharp to validate and get page dimensions
            using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
            if (pageNumber < 1 || pageNumber > document.PageCount)
                return null;

            var pdfPage = document.Pages[pageNumber - 1];

            // Calculate thumbnail dimensions maintaining aspect ratio
            double scaleX = ThumbnailWidth / pdfPage.Width;
            double scaleY = ThumbnailHeight / pdfPage.Height;
            double scale = Math.Min(scaleX, scaleY);

            int width = (int)(pdfPage.Width * scale);
            int height = (int)(pdfPage.Height * scale);

            // Use Windows.Data.Pdf (WinRT) to render the page to a stream.
            // This requires Windows 10 1809 (build 17763) or later.
            var file = await StorageFile.GetFileFromPathAsync(pdfPath);
            var pdfWinDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);
            var pdfWinPage = pdfWinDoc.GetPage((uint)(pageNumber - 1));

            var options = new PdfPageRenderOptions
            {
                DestinationWidth = (uint)Math.Max(1, width),
                DestinationHeight = (uint)Math.Max(1, height),
            };

            var stream = new InMemoryRandomAccessStream();
            await pdfWinPage.RenderToStreamAsync(stream, options);

            // Convert to Avalonia Bitmap
            stream.Seek(0);
            var managedStream = stream.AsStream();
            return new Bitmap(managedStream);
        }
        catch
        {
            return null;
        }
    }
}
