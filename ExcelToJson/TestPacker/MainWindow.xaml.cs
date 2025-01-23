using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using INI;
using System.Collections.ObjectModel;
using System.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TestPacker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public class ClientDictionaryInfo
    {
        public string table;
        public int key;
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static string[] Languages = { "kr", "en", "jp", "tw" };

        List<TodoItem> items = new List<TodoItem>();

        private string ouputJsonPath;
        public string OutputJsonPath
        {
            get { return ouputJsonPath; }
            set
            {
                ouputJsonPath = value;
                OnPropertyChanged("OutputJsonPath");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OutputJsonPath", ouputJsonPath);
            }
        }
        private bool outputJsonPathChecked;
        public bool OutputJsonPathChecked
        {
            get { return outputJsonPathChecked; }
            set
            {
                outputJsonPathChecked = value;
                OnPropertyChanged("OutputJsonPathChecked");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                if(outputJsonPathChecked)
                    ini.SetIniValue("TEST", "OutputJsonPathChecked", "1");
                else
                    ini.SetIniValue("TEST", "OutputJsonPathChecked", "0");
            }
        }
        private string ouputScriptableObject;
        public string OuputScriptableObject
        {
            get { return ouputScriptableObject; }
            set
            {
                ouputScriptableObject = value;
                OnPropertyChanged("OuputScriptableObject");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OuputScriptableObject", ouputScriptableObject);
            }
        }
        private bool outputJsonPathChecked2;
        public bool OutputJsonPathChecked2
        {
            get { return outputJsonPathChecked2; }
            set
            {
                outputJsonPathChecked2 = value;
                OnPropertyChanged("OutputJsonPathChecked2");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                if (outputJsonPathChecked2)
                    ini.SetIniValue("TEST", "OutputJsonPathChecked2", "1");
                else
                    ini.SetIniValue("TEST", "OutputJsonPathChecked2", "0");
            }
        }
        private string ouputSource;
        public string OuputSource
        {
            get { return ouputSource; }
            set
            {
                ouputSource = value;
                OnPropertyChanged("OuputScriptableObject");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OuputSource", ouputSource);
            }
        }
      
        private string ouputServerSource;
        public string OuputServerSource
        {
            get { return ouputServerSource; }
            set
            {
                ouputServerSource = value;
                OnPropertyChanged("OuputScriptableObject");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OuputServerSource", ouputServerSource);
            }
        }
       
        private string ouputManageToolSource;
        public string OuputManageToolSource
        {
            get { return ouputManageToolSource; }
            set
            {
                ouputManageToolSource = value;
                OnPropertyChanged("OuputManageToolSource");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OuputManageToolSource", ouputManageToolSource);
            }
        }
      

        private string outputJsonPathServer;
        public string OutputJsonPathServer
        {
            get { return outputJsonPathServer; }
            set
            {
                outputJsonPathServer = value;
                OnPropertyChanged("ExecelPath");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OutputJsonPathServer", outputJsonPathServer);
            }
        }

        private string excelPath;
        public string ExcelPath
        {
            get { return excelPath; }
            set
            {
                excelPath = value;
                OnPropertyChanged("ExecelPath");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "ExecelPath", excelPath);
            }
        }

        private string excelPathBuildStr;
        public string ExcelPathBuildStr
        {
            get { return excelPathBuildStr; }
            set
            {
                excelPathBuildStr = value;
                OnPropertyChanged("ExcelPathBuildStr");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "ExcelPathBuildStr", excelPathBuildStr);
            }
        }

        private string outputJsonPathBuildStr;
        public string OutputJsonPathBuildStr
        {
            get { return outputJsonPathBuildStr; }
            set
            {
                outputJsonPathBuildStr = value;
                OnPropertyChanged("OutputJsonPathBuildStr");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OutputJsonPathBuildStr", outputJsonPathBuildStr);
            }
        }

        private string excelPathPatchInfo;
        public string ExcelPathPatchInfo
        {
            get { return excelPathPatchInfo; }
            set
            {
                excelPathPatchInfo = value;
                OnPropertyChanged("ExcelPathPatchInfo");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "ExcelPathPatchInfo", excelPathPatchInfo);
            }
        }

        private string outputJsonPathPatchInfo;
        public string OutputJsonPathPatchInfo
        {
            get { return outputJsonPathPatchInfo; }
            set
            {
                outputJsonPathPatchInfo = value;
                OnPropertyChanged("OutputJsonPathPatchInfo");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "OutputJsonPathPatchInfo", outputJsonPathPatchInfo);
            }
        }

        private string serverExcelPath;
        public string ServerExcelPath
        {
            get { return serverExcelPath; }
            set
            {
                serverExcelPath = value;
                OnPropertyChanged("ServerExecelPath");

                FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
                string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
                string fileName = @"\config.ini";  //파일명
                INI.iniUtil ini = new INI.iniUtil(path + fileName);
                ini.SetIniValue("TEST", "ServerExecelPath", serverExcelPath);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);

            string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
            string fileName = @"\config.ini";  //파일명
            INI.iniUtil ini = new INI.iniUtil(path + fileName);
            do
            {
                var outputJsonPath = ini.GetIniValue("TEST", "OutputJsonPath");
                if (outputJsonPath.Equals(""))
                {
                    outputJsonPath = "";
                }
                OutputJsonPath = outputJsonPath;
            } while (false);
            do
            {
                var outputJsonPathChecked = ini.GetIniValue("TEST", "OutputJsonPathChecked");
                bool value = false;
                if (outputJsonPathChecked.Equals("1"))
                {
                    value = true;
                }
                OutputJsonPathChecked = value;
            } while (false);
            do
            {
                var outputJsonPathChecked2 = ini.GetIniValue("TEST", "OutputJsonPathChecked2");
                bool value = false;
                if (outputJsonPathChecked2.Equals("1"))
                {
                    value = true;
                }
                OutputJsonPathChecked2 = value;
            } while (false);
           
            do
            {
                var tempPath = ini.GetIniValue("TEST", "OuputScriptableObject");
                OuputScriptableObject = tempPath;
            } while (false);

            do
            {
                var tempPath = ini.GetIniValue("TEST", "OuputSource");
                OuputSource = tempPath;
            } while (false);


            do
            {
                var tempPath = ini.GetIniValue("TEST", "OuputServerSource");
                OuputServerSource = tempPath;
            } while (false);

            do
            {
                var excelPath = ini.GetIniValue("TEST", "ExecelPath");
                ExcelPath = excelPath;
            } while (false);
            do
            {
                var outputJsonPathServer = ini.GetIniValue("TEST", "OutputJsonPathServer");
                OutputJsonPathServer = outputJsonPathServer;
            } while (false);
            do
            {
                var excelPathBuildStr = ini.GetIniValue("TEST", "ExcelPathBuildStr");
                ExcelPathBuildStr = excelPathBuildStr;
            } while (false);
            do
            {
                var outputJsonPathBuildStr = ini.GetIniValue("TEST", "OutputJsonPathBuildStr");
                OutputJsonPathBuildStr = outputJsonPathBuildStr;
            } while (false);
            do
            {
                var excelPathPatchInfo = ini.GetIniValue("TEST", "ExcelPathPatchInfo");
                ExcelPathPatchInfo = excelPathPatchInfo;
            } while (false);
            do
            {
                var outputJsonPathPatchInfo = ini.GetIniValue("TEST", "OutputJsonPathPatchInfo");
                OutputJsonPathPatchInfo = outputJsonPathPatchInfo;
            } while (false);

            do
            {
                var serverExcelInfo = ini.GetIniValue("TEST", "ServerExecelPath");
                ServerExcelPath = serverExcelInfo;
            } while (false);

            do
            {
                var manageToolSourcePath = ini.GetIniValue("TEST", "OuputManageToolSource");
                OuputManageToolSource = manageToolSourcePath;
            } while (false);

           
            Rows = new ObservableCollection<TodoItem>();

            try
            {
                string path2 = AppDomain.CurrentDomain.BaseDirectory;
                path2 += "IgnoreScript.json";
                var ignoreParse = File.ReadAllText(path2);
                ignoreList = JsonConvert.DeserializeObject<List<string>>(ignoreParse);
            }
            catch (System.Exception excpt)
            {
                List<string> ignoreList = new List<string>();
                var serializeText = JsonConvert.SerializeObject(ignoreList);
                File.WriteAllText("./IgnoreScript.json", serializeText);
            }

            try
            {
                string path2 = AppDomain.CurrentDomain.BaseDirectory;
                path2 += "ClientDictionary.json";
                //if(File.Exists("./ClientDictionary.json"))
                //{
                //    path2 = "./ClientDictionary.json";
                //}
                //else
                //{
                //    path2 = "./ExcelConverter/ClientDictionary.json";
                //}
                var parse = File.ReadAllText(path2);
                clientDictionary = JsonConvert.DeserializeObject<List<ClientDictionaryInfo>>(parse);
            }
            catch (System.Exception excpt)
            {
                List<ClientDictionaryInfo> clientDictionary = new List<ClientDictionaryInfo>();
                clientDictionary.Add(new ClientDictionaryInfo
                {
                    table = "sample",
                    key = 0,
                });
                var serializeText = JsonConvert.SerializeObject(clientDictionary);
                File.WriteAllText("./ClientDictionary.json", serializeText);
            }
           
            Button_Click_View(null, null);
            if (_excelProcessor == null)
            {
                _excelProcessor = new ExcelToJsonConvert();
            }
            _excelProcessor.outputLog += (string log) =>
            {
                pbStatus.Dispatcher.BeginInvoke(new Action(() =>
                {
                    TodoItem item = new TodoItem();
                    item.Title = log;
                    Rows.Add(item);
                    ___Log___ListBox_.ScrollIntoView(___Log___ListBox_.Items[___Log___ListBox_.Items.Count - 1]);

                    //pbStatus.Value = progressMax01 + progressMax02 * count / Languages.Count();
                }));
            };
            _excelProcessor.NoticeLog("엑셀 리스트에 체크 하나도 안 되어있으면 전체 스크립트 출력 & 코드 출력");
            _excelProcessor.NoticeLog("엑셀 개별로 뽑으면 소스 코드는 출력하지 않습니다. ");
            _excelProcessor.NoticeLog("중요 : 스크립트 추가, 제거 또는 변수 추가 시 코드 다시 출력 해야 합니다. ");
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void GetFolderList(string sDir, ArrayList ret)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, "*.plist"))
                    {
                        ret.Add(f);
                    }
                    GetFolderList(d, ret);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        public struct FolderInfo
        {
            public string inputPath;
            public string outputPath;
        }
        public List<string> checkboxStringList = new List<string>();
        private ExcelToJsonConvert _excelProcessor = null;

        delegate void delegateMothod();


        private void InitCopyAll(string _source, string _target)
        {
            if (Directory.Exists(_target))
                Directory.Delete(_target, true);
            CopyAll(_source, _target);
        }

        private async void DeleteCopyAll(string _target)
        {
            if (!Directory.Exists(_target))
            {
                _excelProcessor.NoticeLog("excel Copy 없음");
                return;
            }

            try
            {
                // 디렉토리 안의 모든 파일과 하위 디렉토리 삭제
                foreach (string file in Directory.EnumerateFiles(_target))
                {
                    File.Delete(file);
                }
                foreach (string dir in Directory.EnumerateDirectories(_target))
                {
                    Directory.Delete(dir, true);
                }

                Directory.Delete(_target, true);

                _excelProcessor.NoticeLog("excel Copy 삭제 완료");
            }
            catch (IOException e)
            {
                MessageBox.Show("excel Copy 삭제 실패 에러: " + e.Message);
            }
        }

        private void InitCopyFile(string _source, string _target, List<string> changefileName)
        {
            if (Directory.Exists(_target) == false)
                Directory.CreateDirectory(_target);
            foreach (var item in changefileName)
            {
                if (File.Exists($"{_target}/{item}"))
                    File.Delete($"{_target}/{item}");

                if (item.Contains(".meta") == false)
                    File.Copy($"{_source}/{item}", $"{_target}/{item}", true);
            }

        }
        private void InitCopyAllowed(string _source, string _target, string[] allowedFiles)
        {
            if (Directory.Exists(_target))
                Directory.Delete(_target, true);
            CopyAllowed(_source, _target, allowedFiles);
        }

        private async void ConvertExcel()
        {
            ___Button___Extract_.IsEnabled = false;
            pbStatus.Value = 0;

            if (_excelProcessor == null)
            {
                _excelProcessor = new ExcelToJsonConvert();
            }

            bool bError = false;
            delegateMothod del01 = () =>
            {
                try
                {
                    pbStatus.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TodoItem item = new TodoItem();
                        item.Title = "WORK1";
                        Rows.Add(item);
                        ___Log___ListBox_.ScrollIntoView(___Log___ListBox_.Items[___Log___ListBox_.Items.Count - 1]);
                        pbStatus.Value = 10;
                    }));

                    InitCopyAll(excelPath, "excel");
                    if (OutputJsonPathChecked2)
                    {
                        InitCopyAll(serverExcelPath, "excelServer");
                    }



                    List<SourceInfo> sourceInfo;
                    List<string> changefileName;
                    if (_excelProcessor.ConvertExcelFilesToData("excel", out List<DataSet> excel, checkboxStringList))
                    {
                        bool check = checkboxStringList != null && checkboxStringList.Count > 0;
                        if (check == false && Directory.Exists(OutputJsonPath))
                            Directory.Delete(OutputJsonPath, true);

                        var clientExcel = new List<DataSet>();
                        for (int i = 0; i < excel.Count; i++)
                        {
                            clientExcel.Add(excel[i].Copy());
                        }

                        do//클라이언트 처리
                        {
                            if (OutputJsonPath == null || OutputJsonPath.Length == 0)
                                break;
                            if (OutputJsonPathChecked == false)
                                break;
                            _excelProcessor.ConvertExcelDataToJson(clientExcel, OutputJsonPath, ColumnDataCheckType.Client, out sourceInfo, out changefileName, false);

                            if (check == false && Directory.Exists(OuputSource))
                            {
                                Directory.Delete(OuputSource, true);
                                _excelProcessor.ConvertExcelDataToClinetSource(sourceInfo, OuputSource, ignoreList, clientDictionary);
                            }

                        } while (false);


                        do//서버 처리
                        {
                            if (OutputJsonPathServer == null || OutputJsonPathServer.Length == 0)
                                break;
                            
                            if (OutputJsonPathChecked2 == false)
                                break;

                            if (_excelProcessor.ConvertExcelFilesToData("excelServer", out List<DataSet> excelServer, checkboxStringList))
                            {
                            }

                            excelServer.AddRange(excel);

                            List<SourceInfo> serverSourceInfo;
                            List<string> serverChangefileName;
                            var serverOutputJsonPath = $"{OutputJsonPathServer}/GameServer/scripts";

                            _excelProcessor.ConvertExcelDataToJson(excelServer, serverOutputJsonPath, ColumnDataCheckType.Server, out serverSourceInfo, out serverChangefileName, check);

                            if (false == string.IsNullOrEmpty(OuputServerSource))
                            {
                                if(check == false)
                                {
                                    InitCopyAll(serverOutputJsonPath, $"{OutputJsonPathServer}/ServiceInfoServer/scripts");
                                    InitCopyAll(serverOutputJsonPath, $"{OutputJsonPathServer}/SignServer/scripts");
                                    InitCopyAll(serverOutputJsonPath, $"{OutputJsonPathServer}/bin/scripts");
                                    InitCopyAll(serverOutputJsonPath, $"{OutputJsonPathServer}/bin/DedicatedServer/scripts");
                                    InitCopyAll(serverOutputJsonPath, $"{OutputJsonPathServer}/bin/GameServer/scripts");
                                }
                                else
                                {
                                    InitCopyFile(serverOutputJsonPath, $"{OutputJsonPathServer}/ServiceInfoServer/scripts", serverChangefileName);
                                    InitCopyFile(serverOutputJsonPath, $"{OutputJsonPathServer}/SignServer/scripts", serverChangefileName);
                                    InitCopyFile(serverOutputJsonPath, $"{OutputJsonPathServer}/bin/scripts", serverChangefileName);
                                    InitCopyFile(serverOutputJsonPath, $"{OutputJsonPathServer}/bin/DedicatedServer/scripts", serverChangefileName);
                                    InitCopyFile(serverOutputJsonPath, $"{OutputJsonPathServer}/bin/GameServer/scripts", serverChangefileName);
                                }

                                if (check == false && Directory.Exists(OuputServerSource + "/Data/AutoJson"))
                                {
                                    Directory.Delete(OuputServerSource + "/Data/AutoJson", true);
                                    _excelProcessor.ConvertExcelDataToServerSource(serverSourceInfo, OuputServerSource);
                                }

                            }

                            if (false == string.IsNullOrWhiteSpace(OuputManageToolSource))
                            {
                                if (check == false && Directory.Exists(OuputManageToolSource + "/scripts"))
                                    Directory.Delete(OuputManageToolSource + "/scripts", true);

                                InitCopyAll(serverOutputJsonPath, $"{OuputManageToolSource}/scripts");

                                if (check == false && Directory.Exists(OuputManageToolSource + "/Data/AutoJson"))
                                {
                                    Directory.Delete(OuputManageToolSource + "/Data/AutoJson", true);
                                    _excelProcessor.ConvertExcelDataToServerSource(serverSourceInfo, OuputManageToolSource);
                                }

                            }

                            

                        } while (false);
                    }


                }
                catch (System.InvalidOperationException e )
                {
                    pbStatus.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(this, "출력될 엑셀이 켜져있습니다 꺼주세요 \n" + e.ToString());
                    }));
                    bError = true;
                }
                catch (Exception ex)
                {
                    pbStatus.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(this, "알수없는 에러입니다. 동일한 이름이 존재하는지 확인해주세요" + ex.ToString());
                    }));
                    bError = true;
                }
            };

            var t1 = Task<int>.Run(() => del01());
            await t1;

            if (bError)
                return;
            
            // 엑셀 카피본 제거
            var t2 = Task<int>.Run(() => DeleteCopyAll("excel"));
            await t2;

            //02. plist 작성

            pbStatus.Value = 100;
            ___Button___Extract_.IsEnabled = true;
            MessageBox.Show(this, "완료 되었습니다");
        }

        private void Button_Click_Export(object sender, RoutedEventArgs e)
        {
            ConvertExcel();
        }	
		private void Button_Click_View(object sender, RoutedEventArgs e)
        {
            if (_excelProcessor == null)
            {
                _excelProcessor = new ExcelToJsonConvert();
            }
            ___ListBox__Excel.Items.Clear();
            checkboxStringList.Clear();
            List<string> excelFiles = _excelProcessor.GetExcelFileNamesInDirectoryFilename(ExcelPath);
            Regex excelRegex = new Regex(@"^(?!~\$).*\.(xlsx|xls)$");
            for (int i = 0; i < excelFiles.Count; i++)
            {
                if (!excelRegex.IsMatch(excelFiles[i]))
                {
                    continue;
                }

                CheckBox item = new CheckBox();
               
                item.Content = System.IO.Path.GetFileName(excelFiles[i]);
                item.IsChecked = false;
                item.Checked += chk_Click;
                item.Unchecked += chk_Click;
                item.Foreground = new SolidColorBrush(Colors.Orange);
                ___ListBox__Excel.Items.Add(item);
            }
           
        }
        private void chk_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (checkboxStringList.Contains(chk.Content.ToString()))
                checkboxStringList.Remove(chk.Content.ToString());
            else
                checkboxStringList.Add(chk.Content.ToString());
        }

        private ObservableCollection<TodoItem> m_Rows;
        private List<string> ignoreList;
        private List<ClientDictionaryInfo> clientDictionary;

        public ObservableCollection<TodoItem> Rows
        {
            get { return m_Rows; }
            set { m_Rows = value; }
        }

        static public void CopyAll(string _source, string _target)
        {
            if (Directory.Exists(_target) == false)
                Directory.CreateDirectory(_target);

            foreach (var file in Directory.GetFiles(_source))
            {
                if (file.Contains(".meta") == false)
                    File.Copy(file, System.IO.Path.Combine(_target, System.IO.Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(_source))
                CopyAll(directory, _target +"/"+ directory.Substring(directory.LastIndexOf('\\')+1));
        }

        static public void CopyAllowed(string _source, string _target, string[] allowedFiles)
        {
            foreach (var file in Directory.GetFiles(_source))
            {
                if (allowedFiles.Contains(System.IO.Path.GetFileName(file).Replace(".json", "").ToLower()) == false)
                    continue;

                if (file.Contains(".meta") == false)
                {
                    if (Directory.Exists(_target) == false)
                        Directory.CreateDirectory(_target);
                    File.Copy(file, System.IO.Path.Combine(_target, System.IO.Path.GetFileName(file)), true);
                }
            }

            foreach (var directory in Directory.GetDirectories(_source))
                CopyAllowed(directory, _target + "/" + directory.Substring(directory.LastIndexOf('\\') + 1), allowedFiles);
        }

        private void Button_Click_MessagePack(object sender, RoutedEventArgs e)
        {
            FileInfo exefileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "MessagePackBuild.bat");
            if (exefileinfo.Exists)
            {
                Process.Start(exefileinfo.FullName);
                return;
            }
            else
            {

            }

            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var path = AppDomain.CurrentDomain.BaseDirectory.Replace(directory.Name + "\\", "") + "MessagePackBuild.bat";
            exefileinfo = new FileInfo(path); 
            if (exefileinfo.Exists)
            {
                Process.Start(exefileinfo.FullName);
                return;
            }
            MessageBox.Show("MessagePackBuild파일을 찾을 수 없습니다." + Environment.NewLine + path);
        }

        private void __EditBox__JsonPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void __EditBox__ExcelPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
		
		private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(ExcelPath.Length > 0)
                System.Diagnostics.Process.Start(ExcelPath);
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if (ServerExcelPath.Length > 0)
                System.Diagnostics.Process.Start(ServerExcelPath);
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (OutputJsonPath.Length > 0)
                System.Diagnostics.Process.Start(OutputJsonPath);
        }
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            if (OuputSource.Length > 0)
                System.Diagnostics.Process.Start(OuputSource);
        }
        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            if (OutputJsonPathServer.Length > 0)
                System.Diagnostics.Process.Start(OutputJsonPathServer);
        }
        private void Button_Click5(object sender, RoutedEventArgs e)
        {
            if (OuputServerSource.Length > 0)
                System.Diagnostics.Process.Start(OuputServerSource);
        }

		private void IsJsonPathAllChecked(object sender, RoutedEventArgs e)
        {
            OutputJsonPathChecked = true;
            OutputJsonPathChecked2 = true;
        }
        private void IsJsonPathAllUnChecked(object sender, RoutedEventArgs e)
        {
            OutputJsonPathChecked = false;
            OutputJsonPathChecked2 = false;
        }
		private void IsJsonPath_Checked(object sender, RoutedEventArgs e)
        {
            OutputJsonPathChecked = true;
        }
        private void IsJsonPath_UnChecked(object sender, RoutedEventArgs e)
        {
            OutputJsonPathChecked = false;
        }
        private void IsJsonPath2_Checked(object sender, RoutedEventArgs e)
        {
            OutputJsonPathChecked2 = true;
        }
        private void IsJsonPath2_UnChecked(object sender, RoutedEventArgs e)
        {
            OutputJsonPathChecked2 = false;
        }
      
    }




    public class TodoItem
    {
        public string Title { get; set; }
        public int Completion { get; set; }
    }
}


