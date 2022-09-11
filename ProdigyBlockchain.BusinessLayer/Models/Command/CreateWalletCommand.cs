using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Command
{
    public class CreateWalletCommand
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
