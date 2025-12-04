using iText.Html2pdf;
using iText.StyledXmlParser.Css.Media;
using System.IO;

namespace SingleOne.Util
{
    public class HtmlToPdfConverter
    {
        public static byte[] ConvertHtmlToPdf(string htmlContent)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ConverterProperties converterProperties = new ConverterProperties();
                converterProperties.SetMediaDeviceDescription(new MediaDeviceDescription(MediaType.PRINT));
                //converterProperties.SetBaseUri("path/to/base/uri"); // Defina isso se seu HTML referenciar recursos externos

                // Converter HTML para PDF e escrever o resultado no MemoryStream
                HtmlConverter.ConvertToPdf(htmlContent, stream, converterProperties);

                // Retorna o array de bytes do MemoryStream que contém o PDF
                return stream.ToArray();
            }
        }
    }
}
