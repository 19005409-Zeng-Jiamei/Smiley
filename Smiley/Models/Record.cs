using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smiley.Models
{
    public class Record
    {
        public int ID { get; set; }
        public string Tag { get; set; }
        public float Probability { get; set; }
        public string URL { get; set; }
        public DateTime DateTaken { get; set; }

    }
}
