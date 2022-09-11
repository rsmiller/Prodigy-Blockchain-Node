using Prodigy.BusinessLayer;
using Prodigy.BusinessLayer.Models.Command;
using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Prodigy.DataUploader
{
    internal class Program
    {
        public static IDatabaseConnectionSettings _BlockchainDatabaseSettings;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Doing data");

            _BlockchainDatabaseSettings = new DatabaseConnectionSettings()
            {
                ConnectionString = "server=YOUR_DATABASE_SERVER;user=YOUR_USER;password=YOUR_PASSWORD;database=YOUR_DATABASE;convert zero datetime=True;TreatTinyAsBoolean=True;SslMode=none",
                DatabaseName = ""
            };


            var _ICertContext = new CertContext(_BlockchainDatabaseSettings);

            var total = _ICertContext.OnlineCerts.Count();

            var total_pages = Math.Ceiling((decimal)total / 500);

            for (var i = 0; i < total_pages; i++)
            {
                var skip = i * 500;
                var certs_list = _ICertContext.OnlineCerts.OrderByDescending(m => m.order_num).Take(100).ToList();

                Console.WriteLine("Got page " + i + " / " + total_pages);


                var httpClient = new HttpClient();

                foreach (var cert in certs_list)
                {

                    var pdf = httpClient.GetStreamAsync("THE_URL_TO_DOWNLOAD_YOUR_PDF_CERTIFCATE").Result;

                    var memory = new MemoryStream();
                    pdf.CopyTo(memory);

                    Random random = new Random();

                    var url = "http://555.555.555.555:8122/";
                    var jwt_auth_token = "4bb31eb6-d7a6-4327-a021-6c32055add07";

                    var _Client = new RestClient(url);
                    var request = new RestRequest("api/Node/CertCreated", Method.POST, DataFormat.Json);
                    request.AddHeader("Authorization", "JWT " + jwt_auth_token);
                    request.AddJsonBody(new DocumentCreateCommand()
                    { 
                        comapny_id = Guid.NewGuid(),
                        identifier1 = random.Next(110000, 119999).ToString(),
                        identifier2 = random.Next(290000, 299999).ToString(),
                        identifier3 = "document",
                        document_base64_data = System.Convert.ToBase64String(memory.ToArray()),
                    });

                    var response = _Client.Execute(request);
                }
            }
        }
    }
}
