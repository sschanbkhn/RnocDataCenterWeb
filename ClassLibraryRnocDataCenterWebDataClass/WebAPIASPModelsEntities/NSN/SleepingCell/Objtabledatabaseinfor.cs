using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell
{
    public class Objtabledatabaseinfor
    {
        public int Id { get; set; }
        public int SttDatabaseConfig { get; set; }
        public string ConnectionName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; } = 5432;
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SslMode { get; set; } = "Prefer";
        public bool TrustServerCertificate { get; set; } = true;
    }
}
