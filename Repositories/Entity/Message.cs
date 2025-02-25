using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Entity
{
    public class Message
    {
        public int Id { get; set; }
        public int FromId { get; set; }
        [ForeignKey("FromId")]
        public virtual User? From { get; set; }

        public int ToId { get; set; }
        [ForeignKey("ToId")]
        public virtual User? To { get; set; }
        public string Text { get; set; }
        public bool Readed { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;

    }
}
