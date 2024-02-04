using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigileye.Models
{
    public class ShareScreen
    {
        public int Id { get; set; }
        [ForeignKey("Computer")]
        public int ComputerID { get; set; }
        public Computer Computer { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
