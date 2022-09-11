using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer
{
    public interface INodeConfig
    {
        int network_id { get; set; }
        string node_id { get; set; }
        int load_from { get; set; }
        string db_directory { get; set; }
        bool encrypt_files { get; set; }
        int average_certs_per_year { get; set; }
        int max_coins_per_year { get; set; }
        string document_db_type { get; set; }
        string s3_service_url { get; set; }
        string s3_service_access_key{ get; set; }
        string s3_service_secret { get; set; }
        string s3_service_bucket { get; set; }
        string log_level { get; set; }
        string log_type { get; set; }
        string ticker { get; set; }
        string ip_seeds { get; set; }
    }

    public class NodeConfig : INodeConfig
    {
        public int network_id { get; set; } = 0;
        public string node_id { get; set; }
        public int load_from { get; set; }
        public string db_directory { get; set; }
        public bool encrypt_files { get; set; } = false;
        public int average_certs_per_year { get; set; } = 20000;
        public int max_coins_per_year { get; set; } = 3;
        public string document_db_type { get; set; } = "file";
        public string s3_service_url { get; set; }
        public string s3_service_access_key { get; set; }
        public string s3_service_secret { get; set; }
        public string s3_service_bucket { get; set; }
        public string log_level { get; set; }
        public string log_type { get; set; }
        public string ticker { get; set; }
        public string ip_seeds { get; set; }
    }
}
