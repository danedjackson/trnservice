using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using trnservice.Models;

namespace trnservice.Services
{
    public class TRNService : ITRNService
    {
        private readonly ILogger<TRNService> _logger;

        public TRNService(ILogger<TRNService> logger)
        {
            _logger = logger;
        }

        public bool SingleTRNValidation(TrnSearchRequestViewModel trnDTO)
        {
            string queryTrn = trnDTO.Trn;
            // Initiate the service used to fetch TRN information
            ServiceReference1.MLSSServicesTRNClient trnClient = new ServiceReference1.MLSSServicesTRNClient();

            // If the TRN is non-numeric or an empty string, then we default it to 111111111
            if (queryTrn == "" || (!queryTrn.All(char.IsDigit)))
            {
                queryTrn = "111111111";
            }

            // Querying the trn service with entered TRN 
            var trnSearchResult = trnClient.GetIndividualTrn(int.Parse(queryTrn));

            if(trnSearchResult != null && trnSearchResult.IndividualInfo != null
                // Assering that names match before returning a positive result
                && RawString(trnDTO.FirstName) == RawString(trnSearchResult.IndividualInfo.FirstName)
                && RawString(trnDTO.LastName) == RawString(trnSearchResult.IndividualInfo.LastName)
                && RawString(trnDTO.Gender) == RawString(trnSearchResult.IndividualInfo.GenderType))
            {
                return true;
            }

            return false;
        }

        public FileResult MultipleTRNValidation(IFormFile formFile)
        {
            string[] ltrn;

            ServiceReference1.MLSSServicesTRNClient obj = new ServiceReference1.MLSSServicesTRNClient();

            StringBuilder sb = new StringBuilder();

            using (var reader = new StreamReader(formFile.OpenReadStream()))
            {
                // Rewrite Headers
                sb.AppendLine(reader.ReadLine() + ", TRN STATUS");
                while(reader.Peek() >= 1)
                {
                    string line = reader.ReadLine();

                    ltrn = line.Split(',');
                    string v = ltrn[5].ToString();
                    // Converted to char away to check if all characters are numeric
                    v.ToCharArray();


                    if (ltrn[5] == "" || (!v.All(char.IsDigit)))
                    {
                        // If trn is empty or not all numeric, assign default value of "111111111"
                        ltrn[5] = "111111111";
                    }

                    // If Gender is not one character, format to match convention of single character
                    if(ltrn[10].Length > 1)
                    {
                        ltrn[10] = ltrn[10].Substring(0, 1);
                    }

                    var objtrn = obj.GetIndividualTrn(int.Parse(ltrn[5]));
                    if (objtrn != null && objtrn.IndividualInfo != null
                        // Assering that names match before returning a positive result
                        && RawString(ltrn[3]) == RawString(objtrn.IndividualInfo.FirstName)
                        && RawString(ltrn[4]) == RawString(objtrn.IndividualInfo.LastName)
                        && RawString(ltrn[10]) == RawString(objtrn.IndividualInfo.GenderType))
                    {
                        sb.AppendLine(line + ", TRN MATCHED");
                    }
                    else
                    {
                        sb.AppendLine(line + ", TRN MISMATCHED");
                    }
                }
            }
            
            return GenerateTRNResponseFile(sb, "BULK_TRN_Results_");
        }

        private FileContentResult GenerateTRNResponseFile(StringBuilder sb, string fileName)
        {
            var dateTime = DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");

            // Transforming our String Builder into a memory stream, which is then be converted to File for Download
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

            // Sets the position to the beginning of the stream before conversion
            memoryStream.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation("Converting memory stream to file for download.");
            return new FileContentResult(memoryStream.ToArray(), "application/octet-stream")
            {
                FileDownloadName = fileName + dateTime + ".csv"
            };
        }

        private string RawString(string input)
        {
            return Regex.Replace(input, "[^a-zA-Z]", "").ToLower();
        }
    }
}
