using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WSFB.Controllers
{
    public class Conexion
    {
        public string connectionString =
               "User=SYSDBA;" +
               "Password=masterkey;" +
               "Database=C:/Program Files (x86)/AbarrotesMultiCaja/db/PDVDATA.FDB;" +
               "DataSource=localhost;" +
               "Port=3050;" +
               "Dialect=3;" +
               "Charset=NONE;" +
               "Role=;" +
               "Connection lifetime=15;" +
               "Pooling=true;" +
               "MinPoolSize=0;" +
               "MaxPoolSize=50;" +
               "Packet Size=8192;" +
               "ServerType=0";

        public string ConnectionStringMysql = "server=localhost;user=root;database=sad;port=3306;password=";
    }
}