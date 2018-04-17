using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using TradingApp.Data.Models;

namespace TradingApp.Domain.Core
{
    public class PythonExec
    {
        
        private IOptions<ApplicationSettings> _appSettings;
        public PythonExec(IOptions<ApplicationSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public void RunPython(string path, int periods,  bool seasonalityHourly, bool seasonalityDaily)
        {           
            var arguments = $"{path} {periods} {seasonalityHourly} " +
                            $"{seasonalityDaily}";

            var start = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = _appSettings.Value.PythonLocation,
                Arguments = $"forecast.py {arguments}",
                RedirectStandardError = true
            };
            try
            {
                using (var process = Process.Start(start))
                { 
                    process.WaitForExit();                  
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    
    }
}