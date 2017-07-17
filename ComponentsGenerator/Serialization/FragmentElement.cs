﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComponentsGenerator.Serialization
{
    public class FragmentElement
    {
        [XmlElement("Directory")]
        public DirectoryElement Directory { get; set; }

        [XmlElement("ComponentGroup")]
        public ComponentGroupElement ComponentGroup { get; set; }        

    }
}
