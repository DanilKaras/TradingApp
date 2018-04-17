using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        
        public OutStats BuildOutTableRows(string path, int period)
        {
            var outStats = new OutStats();
            var table = new List<TableRow>();
            
            using (var reader = new StreamReader(File.OpenRead($"{path}")))
            {
                var counter = 0;

                var dsPos = 0;
                var yhatPos = 0;
                var yhatUpperPos = 0;
                var yhatLowerPos = 0;
                while (!reader.EndOfStream)
                {
                    counter++;
                    var line = reader.ReadLine();

                    if (line == null) continue;
                    var values = line.Split(',');
                    if (counter == 1)
                    {
                        dsPos = FindPosition(values, "ds");
                        yhatPos = FindPosition(values, "yhat");
                        yhatUpperPos = FindPosition(values, "yhat_upper");
                        yhatLowerPos = FindPosition(values, "yhat_lower");
                    }
                    else
                    {
                        var row = new TableRow()
                        {
                            Id = values[0],
                            Ds = values[dsPos],
                            Yhat = decimal.Parse(values[yhatPos], NumberStyles.Any, CultureInfo.InvariantCulture),
                            YhatUpper = decimal.Parse(values[yhatUpperPos], NumberStyles.Any, CultureInfo.InvariantCulture),
                            YhatLower = decimal.Parse(values[yhatLowerPos], NumberStyles.Any, CultureInfo.InvariantCulture) 
                        };
                        table.Add(row);
                    }
                }
            }


            outStats.MaxValue = table.Take(table.Count - period).Select(x => x.Yhat).Max();
            outStats.MinValue = table.Take(table.Count - period).Select(x => x.Yhat).Min();
            outStats.Table = table.Skip(Math.Max(0, table.Count() - period)).Reverse().ToList();

            return outStats; 
        }
        
        private static int FindPosition(IEnumerable<string> array, string colName)
        {
            var pos = -1;
            foreach (var item in array)
            {
                pos++;
                if (item == colName)
                {
                    return pos;
                }
            }
            return 0;
        }
    }
}