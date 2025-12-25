using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SynkTask.Models.Models
{
    public class TaskAttachment
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }

        [ForeignKey(nameof(TaskId))]
        public ProjectTask Task { get; set; }

        public string FileName { get; set; }
        public string FileUrl { get; set; }

        public string ResourceType { get; set; } // image | raw
        public string PublicId { get; set; }

        public long FileSize { get; set; }
        public string ContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
