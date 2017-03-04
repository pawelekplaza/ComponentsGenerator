using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComponentsGenerator.Serialization
{
    public class Wix
    {
        [XmlElement("Fragment")]
        public FragmentElement Fragment { get; set; }
    }
}
