using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ImageUpload.Models
{
    public class Image
    {
        public Image() {

            Images = new List<Image>();
        }
        public int ImageID { get; set; }

        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Upload File")]
        public string? FileUpload { get; set; }
        public string? FileType { get; set; }
        public string? ImageData { get; set; }
        public string[]? FileUploadArray { get; set; }

        public byte[] FileData { get; set; }
        public byte[]? Base64 { get; set; }

        public  List<Image> Images { get; set; }
    }

    public class JsonResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

    }
}
