using iTextSharp.text.pdf.parser;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace trnservice
{
    class Program
    {
        static void Main(string[] args)
        {

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void Temp()
        {
            string[] ltrn;
            //Console.WriteLine("Hello World!");

            ServiceReference1.MLSSServicesTRNClient obj = new ServiceReference1.MLSSServicesTRNClient();

            StringBuilder sb = new StringBuilder();

            foreach (string line in File.ReadLines(@"C:\Users\robinsonod\Downloads\Reports\WestmorelandWestern17-05-2023remainder.csv"))
            {

                ltrn = line.Split(',');
                string v = ltrn[1].ToString();
                // Converted to char away to check if all characters are numeric
                v.ToCharArray();


                if (ltrn[1] == "" || (!v.All(char.IsDigit)))
                {
                    // If trn is empty or not all numeric, assign default value of "111111111"
                    ltrn[1] = "111111111";
                }

                var objtrn = obj.GetIndividualTrn(int.Parse(ltrn[1]));
                if (objtrn != null && objtrn.IndividualInfo != null)
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", line, objtrn.IndividualInfo.FirstName, objtrn.IndividualInfo.MiddleName, objtrn.IndividualInfo.LastName, objtrn.IndividualInfo.BirthDate.Value.ToShortDateString(), objtrn.IndividualInfo.GenderType, objtrn.IndividualInfo.NbrTrn));
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            File.AppendAllText(@"C:\Users\robinsonod\Downloads\Reports\WestmorelandWestern 17-05-2023 remainder Results2.csv", sb.ToString());

        }
    }
}



