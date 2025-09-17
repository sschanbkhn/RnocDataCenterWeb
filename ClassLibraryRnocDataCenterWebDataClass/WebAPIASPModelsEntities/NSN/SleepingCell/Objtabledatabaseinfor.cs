using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
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
        [Column("ssl_mode")]
        [StringLength(20)]
        public string? SslMode { get; set; } = "Prefer"; // Default value

        [Column("trust_server_certificate")]
        public string? TrustServerCertificate { get; set; } = "true"; // Default value
        [Column("active")]
        public bool? Active { get; set; }

    }
}
