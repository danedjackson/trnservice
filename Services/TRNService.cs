using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using trnservice.Models;

namespace trnservice.Services
{
    public class TRNService : ITRNService
    {
        private readonly ILogger _logger;

        public TRNService(ILogger logger)
        {
            _logger = logger;
        }

        public FileResult SingleTRNValidation(TrnDTO trnDTO)
        {
            _logger.LogInformation("Is this null? "+ trnDTO.FirstName);
            string ltrn = trnDTO.Trn;
            ServiceReference1.MLSSServicesTRNClient obj = new ServiceReference1.MLSSServicesTRNClient();

            StringBuilder sb = new StringBuilder();

            string v = ltrn.ToString();
            // Converted to char away to check if all characters are numeric
            v.ToCharArray();


            if (ltrn == "" || (!v.All(char.IsDigit)))
            {
                // If trn is empty or not all numeric, assign default value of "111111111"
                ltrn = "111111111";
            }

            var objtrn = obj.GetIndividualTrn(int.Parse(ltrn));
            if (objtrn != null && objtrn.IndividualInfo != null)
            {
                // If found, print the First Name, Middle Name, Last Name, DOB, Gender, and TRN
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", objtrn.IndividualInfo.FirstName, objtrn.IndividualInfo.MiddleName, objtrn.IndividualInfo.LastName, objtrn.IndividualInfo.BirthDate.Value.ToShortDateString(), objtrn.IndividualInfo.GenderType, objtrn.IndividualInfo.NbrTrn));
            }
            else
            {
                // If not found, print the First Name, Middle Name, Last Name, DOB, Gender and "NOT FOUND"
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", trnDTO.FirstName, trnDTO.MiddleName, trnDTO.LastName, trnDTO.DateOfBirth, trnDTO.Gender, "NOT FOUND"));
            }
            var dateTime = DateTime.Now.ToString();
            // Transforming our String Builder into a memory stream, which is then be converted to File for Download
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            // Sets the position to the beginning of the stream before conversion
            memoryStream.Seek(0, SeekOrigin.Begin);


            return new FileContentResult(memoryStream.ToArray(), "application/octet-stream")
            {
                FileDownloadName = "TRN Results - " + dateTime + ".csv"
            };
        }

        public void MultipleTRNValidation(IFormFile formFile)
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
