using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.IO;

namespace ExamScheduler.Data
{
    public class VeriTabaniBaglanti
    {
        private readonly string _connectionString;

        public VeriTabaniBaglanti()
        {
            // appsettings.json dosyasından bağlantı bilgisini okur
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = config.GetConnectionString("SinavPlanlayiciDB");
        }

        /// <summary>
        /// SQL bağlantısını açar ve SqlConnection döner.
        /// </summary>
        public SqlConnection BaglantiAc()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
