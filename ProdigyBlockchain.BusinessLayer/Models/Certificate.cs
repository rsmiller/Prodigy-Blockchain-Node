using System;
using System.Collections.Generic;
using System.Text;

namespace Prodigy.BusinessLayer.Models
{
    public class Certificate
    {
        public int mysqlid { get; set; }
        public long id { get; set; }
        public string cert_title { get; set; }
        public string cert_header { get; set; }
        public string cert_body { get; set; }
        public string cert_footer { get; set; }
        public string order_num { get; set; }
        public int assy_id { get; set; }
        public int signature_1 { get; set; }
        public int signature_2 { get; set; }
        public int cert_type { get; set; }
        public bool valid { get; set; }
        public int cust_id { get; set; }
        public string po { get; set; }
        public string cust_part { get; set; }
        public string description { get; set; }
        public string serial_num { get; set; }
        public int dnvyear { get; set; }
        public string dnv_cert_id { get; set; }
        public DateTime creation_date { get; set; }
        public int mod_by { get; set; }
        public string locations { get; set; }
        public bool isdeleted { get; set; }
        public bool sync { get; set; }

    }
}
