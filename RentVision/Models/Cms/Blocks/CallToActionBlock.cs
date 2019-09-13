using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Extend;
using Piranha.Extend.Fields;

namespace RentVision.Models
{
    [BlockType(Name = "Call-to-action block", Category = "RentVision",
        Icon = "fas fa-columns", Component = "call-to-action-block")]
    public class CallToActionBlock : Block
    {
        public TextField Title { get; set; }

        public HtmlField Column { get; set; }
    }
}
