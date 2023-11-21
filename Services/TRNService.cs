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
        private readonly ILogger _logger;

        public TRNService(ILogger logger)
        {
            _logger = logger;
        }

        public FileResult SingleTRNValidation(TrnViewModel trnDTO)
        {
            _logger.LogInformation("Is this null? "+ trnDTO.FirstName);
            string ltrn = trnDTO.Trn;
            ServiceReference1.MLSSServicesTRNClient obj = new ServiceReference1.MLSSServicesTRNClient();

            StringBuilder sb = new StringBuilder();

            // Add headings to String Builder
            sb.AppendLine("FIRSTNAME,MIDDLENAME,LASTNAME,DOB,GENDER,TRN");

            string v = ltrn.ToString();
            // Converted to char away to check if all characters are numeric
            v.ToCharArray();


            if (ltrn == "" || (!v.All(char.IsDigit)))
            {
                // If trn is empty or not all numeric, assign default value of "111111111"
                ltrn = "111111111";
            }

            var objtrn = obj.GetIndividualTrn(int.Parse(ltrn));
            if (objtrn != null && objtrn.IndividualInfo != null 
                // Assering that names match before returning a positive result
                && RawString(trnDTO.FirstName) == RawString(objtrn.IndividualInfo.FirstName)
                && RawString(trnDTO.LastName) == RawString(objtrn.IndividualInfo.LastName)
                && RawString(trnDTO.Gender) == RawString(objtrn.IndividualInfo.GenderType))
            {
                // If found, print the First Name, Middle Name, Last Name, DOB, Gender, and TRN
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", objtrn.IndividualInfo.FirstName, objtrn.IndividualInfo.MiddleName, objtrn.IndividualInfo.LastName, objtrn.IndividualInfo.BirthDate.Value.ToShortDateString(), objtrn.IndividualInfo.GenderType, objtrn.IndividualInfo.NbrTrn));
            }
            else
            {
                // If not found, print the First Name, Middle Name, Last Name, DOB, Gender and "NOT FOUND"
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", trnDTO.FirstName, trnDTO.MiddleName, trnDTO.LastName, trnDTO.DateOfBirth, trnDTO.Gender, "TRN MISMATCH"));
            }

            return GenerateTRNResponseFile(sb, ltrn+"_TRN_Result_");
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
                        //sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", line, objtrn.IndividualInfo.FirstName, objtrn.IndividualInfo.MiddleName, objtrn.IndividualInfo.LastName, objtrn.IndividualInfo.BirthDate.Value.ToShortDateString(), objtrn.IndividualInfo.GenderType, objtrn.IndividualInfo.NbrTrn));
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
