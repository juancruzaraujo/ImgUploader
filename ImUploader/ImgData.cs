using System.Text.Json.Serialization;

namespace ImgUploader
{
    public class ImgData
    {
        public string UserId { get; set; }

        public IFormFile Image { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? ImagePath { get; set; }
    }
}
