﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Extend;
using Piranha.Extend.Fields;

namespace RentVision.Models
{
    [BlockType(Name = "Block gray", Category = "RentVision",
        Icon = "fas fa-columns", Component = "two-column-block-gray")]
    public class TwoColumnBlockGray : Block
    {
        public TextField Title { get; set; }

        public TextField Subtitle { get; set; }

        public HtmlField Column1 { get; set; }

        public HtmlField Column2 { get; set; }
    }
}
