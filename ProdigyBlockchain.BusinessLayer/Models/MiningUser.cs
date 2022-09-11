using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Prodigy.BusinessLayer.Models
{
	[Serializable]
	public class MiningUser
	{
		public string walled_id { get; set; }
		public string internal_ip_address { get; set; }
		public string external_ip_address { get; set; }
	}
}
