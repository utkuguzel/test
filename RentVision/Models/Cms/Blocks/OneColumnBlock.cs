using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Extend;
using Piranha.Extend.Fields;

namespace RentVision.Models
{
    [BlockType(Name = "One-column block white", Category = "RentVision",
        Icon = "fas fa-columns", Component = "one-column-block")]
    public class OneColumnBlock : Block
    {
        public TextField Title { get; set; }

        public HtmlField Column { get; set; }
    }
}
