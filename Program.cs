using iTextSharp.text.pdf.parser;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace trnservice
{
    class Program
    {
        static void Main(string[] args)
        {
            String cTrn;
            String[] ltrn;
            Console.WriteLine("Hello World!");
         


            ServiceReference1.MLSSServicesTRNClient obj = new ServiceReference1.MLSSServicesTRNClient();
            //var objtrn = obj.GetIndividualTrnAsync(111734878).Result;
            //Console.WriteLine(objtrn);

            StringBuilder sb = new StringBuilder();

            foreach (String line in File.ReadLines(@"C:\Users\robinsonod\Downloads\Reports\WestmorelandWestern17-05-2023remainder.csv"))
            {

                ltrn = line.Split(',');
                string v = ltrn[1].ToString();
                string test = v;
                v.ToCharArray();


                if (ltrn[1] == "" || (!v.All(char.IsDigit)))

                {
                    ltrn[1] = "111111111";
                }

                var objtrn = obj.GetIndividualTrn(int.Parse(ltrn[1]));
                if (objtrn != null && objtrn.IndividualInfo != null)
                {
                    sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6}", line, objtrn.IndividualInfo.FirstName, objtrn.IndividualInfo.MiddleName, objtrn.IndividualInfo.LastName, objtrn.IndividualInfo.BirthDate.Value.ToShortDateString(), objtrn.IndividualInfo.GenderType, objtrn.IndividualInfo.NbrTrn));
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



