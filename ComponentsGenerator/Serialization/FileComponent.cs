﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComponentsGenerator.Serialization
{
    public class FileComponent
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Source { get; set; }

        [XmlAttribute]
        public string KeyPath { get; set; }        
    }
}
