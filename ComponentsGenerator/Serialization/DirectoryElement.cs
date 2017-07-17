using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComponentsGenerator.Serialization
{
    public class DirectoryElement
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
        
        [XmlElement("Directory")]
        public List<DirectoryElement> Directories { get; set; }
    }
}
