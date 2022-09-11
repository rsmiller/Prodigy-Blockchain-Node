using System;
using System.Collections.Generic;
using System.Text;

namespace Prodigy.BusinessLayer
{
    public interface IDatabaseConnectionSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }

    public class DatabaseConnectionSettings : IDatabaseConnectionSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
