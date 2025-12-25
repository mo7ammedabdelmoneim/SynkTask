using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class FileUploadResult
    {
        public string Url { get; set; }
        public string PublicId { get; set; }
        public string ResourceType { get; set; }
    }

}
