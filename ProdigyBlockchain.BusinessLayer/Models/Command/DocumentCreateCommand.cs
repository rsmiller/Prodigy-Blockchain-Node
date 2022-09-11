using Prodigy.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Command
{
    public class DocumentCreateCommand
    {
        public Guid comapny_id { get; set; }
        public string document_base64_data { get; set; }
        public string identifier1 { get; set; }
        public string identifier2 { get; set; }
        public string identifier3 { get; set; }
        public string identifier4 { get; set; }
        public string identifier5 { get; set; }
    }
}
