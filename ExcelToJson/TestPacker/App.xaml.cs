using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TestPacker
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string input = "";
            string output = "";
            string outputSource = "";
            bool startMinimized = false;
            if( e.Args.Length == 2)
            {
                input = e.Args[0];
                output = e.Args[1];
                outputSource = e.Args[2];
                startMinimized = true;
            }

            // Create main application window, starting minimized if specified
            if (startMinimized)
            {
                ConsoleManager.Show();
                var _excelProcessor = new ExcelToJsonConvert();
                _excelProcessor.outputLog += (string log) =>
                {
                    Console.WriteLine(log);
                };
                _excelProcessor.ConvertExcelFilesToJsonAndSource(input, output, outputSource,  ColumnDataCheckType.Client, false);
                Console.WriteLine("- 변환이 완료되었습니다. 엔터 입력 시 종료됩니다. -");
                Console.ReadLine(); //Pause
                System.Windows.Application.Current.Shutdown();
                return;
            }
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
