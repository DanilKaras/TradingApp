using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using TradingApp.Data.Models;

namespace TradingApp.Data.Managers
{
    public class FileManager
    {
        private readonly IOptions<ApplicationSettings> _services;

        public FileManager(IOptions<ApplicationSettings> services)
        {
            _services = services;
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
        
        public static List<string> ReadAssetsFromExcel(string path)
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
                throw new Exception("Symbols file is empty");
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
    }
}