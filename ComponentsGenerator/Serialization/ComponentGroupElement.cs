using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComponentsGenerator.Serialization
{    
    public class ComponentGroupElement
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlElement("Component")]
        public List<ComponentElement> Components { get; set; }
    }
}
