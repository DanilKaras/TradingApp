using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using TradingApp.Data.Models;

namespace TradingApp.Data.Managers
{
    public class FileManager
    {
        private readonly IOptions<ApplicationSettings> _settings;
        private readonly string _env;

        public FileManager(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public void WriteAssetsToExcel(string path, ExchangeData exchangeData)
        {
            var file = new FileInfo(path);
            using (var package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                var rowNumber = 1;
                foreach (var pair in exchangeData.Pairs)
                {
                    var asset = pair.CurrencyFrom + "_" +
                                pair.CurrencyTo + "_" +
                                exchangeData.ExchangeName;

                    worksheet.Cells[rowNumber, 1].Value = asset;
                    rowNumber++;
                }

                package.Save();
            }
        }

        public IEnumerable<string> ReadAssetsFromExcel(string path)
        {
            try
            {
                var file = new FileInfo(path);
                var rawText = new List<string>();
                using (var package = new ExcelPackage(file))
                {
                    var worksheet = package.Workbook.Worksheets[1];
                    var rowCount = worksheet.Dimension.Rows;
                    for (var row = 1; row <= rowCount; row++)
                    {
                        rawText.Add(worksheet.Cells[row, 1].Value.ToString());
                    }
                }

                return rawText;
            }
            catch (Exception)
            {
                throw new Exception("Symbol file is empty");
            }
        }


        public CustomSettings ReadCustomSettings(string json)
        {
            var settings = JsonConvert.DeserializeObject<CustomSettings>(json);
            return settings;
        }

        public string ConvertCustomSettings(CustomSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings);
            return json;
        }

        public bool CreateDataCsv(List<CoinOptimized> model, string saveToLocation)
        {
            var directory = new DirectoryManager(_settings, _env);
            var csv = new StringBuilder();
            const string fs = "Time";
            const string sc = "avg";
            var beginning = $"{fs},{sc}{Environment.NewLine}";

            csv.Append(beginning);
            var counter = 0;
            var maxcount = 0;

            foreach (var item in model)
            {
                maxcount++;
            }

            foreach (var subModel in model)
            {
                counter++;
                var formattedDate = subModel.Time.ToString("u").Replace("Z", ""); // + " UTC";
                decimal avg = 0;
                try
                {
                    checked
                    {
                        avg = ((Convert.ToDecimal(subModel.Close) +
                                Convert.ToDecimal(subModel.High) +
                                Convert.ToDecimal(subModel.Low)) / 3) * 100;
                    }
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("CHECKED and CAUGHT:  " + e);
                }

                var second = avg.ToString(CultureInfo.CurrentCulture);
                var newLine = counter < maxcount
                    ? $"{formattedDate},{second}{Environment.NewLine}"
                    : $"{formattedDate},{second}";
                csv.Append(newLine);
            }

            if (csv.Length > 0)
            {
                directory.SaveDataFile(csv, saveToLocation);
                return true;
            }

            return true;
        }
    }
}