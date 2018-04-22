using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.CoinOptimizationRelated;

namespace TradingApp.Data.Managers
{
    public class FileManager : IFileManager
    {
        private readonly IOptions<ApplicationSettings> _settings;
        private readonly string _env;
        private readonly NumberFormatInfo _numFormat;
        public FileManager(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
            _numFormat = new CultureInfo("en-US", false ).NumberFormat;
            _numFormat.PercentDecimalDigits = 3;
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

        public List<ExcelLog> ReadLog(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var excelLog = new List<ExcelLog>();
                var file = new FileInfo(path);
                using (var package = new ExcelPackage(file))
                {       
                    var worksheet = package.Workbook.Worksheets[1];
                    var rowCount = worksheet.Dimension.Rows;          
                    for (var row = 1; row <= rowCount; row++)
                    {
                        excelLog.Add(new ExcelLog()
                        {
                            AssetName = worksheet.Cells[row, 1].Value.ToString(),
                            Log =  worksheet.Cells[row, 2].Value.ToString(),
                            Rate = worksheet.Cells[row,3].Value.ToString()
                        });                 
                    }
                }
            
                return excelLog;
            }
            catch (Exception)
            {
                throw new Exception("Log file is empty");
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
            IDirectoryManager directory = new DirectoryManager(_settings, _env);
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
                        avg = ((decimal.Parse(subModel.Close, NumberStyles.Any, CultureInfo.InvariantCulture) +
                                decimal.Parse(subModel.High, NumberStyles.Any, CultureInfo.InvariantCulture) +
                                decimal.Parse(subModel.Low, NumberStyles.Any, CultureInfo.InvariantCulture)) / 3) * 100;
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
        
        public void WriteLogExcel(string path, IEnumerable<ExcelLog> log)
        {
            var file = new FileInfo(Path.Combine(path, _settings.Value.AssetFile));
            var query = (from p in log
                         group p by p.Log into g
                         select new { key = g.Key, list = g.Select(x=> new {Asset = x.AssetName, Rate = x.Rate}).ToList() }).ToList();


            var positiveGroup = query.Where(x => x.key == Indicator.Positive.ToString()).Select(x => x).SingleOrDefault();
            var neutralGroup = query.Where(x => x.key == Indicator.Neutral.ToString()).Select(x => x).SingleOrDefault();
            var negativeGroup = query.Where(x => x.key == Indicator.Negative.ToString()).Select(x => x).SingleOrDefault();
            var zeroGroup = query.Where(x =>x.key == Indicator.ZeroRezults.ToString()).Select(x => x).SingleOrDefault();
            var strongPositiveGroup = query.Where(x =>x.key == Indicator.StrongPositive.ToString()).Select(x => x).SingleOrDefault();          
            
            var sortedLog = new List<ExcelLog>();
            
            if (strongPositiveGroup != null)
            {
                var strongPositive = strongPositiveGroup.list.Select(x => new { x.Asset, x.Rate }).OrderByDescending(x => x.Rate).ToList();
                foreach (var item in strongPositive)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", _numFormat), Log = Indicator.StrongPositive.ToString()});
                }
            }
            
            if (positiveGroup != null)
            {
                var positive = positiveGroup.list.Select(x => new { x.Asset, x.Rate }).OrderByDescending(x => Convert.ToDecimal(x.Rate)).ToList();
                foreach (var item in positive)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", _numFormat), Log = Indicator.Positive.ToString()});
                }
            }

            if (neutralGroup != null)
            {
                var neutral = neutralGroup.list.Select(x => new { x.Asset, x.Rate }).OrderBy(x => Convert.ToDecimal(x.Rate)).ToList();
                foreach (var item in neutral)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", _numFormat), Log = Indicator.Neutral.ToString()});
                }
            }

            if (negativeGroup != null)
            {
                var negative = negativeGroup.list.Select(x => new { x.Asset, x.Rate }).OrderBy(x => Convert.ToDecimal(x.Rate)).ToList();
                foreach (var item in negative)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", _numFormat), Log = Indicator.Negative.ToString()});
                }
            }

            if (zeroGroup != null)
            {
                var zero = zeroGroup.list.Select(x => new { x.Asset, x.Rate }).ToList();
                foreach (var item in zero)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = "Unknown", Log = Indicator.ZeroRezults.ToString()});
                }
            }

            //return sortedLog;
            IDirectoryManager manager = new DirectoryManager(_settings, _env);
            manager.WriteLogToExcel(file, sortedLog);
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


        public void WriteObservables(IEnumerable<string> list, string path)
        {
            var file = new FileInfo(path);
            using (var package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                var rowNumber = 1;
                foreach (var asset in list)
                {
                    worksheet.Cells[rowNumber, 1].Value = asset;
                    rowNumber++;
                }

                package.Save();
            }
        }
    }
}