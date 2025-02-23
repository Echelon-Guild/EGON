using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGON.Library.Models
{
    public class WoWInstanceInfo
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool Legacy { get; set; }
        public InstanceType InstanceType { get; set; }
    }
}
