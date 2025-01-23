using Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestPacker
{
    public enum ColumnDataCheckType
    {
        Server = 1,
        Client = 2
    }
    public class ETJColumn
    {
        public bool key;
        public string dataType;
        public string column;
    }

    public class SourceInfo
    {
        public string table;
        public List<ETJColumn> colums;

    }

    public class ExcelToJsonConvert
    {
        public delegate void ConversionToJsonSuccessfullHandler();
        public event ConversionToJsonSuccessfullHandler ConversionToJsonSuccessfull = delegate { };

        public delegate void ConversionToJsonFailedHandler();
        public event ConversionToJsonFailedHandler ConversionToJsonFailed = delegate { };

        public delegate void OutputLog(String log);
        public event OutputLog outputLog = delegate { };




        /// <summary>
        /// Converts all excel files in the input folder to json and saves them in the output folder.
        /// Each sheet within an excel file is saved to a separate json file with the same name as the sheet name.
        /// Files, sheets and columns whose name begin with '~' are ignored.
        /// </summary>
        /// <param name="inputPath">Input path.</param>
        /// <param name="outputPath">Output path.</param>
        /// <param name="recentlyModifiedOnly">If set to <c>true</c>, will only process recently modified files only.</param>
        /// 


        public bool ConvertExcelFilesToData(string inputPath, out List<DataSet> excel, List<string> checkFile = null)
        {
            excel = new List<DataSet>();

            List<string> excelFiles = GetExcelFileNamesInDirectory(inputPath);
            int checkExcel = 0;
            outputLog("총 " + excelFiles.Count.ToString() + " 개의 엑셀파일을 찾았습니다.");

            bool check = checkFile != null && checkFile.Count > 0;
            for (int i = 0; i < excelFiles.Count; i++)
            {
                if(check)
                {
                    bool con = false;
                    foreach (var item in checkFile)
                    {
                        if (excelFiles[i].Contains(item))
                        {
                            con = true;
                            break;
                        }
                    }
                    if (con == false)
                        continue;
                }
                checkExcel++;
                GetExcelData(excelFiles[i], ref excel);
            }
            if(check)
                outputLog("그중에 총 " + checkExcel.ToString() + " 개의 엑셀파일이 체크되었습니다");
            ConversionToJsonSuccessfull();
            return true;

        }

        public bool ConvertExcelDataToJson(List<DataSet> excel, string outputPath, ColumnDataCheckType checkType, out List<SourceInfo> sourceInfo, out List<string> changefileName, bool fileCopy = false)
        {
            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);


            List<string> failList = new List<string>();
            sourceInfo = new List<SourceInfo>();
            changefileName = new List<string>();

            var ignoreList = AppDomain.CurrentDomain.BaseDirectory + "IgnoreExcelScript.json";
            List<string> filterList = new List<string>();
            //File.WriteAllText ("./IgnoreExcelScript.json", ignoreList);
            try
            {
                var ignoreParse = File.ReadAllText(ignoreList);
                filterList = JsonConvert.DeserializeObject<List<string>>(ignoreParse);
                foreach (var item in filterList)
                {
                    outputLog($" {item} 을 필터링 했습니다.");
                }
            }
            catch(Exception e)
            {
                outputLog($" 필터링 실패 : {e}");
            }

            for (int i = 0; i < excel.Count; i++)
            {
                ConvertExcelDataToJson(excel[i], outputPath, filterList, checkType, ref failList, ref sourceInfo, ref changefileName, fileCopy);
            }

            return true;
        }


        public bool ConvertExcelDataToClinetSource(List<SourceInfo> sourceInfo, string outputPath, List<string> ignoreList, List<ClientDictionaryInfo> clientDictionary)
        {
            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);


            List<CreateFileArg> loadScript = new List<CreateFileArg>();

            for (int i = 0; i < sourceInfo.Count; ++i)
            {
                ConvertExcelDataToClinetSource(sourceInfo[i], outputPath, ignoreList, clientDictionary, ref loadScript);
            }

            if (!CreateLoadSource(outputPath, ref loadScript))
            {
                ConversionToJsonFailed();
                return false;
            }

            return true;
        }

        public void ConvertExcelDataToClinetSource(SourceInfo excelData, string outputPath, List<string> ignoreList, List<ClientDictionaryInfo> clientDictionary, ref List<CreateFileArg> loadScript)
        {
            string fileName = excelData.table.Replace(" ", string.Empty);
            fileName = fileName.Replace("_", "/");

            string directory = Path.GetDirectoryName(outputPath + "/" + fileName + ".cs");
      
            string sourceFileName = Path.GetFileName(outputPath + "/" + fileName + ".cs");


            string sourceName = sourceFileName.Substring(0, sourceFileName.IndexOf('.'));
            string scriptClassName = Regex.Replace(sourceName, @"[\d]", "");
            scriptClassName = $"{char.ToUpper(scriptClassName[0])}{scriptClassName.Substring(1)}";



            var valName = scriptClassName.ToLower();
            var valNameNum = sourceName.ToLower();

            bool ignore = false;
            if (ignoreList != null)
            {
                ignore = ignoreList.Any(_ => _.ToLower() == valNameNum);
                if (ignore)
                    return;
            }

            ClientDictionaryInfo dictionaryInfo = null;
            if (clientDictionary != null)
            {
                dictionaryInfo = clientDictionary.Find(_ => _.table == sourceName);
            }

            if (valNameNum.CompareTo(valName.ToLower()) != 0)
                return;

            var checkCreateSource = loadScript.Find(_ => _.className == scriptClassName);
            if (checkCreateSource != null)
                return;


            loadScript.Add(new CreateFileArg() { className = scriptClassName, jsonfile = sourceName });

            directory = directory.Replace("\\", "/");

            string bundleName = Path.GetDirectoryName(fileName + ".cs"); ;

            bundleName = $"scripts/{bundleName}";
            var relativeDirectory = bundleName;
            bundleName = bundleName.ToLower();

            var sb = new StringBuilder();
            sb.AppendLine(@"
/********************************************************/
/*Auto Create File*/
/*Source : ExcelToJsonConvert*/
/********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MessagePack;
using UnityEngine;
using UniRx;
using Data;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
");
            sb.AppendLine($@"
[Serializable][MessagePackObject]
public class {scriptClassName}Script
{{
");
            for (int k = 0; k < excelData.colums.Count; k++)
            {
                sb.AppendLine($"    [Key({k})]public {excelData.colums[k].dataType} {excelData.colums[k].column};");
                //sb.AppendLine($"    public {excelData.colums[k].dataType} {excelData.colums[k].column};");
            }
            
            sb.AppendLine($@"
}}

public partial class DataManager
{{
    [Serializable][MessagePackObject]
    public class {scriptClassName}ScriptAll
    {{
        [Key(0)]public List<{scriptClassName}Script> result;
    }}

");
            if (dictionaryInfo == null)
            {

                sb.AppendLine($@"
    private List<{scriptClassName}Script> list{scriptClassName}Script = null;
");
                if (!ignore)
                {
                    sb.AppendLine($@"
    public {scriptClassName}Script Get{scriptClassName}Script(Predicate<{scriptClassName}Script> predicate)
    {{
        return list{scriptClassName}Script?.Find(predicate);
    }}
    public List<{scriptClassName}Script> Get{scriptClassName}ScriptList {{ 
        get {{ 
                return list{scriptClassName}Script;
        }}
    }}
");
                }

                sb.AppendLine($@"

    void Clear{scriptClassName}()
    {{
        list{scriptClassName}Script?.Clear();
    }}
");
            }
            else
            {
                sb.AppendLine($@"
    private Dictionary<{excelData.colums[dictionaryInfo.key].dataType}, {scriptClassName}Script> dic{scriptClassName}Script = new Dictionary<{excelData.colums[dictionaryInfo.key].dataType}, {scriptClassName}Script>();

    void Clear{scriptClassName}()
    {{
        dic{scriptClassName}Script.Clear();
    }}

");
            }

            string strJsonPath = "\"{ \\\"result\\\" : \" + load + \"}\"";
            string dediPath = $"./{relativeDirectory}/{sourceName}.json";
            sb.AppendLine($@"
    async UniTask LoadScript{scriptClassName}()
    {{
        List<{scriptClassName}Script> resultScript = null;
        if(resultScript == null)
        {{
            var load = await Managers.Resource.LoadScript(""{bundleName}"", ""{sourceName}""); 
            if (load == """") 
            {{
                Debug.LogWarning(""{scriptClassName} is empty"");
                return;
            }}
            var json = JsonUtility.FromJson<{scriptClassName}ScriptAll>({strJsonPath});
            resultScript = json.result;
        }}

");


            if (dictionaryInfo != null)
            {
                sb.AppendLine($@"
        for (int i = 0; i < resultScript.Count; i++)
            dic{scriptClassName}Script.Add( resultScript[i].{excelData.colums[dictionaryInfo.key].column}, resultScript[i]);
");
            }
            else
            {
                sb.AppendLine($@"
        list{scriptClassName}Script = resultScript;
");
            }


            var path = "Assets/BackGround/Prefabs/" + bundleName + "/" + sourceName + ".json";
            var convertPath = "Assets/BackGround/Prefabs/" + bundleName + "/" + sourceName + ".bytes";
            sb.AppendLine($@"
//#endif
    }}
#if UNITY_EDITOR
    public static async UniTask ConvertBinary{scriptClassName}()
    {{
        var path = ""{path}"";
        var load = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
        var resultScript = JsonUtility.FromJson<{scriptClassName}ScriptAll>({strJsonPath});
        var convertBytes = MessagePackSerializer.Serialize(resultScript);
        var convertPath = ""{convertPath}"";
        File.WriteAllBytes(convertPath, convertBytes);
        try
        {{
            UnityEngine.Windows.File.Delete(path);
        }}
        catch
        {{
        }}
    }}
#endif
}}

");
            WriteTextToFile(sb.ToString(), Path.Combine(outputPath, $"DataManager.{scriptClassName}.cs"));
            outputLog("= " + outputPath + $"DataManager.{scriptClassName}.cs" + " 생성");
        }

        public bool ConvertExcelDataToServerSource(List<SourceInfo> sourceInfo, string outputPath)
        {
            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);

            List<CreateFileArg> loadScript = new List<CreateFileArg>();
            for (int i = 0; i < sourceInfo.Count; i++)
            {
                ConvertExcelDataToServerSource(sourceInfo[i], outputPath, ref loadScript);
            }

            if (!CreateLoadServerSource(outputPath, ref loadScript))
            {
                ConversionToJsonFailed();
                return false;
            }

            return true;
        }


        public void ConvertExcelDataToServerSource(SourceInfo excelData, string outputPath, ref List<CreateFileArg> loadScript)
        {
            string fileName = excelData.table.Replace(" ", string.Empty);
            fileName = fileName.Replace("_", "/");

            bool checkDictionary = false;

            foreach (var item in excelData.colums)
            {
                if (item.key == true)
                { 
                    checkDictionary = true;
                    break;
                }
            }

            string directory = Path.GetDirectoryName(outputPath + "/" + fileName + ".cs");

            string sourceFileName = Path.GetFileName(outputPath + "/" + fileName + ".cs");


            string sourceName = sourceFileName.Substring(0, sourceFileName.IndexOf('.'));
            string sourceNoneNumber = Regex.Replace(sourceName, @"[\d]", "");
            sourceNoneNumber = $"{char.ToUpper(sourceNoneNumber[0])}{sourceNoneNumber.Substring(1)}";

            var valName = sourceNoneNumber.ToLower();
            var valNameNum = sourceName.ToLower();

            if (valNameNum.CompareTo(valName.ToLower()) != 0)
                return;

            var checkCreateSource = loadScript.Find(_ => _.className == sourceNoneNumber);

            if (checkCreateSource != null)
                return;

            loadScript.Add(new CreateFileArg() { className = sourceNoneNumber, jsonfile = sourceName });

            directory = directory.Replace("\\", "/");

            string bundleName = Path.GetDirectoryName(fileName + ".cs"); ;

            bundleName = $"scripts{(string.IsNullOrEmpty(bundleName) ? "" : "/" + bundleName)}";
            //bundleName = bundleName.ToLower();

            var source = checkDictionary ? ExcelToServerSourceDictionary(excelData, sourceName, sourceNoneNumber, bundleName) : ExcelToServerSourceList(excelData, sourceName, sourceNoneNumber, bundleName);

            WriteTextToFile(source, Path.Combine($"{outputPath}/Data/AutoJson", $"Script.{sourceNoneNumber}.cs"));

        }

        string ExcelToServerSourceDictionary(SourceInfo excelData, string sourceName, string sourceNoneNumber, string bundleName)
        {
            var dicSb = new StringBuilder();
            var keyExcelData = new List<ETJColumn>();

            var sb = new StringBuilder();
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("/*Auto Create File*/");
            sb.AppendLine($"/*Source : ExcelToJsonConvert*/");
            sb.AppendLine("/********************************************************/");

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("");
            sb.AppendLine($"    public class {sourceNoneNumber}Script");
            sb.AppendLine("    {");
            for (int k = 0; k < excelData.colums.Count; k++)
            {
                sb.AppendLine($"        public {excelData.colums[k].dataType} {excelData.colums[k].column};");
            }
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine($"    public partial class JsonDataManager");
            sb.AppendLine("    {");
            sb.AppendLine($"        ReaderWriterLockSlim _lock{sourceNoneNumber} = new ReaderWriterLockSlim();");
            sb.AppendLine("");

            var keyCount = excelData.colums.FindAll(_ => _.key).Count;

            dicSb.Append($"Dictionary<");
            if(keyCount > 1)
            {
                dicSb.Append($"(");
            }

            for (int k = 0; k < excelData.colums.Count; k++)
            {
                if (excelData.colums[k].key == true)
                {
                    keyExcelData.Add(excelData.colums[k]);
                    if(keyCount == 1)
                    {
                        dicSb.Append($"{ excelData.colums[k].dataType},");
                    }
                    else
                    {
                        dicSb.Append($"{excelData.colums[k].dataType}");
                        if(keyExcelData.Count != keyCount)
                        {
                            dicSb.Append(",");
                        }
                    }
                }
            }

            if(keyCount > 1)
            {
                dicSb.Append($"),");
            }
            dicSb.Append($"List<{sourceNoneNumber}Script>>");

            sb.AppendLine($"        {dicSb.ToString()} _dic{sourceNoneNumber}Script = new  {dicSb.ToString()}();");
            sb.AppendLine($"        private  {dicSb.ToString()} GetDic{sourceNoneNumber}Script {{ ");
            sb.AppendLine("            get { ");
            sb.AppendLine($"                _lock{sourceNoneNumber}.EnterReadLock();");
            sb.AppendLine("                try");
            sb.AppendLine("                {");
            sb.AppendLine($"                    return _dic{sourceNoneNumber}Script;");
            sb.AppendLine("                }");
            sb.AppendLine("                finally");
            sb.AppendLine("                {");
            sb.AppendLine($"                    _lock{sourceNoneNumber}.ExitReadLock();");
            sb.AppendLine("                }");
            sb.AppendLine("            } ");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine($"        public class {sourceNoneNumber}ScriptAll");
            sb.AppendLine("        {");
            sb.AppendLine($"            public {sourceNoneNumber}Script[] result;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            string strJsonTarget = "$\"{{ \\\"result\\\" : {text}}}\"";
            sb.AppendLine($"        bool LoadScript{sourceNoneNumber}()");
            sb.AppendLine("        {");
            sb.AppendLine("            string text = \"\";");
            sb.AppendLine($"            if (!File.Exists(\"./{bundleName}/{sourceName}.json\"))");
            sb.AppendLine("            {");
            sb.AppendLine($"                _log?.Error(\"Load Fail(File.Exists : {sourceName}.json)\");");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine("");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                text = File.ReadAllText(\"./{bundleName}/{sourceName}.json\");");
            sb.AppendLine($"                var resultScript = Newtonsoft.Json.JsonConvert.DeserializeObject<{sourceNoneNumber}ScriptAll>({strJsonTarget});");

            sb.AppendLine($"                {dicSb.ToString()} loadScript0 = new {dicSb.ToString()}();");

            sb.AppendLine("");
            sb.AppendLine("                for (int i = 0; i < resultScript.result.Length; i++)");
            sb.AppendLine("                {");
            sb.Append("                    if(loadScript0.TryGetValue(");

            if(keyCount > 1)
            {
                sb.Append("(");
            }
            for (int i = 0; i < keyExcelData.Count; i++)
            {
                sb.Append($"resultScript.result[i].{keyExcelData[i].column}");
                if (i == keyCount)
                {
                    sb.Append(")");
                }
                else if (i != keyCount - 1)
                {
                    sb.Append(",");
                }
            }
            if (keyCount > 1)
            {
                sb.Append(")");
            }
            sb.Append(", out var loadScript1)  == false)");
            sb.AppendLine("");
            sb.AppendLine("                    {");
            sb.Append("                        loadScript0.Add(");

            if(keyCount > 1)
            {
                sb.Append("(");
            }
            for(int i = 0; i< keyExcelData.Count; i++)
            {
                sb.Append($"resultScript.result[i].{keyExcelData[i].column}");
                if(i== keyCount)
                {
                    sb.Append(")");
                }
                else if(i != keyCount - 1)
                {
                    sb.Append(",");
                }
            }
            if(keyCount > 1)
            {
                sb.Append(")");
            }
            sb.Append($", new List<{sourceNoneNumber}Script>()");
            sb.Append("{resultScript.result[i]});");

            sb.AppendLine("");
            sb.AppendLine("                    }");
            sb.AppendLine("                    else");
            sb.AppendLine("                    {");
            sb.AppendLine("                        loadScript1.Add(resultScript.result[i]);");
            sb.AppendLine("                    }");

            sb.AppendLine("                }");
            sb.AppendLine("");
            sb.AppendLine($"                _lock{sourceNoneNumber}.EnterWriteLock();");
            sb.AppendLine("                try");
            sb.AppendLine("                {");
            sb.AppendLine($"                    _dic{sourceNoneNumber}Script = loadScript0;");
            sb.AppendLine("                }");
            sb.AppendLine("                finally");
            sb.AppendLine("                {");
            sb.AppendLine($"                    _lock{sourceNoneNumber}.ExitWriteLock();");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine($"                _log?.Error($\"Load Fail {sourceNoneNumber}(Exception : {{ex.Message}})\");");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            return sb.ToString();
        }

        string ExcelToServerSourceList(SourceInfo excelData, string sourceName, string sourceNoneNumber , string bundleName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("/*Auto Create File*/");
            sb.AppendLine($"/*Source : ExcelToJsonConvert*/");
            sb.AppendLine("/********************************************************/");

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("");
            sb.AppendLine($"    public class {sourceNoneNumber}Script");
            sb.AppendLine("    {");
            for (int k = 0; k < excelData.colums.Count; k++)
            {
                sb.AppendLine($"        public {excelData.colums[k].dataType} {excelData.colums[k].column};");
            }
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine($"    public partial class JsonDataManager");
            sb.AppendLine("    {");
            sb.AppendLine($"        ReaderWriterLockSlim _lock{sourceNoneNumber} = new ReaderWriterLockSlim();");
            sb.AppendLine("");
            sb.AppendLine($"        List<{sourceNoneNumber}Script> _li{sourceNoneNumber}Script = new List<{sourceNoneNumber}Script>();");
            sb.AppendLine($"        private List<{sourceNoneNumber}Script> Get{sourceNoneNumber}ScriptList {{ ");
            sb.AppendLine("            get { ");
            sb.AppendLine($"                _lock{sourceNoneNumber}.EnterReadLock();");
            sb.AppendLine("                try");
            sb.AppendLine("                {");
            sb.AppendLine($"                    return _li{sourceNoneNumber}Script;");
            sb.AppendLine("                }");
            sb.AppendLine("                finally");
            sb.AppendLine("                {");
            sb.AppendLine($"                    _lock{sourceNoneNumber}.ExitReadLock();");
            sb.AppendLine("                }");
            sb.AppendLine("            } ");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine($"        public class {sourceNoneNumber}ScriptAll");
            sb.AppendLine("        {");
            sb.AppendLine($"            public {sourceNoneNumber}Script[] result;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            string strJsonTarget = "$\"{{ \\\"result\\\" : {text}}}\"";
            sb.AppendLine($"        bool LoadScript{sourceNoneNumber}()");
            sb.AppendLine("        {");
            sb.AppendLine("            string text = \"\";");
            sb.AppendLine($"            if (!File.Exists(\"./{bundleName}/{sourceName}.json\"))");
            sb.AppendLine("            {");
            sb.AppendLine($"                _log?.Error(\"Load Fail(File.Exists : {sourceName}.json)\");");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine("");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine($"                text = File.ReadAllText(\"./{bundleName}/{sourceName}.json\");");
            sb.AppendLine($"                var resultScript = Newtonsoft.Json.JsonConvert.DeserializeObject<{sourceNoneNumber}ScriptAll>({strJsonTarget});");
            sb.AppendLine($"                List<{sourceNoneNumber}Script> loadScript = new List<{sourceNoneNumber}Script>();");
            sb.AppendLine("");
            sb.AppendLine("                for (int i = 0; i < resultScript.result.Length; i++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    loadScript.Add(resultScript.result[i]);");
            sb.AppendLine("                }");
            sb.AppendLine("");
            sb.AppendLine($"                _lock{sourceNoneNumber}.EnterWriteLock();");
            sb.AppendLine("                try");
            sb.AppendLine("                {");
            sb.AppendLine($"                    _li{sourceNoneNumber}Script = loadScript;");
            sb.AppendLine("                }");
            sb.AppendLine("                finally");
            sb.AppendLine("                {");
            sb.AppendLine($"                    _lock{sourceNoneNumber}.ExitWriteLock();");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine($"                _log?.Error($\"Load Fail {sourceNoneNumber}(Exception : {{ex.Message}})\");");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            return sb.ToString();
        }


        public bool CreateLoadServerSource(string _outputSource, ref List<CreateFileArg> _loadScript)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("/*Auto Create File*/");
            sb.AppendLine($"/*Source : ExcelToJsonConvert*/");
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("");
            sb.AppendLine("    public partial class JsonDataManager");
            sb.AppendLine("    {");
            sb.AppendLine("        readonly LogManagerBase _log;");
            sb.AppendLine("");
            sb.AppendLine("        public List<string> LoadScriptAll()");
            sb.AppendLine("        {");
            sb.AppendLine("            var ret = new List<string>();");
            for (int k = 0; k < _loadScript.Count; k++)
            {
                sb.AppendLine($"            if (false == LoadScript{_loadScript[k].className}())");
                sb.AppendLine($"                ret.Add(\"LoadScript{_loadScript[k].className}\");");
            }
            sb.AppendLine("            return ret;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            WriteTextToFile(sb.ToString(), Path.Combine($"{_outputSource}/Data/Managers", "JsonDataManager.cs"));
            return true;
        }

        public bool ConvertExcelDataToNodeSource(List<SourceInfo> sourceInfo, string outputPath, string[] allowedFiles)
        {
            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);

            List<CreateFileArg> loadScript = new List<CreateFileArg>();
            for (int i = 0; i < sourceInfo.Count; i++)
            {
                ConvertExcelDataToNodeSource(sourceInfo[i], outputPath, ref loadScript, allowedFiles);
            }

            //if (!CreateLoadNodeSource(outputPath, ref loadScript))
            //{
            //    ConversionToJsonFailed();
            //    return false;
            //}

            return true;
        }

        public void ConvertExcelDataToNodeSource(SourceInfo excelData, string outputPath, ref List<CreateFileArg> loadScript, string[] allowedFiles)
        {
            string fileName = excelData.table.Replace(" ", string.Empty);
            fileName = fileName.Replace("_", "/");



            string directory = Path.GetDirectoryName(outputPath + "/" + fileName + ".ts");

            string sourceFileName = Path.GetFileName(outputPath + "/" + fileName + ".ts");


            string sourceName = sourceFileName.Substring(0, sourceFileName.IndexOf('.'));
            string sourceNoneNumber = Regex.Replace(sourceName, @"[\d]", "");
            sourceNoneNumber = $"{char.ToUpper(sourceNoneNumber[0])}{sourceNoneNumber.Substring(1)}";

            if (0 < allowedFiles.Length && !allowedFiles.Contains(sourceNoneNumber.ToLower()))
                return;

            var valName = sourceNoneNumber.ToLower();
            var valNameNum = sourceName.ToLower();

            if (valNameNum.CompareTo(valName.ToLower()) != 0)
                return;

            var checkCreateSource = loadScript.Find(_ => _.className == sourceNoneNumber);

            if (checkCreateSource != null)
                return;

            loadScript.Add(new CreateFileArg() { className = sourceNoneNumber, jsonfile = sourceName });

            directory = directory.Replace("\\", "/");

            string bundleName = Path.GetDirectoryName(fileName + ".ts"); ;

            bundleName = $"scripts{(string.IsNullOrEmpty(bundleName) ? "" : "/" + bundleName)}";
            bundleName = bundleName.ToLower();

            var sb = new StringBuilder();
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("/*Auto Create File*/");
            sb.AppendLine($"/*Source : ExcelToJsonConvert*/");
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("import { Defines } from './../defines/index.js';");
            sb.AppendLine("import { readFileSync } from 'fs';");
            sb.AppendLine($"const jsonFile = readFileSync(new URL('./../../{bundleName}/{sourceName}.json', import.meta.url), 'utf8');");
            sb.AppendLine("const json = JSON.parse(jsonFile);");
            sb.AppendLine("");

            sb.AppendLine($"export interface {sourceNoneNumber}Script {{");
            for (int k = 0; k < excelData.colums.Count; k++)
            {
                var dataType = excelData.colums[k].dataType;
                switch (dataType)
                {
                    case "UInt64":
                        dataType = "number";
                        break;

                    case "Int64":
                        dataType = "number";
                        break;

                    case "Int32":
                        dataType = "number";
                        break;

                    case "int":
                        dataType = "number";
                        break;

                    case "float":
                        dataType = "number";
                        break;

                    case "double":
                        dataType = "number";
                        break;

                    case "DayOfWeek":
                        dataType = "number";
                        break;

                    case "bool":
                        dataType = "boolean";
                        break;

                    case "long":
                        dataType = "number";
                        break;
                }

                sb.AppendLine($"    {excelData.colums[k].column}: {dataType};");
            }
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine($"export function Get{sourceNoneNumber}Scripts (): {sourceNoneNumber}Script[] {{");
            sb.AppendLine($"    let result: {sourceNoneNumber}Script[] = [];");
            sb.AppendLine("    try {");
            sb.AppendLine("        for (var i = 0; i < json.length; i++) {");
            sb.AppendLine("            var dataArray = Object.entries(json[i]);");
            sb.AppendLine("            result.push({");
            for (int k = 0; k < excelData.colums.Count; k++)
            {
                var dataType = excelData.colums[k].dataType;
                switch (dataType)
                {
                    case "UInt64":
                        dataType = "number";
                        break;

                    case "Int64":
                        dataType = "number";
                        break;

                    case "Int32":
                        dataType = "number";
                        break;

                    case "int":
                        dataType = "number";
                        break;

                    case "float":
                        dataType = "number";
                        break;

                    case "double":
                        dataType = "number";
                        break;

                    case "DayOfWeek":
                        dataType = "number";
                        break;

                    case "bool":
                        dataType = "boolean";
                        break;

                    case "long":
                        dataType = "number";
                        break;
                }
                var value = $"dataArray[{k}][1]";

                if (dataType == "number")
                {
                    value = $"Number({value})";
                }
                else if (dataType == "DayOfWeek")
                {
                    value = $"Number({value})";
                }
                else if (dataType == "boolean")
                {
                    value = $"'true' == {value}.toLowerCase()";
                }
                else if (dataType.StartsWith("Defines"))
                {
                    value = $"Number({value})";
                }
                else
                {
                    value = $"{value} as string";
                }
                sb.AppendLine($"                {excelData.colums[k].column}: {value}{(k == excelData.colums.Count - 1 ? "" : ",")}");
            }
            sb.AppendLine("            })");
            sb.AppendLine("        }");
            sb.AppendLine("    } catch (error) {");
            sb.AppendLine($"        console.log(`Get{sourceNoneNumber}Scripts: ${{error}}`);");
            sb.AppendLine("    }");
            sb.AppendLine("    return result;");
            sb.AppendLine("}");

            WriteTextToFile(sb.ToString(), Path.Combine($"{outputPath}/src/data/autoJson", $"script.{sourceNoneNumber}.ts"));
        }

        public bool CreateLoadNodeSource(string _outputSource, ref List<CreateFileArg> _loadScript)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("/*Auto Create File*/");
            sb.AppendLine($"/*Source : ExcelToJsonConvert*/");
            sb.AppendLine("/********************************************************/");
            sb.AppendLine("");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using .Data.Managers;");
            sb.AppendLine("");
            sb.AppendLine("    public partial class JsonDataManager");
            sb.AppendLine("    {");
            sb.AppendLine("        readonly LogManager _log;");
            sb.AppendLine("");
            sb.AppendLine("        public List<string> LoadScriptAll()");
            sb.AppendLine("        {");
            sb.AppendLine("            var ret = new List<string>();");
            for (int k = 0; k < _loadScript.Count; k++)
            {
                sb.AppendLine($"            if (false == LoadScript{_loadScript[k].className}())");
                sb.AppendLine($"                ret.Add(\"LoadScript{_loadScript[k].className}\");");
            }
            sb.AppendLine("            return ret;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            WriteTextToFile(sb.ToString(), Path.Combine($"{_outputSource}/data/managers", "jsonDataManager.ts"));
            return true;
        }


        public void ConvertExcelDataToJson(DataSet excelData, string outputPath, List<string> filterList, ColumnDataCheckType checkType, ref List<string> failList, ref List<SourceInfo> sourceInfo, ref List<string> changefileName, bool fileCopy = false)
        {
            string spreadSheetJson = "";

            // Process Each SpreadSheet in the excel file
            for (int i = 0; i < excelData.Tables.Count; i++)
            {

                if (filterList.Any(_ => _.ToLower() == excelData.Tables[i].TableName.ToLower()))
                    continue;

                SourceInfo source = new SourceInfo();
                source.table = excelData.Tables[i].TableName;

                spreadSheetJson = GetSpreadSheetJson(excelData, excelData.Tables[i].TableName, checkType, out source.colums);
                if (String.IsNullOrEmpty(spreadSheetJson))
                {
                    outputLog("Excel To Json Converter: Failed to covert Spreadsheet '" + excelData.Tables[i].TableName + "' to json.");
                    failList.Add(excelData.Tables[i].TableName);
                }
                else
                {
                    sourceInfo.Add(source);
                    string fileName = excelData.Tables[i].TableName.Replace(" ", string.Empty);
                    fileName = fileName.Replace("_", "/");
                    string filePath = $"{outputPath}/{fileName}.json";
                    WriteTextToFile(spreadSheetJson, filePath);
                    outputLog("ㄴ " + filePath + " 생성");
                    if(fileCopy)
                        changefileName.Add($"{fileName}.json");
                }
            }
        }

        public void ConvertExcelFilesToJsonAndSource(string inputPath, string outputPath, string _outputSource, ColumnDataCheckType checkType, bool recentlyModifiedOnly = false)
        {
            List<string> excelFiles = GetExcelFileNamesInDirectory(inputPath);
            outputLog("총 " + excelFiles.Count.ToString() + " 개의 엑셀파일을 찾았습니다.");

            if (recentlyModifiedOnly)
            {
                excelFiles = RemoveUnmodifiedFilesFromProcessList(excelFiles, outputPath);

                if (excelFiles.Count == 0)
                {
                    outputLog("Excel To Json Converter: No updates to excel files since last conversion.");
                }
                else
                {
                    outputLog("Excel To Json Converter: " + excelFiles.Count.ToString() + " excel files updated/added since last conversion.");
                }
            }

            bool succeeded = true;
           
            List<CreateFileArg> loadScript = new List<CreateFileArg>();

            for (int i = 0; i < excelFiles.Count; i++)
            {
                if (!ConvertExcelFileToJson(excelFiles[i], outputPath, _outputSource, checkType, ref loadScript))
                {
                    succeeded = false;
                    break;
                }
            }

            if (!CreateLoadSource(_outputSource, ref loadScript))
            {
                succeeded = false;
            }

            if (succeeded)
            {
                ConversionToJsonSuccessfull();
            }
            else
            {
                ConversionToJsonFailed();
            }
        }

        /// <summary>
        /// Gets all the file names in the specified directory
        /// </summary>
        /// <returns>The excel file names in directory.</returns>
        /// <param name="directory">Directory.</param>
        private List<string> GetExcelFileNamesInDirectory(string directory)
        {
            //string[] directoryFiles = Directory.GetFiles(directory);
            //List<string> excelFiles = new List<string>();

            //// Regular expression to match against 2 excel file types (xls & xlsx), ignoring
            //// files with extension .meta and starting with ~$ (temp file created by excel when fie
            //Regex excelRegex = new Regex(@"^((?!(~\$)).*\.(xlsx|xls$))$");

            //for (int i = 0; i < directoryFiles.Length; i++)
            //{
            //    string fileName = directoryFiles[i].Substring(directoryFiles[i].LastIndexOf('/') + 1);

            //     if (excelRegex.IsMatch(fileName))
            //    {
            //        excelFiles.Add(directoryFiles[i]);
            //    }
            //}

            //return excelFiles;

            // Get all files in the directory
            string[] directoryFiles = Directory.GetFiles(directory);
            List<string> excelFiles = new List<string>();

            // Regular expression to match excel files (excluding temp files starting with ~$
            Regex excelRegex = new Regex(@"^(?!~\$).*\.(xlsx|xls)$");

            // Iterate through the directory files
            for (int i = 0; i < directoryFiles.Length; i++)
            {
                // Safely extract the file name using Path.GetFileName
                string fileName = Path.GetFileName(directoryFiles[i]);

                // Check if the file matches the excel file pattern and is not a temp file
                if (excelRegex.IsMatch(fileName))
                {
                    excelFiles.Add(directoryFiles[i]);
                }
            }

            return excelFiles;
        }

        public List<string> GetExcelFileNamesInDirectoryFilename(string directory)
        {
            List<string> excelFiles = new List<string>();
            try
            {
                string[] directoryFiles = Directory.GetFiles(directory);

                // Regular expression to match against 2 excel file types (xls & xlsx), ignoring
                // files with extension .meta and starting with ~$ (temp file created by excel when fie
                Regex excelRegex = new Regex(@"^((?!(~\$)).*\.(xlsx|xls$))$");

                for (int i = 0; i < directoryFiles.Length; i++)
                {
                    string fileName = directoryFiles[i].Substring(directoryFiles[i].LastIndexOf('/') + 1);

                    if (excelRegex.IsMatch(fileName))
                    {
                        excelFiles.Add(Path.GetFileName(fileName));
                    }
                }

            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
            return excelFiles;
        }
        /// <summary>
        /// Converts each sheet in the specified excel file to json and saves them in the output folder.
        /// The name of the processed json file will match the name of the excel sheet. Ignores
        /// sheets whose name begin with '~'. Also ignores columns whose names begin with '~'.
        /// </summary>
        /// <returns><c>true</c>, if excel file was successfully converted to json, <c>false</c> otherwise.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="outputPath">Output path.</param>
        /// 

        public bool CreateLoadSource(string _outputSource, ref List<CreateFileArg> _loadScript)
        {
            _loadScript.Sort((a, b) =>
            {
                return a.className.CompareTo(b.className);
            });
            string loadFile = Path.Combine(_outputSource, "DataManager.Loader.cs");

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Cysharp.Threading.Tasks;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UniRx;");

            sb.AppendLine("");

            sb.AppendLine("public partial class DataManager");
            sb.AppendLine("{");

            sb.AppendLine("    public int cntLoad = 0;");
            sb.AppendLine($"   public int maxCnt = {_loadScript.Count};");
            sb.AppendLine("    ");
            sb.AppendLine("    public void Complete()");
            sb.AppendLine("    {");
            sb.AppendLine("        cntLoad++;");
            sb.AppendLine("    }");
            sb.AppendLine("    ");
            sb.AppendLine("    public float LoadProcess()");
            sb.AppendLine("    {");
            sb.AppendLine("        return (float)cntLoad / (float)maxCnt;");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    public async UniTask LoadAllParser()");
            sb.AppendLine("    {");
            sb.AppendLine("        cntLoad = 0;");
            for (int k = 0; k < _loadScript.Count; k++)
            {
                sb.AppendLine($"        Clear{_loadScript[k].className}();");
            }
            sb.AppendLine("");

            for (int k = 0; k < _loadScript.Count; k++)
            {
                if (k % 10 == 0)
                    sb.AppendLine("    await UniTask.WhenAll(");
                sb.Append($"            LoadScript{_loadScript[k].className}()");

                if (k > 0 && (k + 1) % 10 == 0 || k + 1 == _loadScript.Count)
                {
                    sb.AppendLine(");");
                    continue;
                }

                if (k + 1 != _loadScript.Count)
                    sb.AppendLine(",");
            }

            sb.AppendLine("    }");
            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("    public static async UniTask ConvertBinary()");
            sb.AppendLine("    {");
            sb.AppendLine("    await UniTask.WhenAll(");
            for (int k = 0; k < _loadScript.Count; k++)
            {
                sb.Append($"            ConvertBinary{_loadScript[k].className}()");

                if (k + 1 != _loadScript.Count)
                    sb.AppendLine(",");
            }
            sb.AppendLine("    );");
            sb.AppendLine("    }");
            sb.AppendLine("#endif");
            sb.AppendLine("}");
            WriteTextToFile(sb.ToString(), loadFile);
            return true;
        }


        public class CreateFileArg
        {
            public string className;
            public string jsonfile;
        }


        public bool GetExcelData(string filePath, ref List<DataSet> excel)
        {
            outputLog(filePath + " 엑셀을 분석합니다.");
            DataSet excelData = GetExcelDataSet(filePath);

            if (excelData == null)
            {
                outputLog("Excel To Json Converter: Failed to process file: " + filePath);
                return false;
            }

            excel.Add(excelData);
            return true;
        }






        public bool ConvertExcelFileToJson(string filePath, string outputPath, string _outputSource, ColumnDataCheckType checkType, ref List<CreateFileArg> _loadScript)
        {
            outputLog(filePath + " 엑셀을 분석합니다.");
            DataSet excelData = GetExcelDataSet(filePath);

            if (excelData == null)
            {
                outputLog("Excel To Json Converter: Failed to process file: " + filePath);
                return false;
            }

            string spreadSheetJson = "";

            // Process Each SpreadSheet in the excel file
            for (int i = 0; i < excelData.Tables.Count; i++)
            {
                List<ETJColumn> colums;
                spreadSheetJson = GetSpreadSheetJson(excelData, excelData.Tables[i].TableName, checkType, out colums);
                if (String.IsNullOrEmpty(spreadSheetJson))
                {
                    outputLog("Excel To Json Converter: Failed to covert Spreadsheet '" + excelData.Tables[i].TableName + "' to json.");
                    return false;
                }
                else
                {
                    spreadSheetJson = spreadSheetJson.Replace("},", "}," + Environment.NewLine);

                    // The file name is the sheet name with spaces removed
                    string fileName = excelData.Tables[i].TableName.Replace(" ", string.Empty);
                    fileName = fileName.Replace("_", "/");
                    WriteTextToFile(spreadSheetJson, outputPath + "/" + fileName + ".json");
                    outputLog("ㄴ " + outputPath + "/" + fileName + ".json" + " 생성");

                    string directory = Path.GetDirectoryName(_outputSource + "/" + fileName + ".cs");
                    string sourceFileName = Path.GetFileName(_outputSource + "/" + fileName + ".cs");
                    string sourceName = sourceFileName.Substring(0, sourceFileName.IndexOf('.'));
                    string sourceNoneNumber = Regex.Replace(sourceName, @"[\d]", "");
                    sourceNoneNumber = $"{char.ToUpper(sourceNoneNumber[0])}{sourceNoneNumber.Substring(1)}";

                    var valName = sourceNoneNumber.ToLower();
                    var valNameNum = sourceName.ToLower();

                    _loadScript.Add(new CreateFileArg() { className = sourceNoneNumber, jsonfile = sourceName });
                    if (valNameNum.CompareTo(valName.ToLower()) != 0)
                        continue;

                    directory = directory.Replace("\\", "/");

                    string bundleName = Path.GetDirectoryName(fileName + ".cs"); ;

                    bundleName = $"scripts/{bundleName}";
                    bundleName = bundleName.ToLower();

                    var sb = new StringBuilder();
                    sb.AppendLine("using System;");
                    sb.AppendLine("using System.Collections;");
                    sb.AppendLine("using System.Collections.Generic;");
                    sb.AppendLine("using UnityEngine;");
                    sb.AppendLine("using UniRx;");

                    sb.AppendLine("");

                    sb.AppendLine("[Serializable]");
                    sb.AppendLine($"public class {sourceNoneNumber}Script");
                    sb.AppendLine("{");
                    for (int k = 0; k < colums.Count; k++)
                    {
                        sb.AppendLine($"    public {colums[k].dataType} {colums[k].column};");
                    }
                    sb.AppendLine("}");

                    sb.AppendLine("");
                    sourceFileName = Path.GetFileName($"{_outputSource}/{Regex.Replace(fileName, @"[\d]", "")}.cs");

                    sb.AppendLine($"public partial class DataManager");
                    sb.AppendLine("{");
                    sb.AppendLine($"    private List<{sourceNoneNumber}Script> list{sourceNoneNumber}Script = new List<{sourceNoneNumber}Script>();");
                    sb.AppendLine("");

                    sb.AppendLine("    [Serializable]");
                    sb.AppendLine($"    class {sourceNoneNumber}ScriptAll");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        public {sourceNoneNumber}Script[] result;");
                    sb.AppendLine("    }");
                    sb.AppendLine("");
                    sb.AppendLine($"    void Clear{sourceNoneNumber}()");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        list{sourceNoneNumber}Script.Clear();");
                    sb.AppendLine("    }");
                    sb.AppendLine("");
                    string strJsonTarget = "\"{ \\\"result\\\" : \" + _ + \"}\"";
                    sb.AppendLine($"    IObservable<string> LoadScript{sourceNoneNumber}()");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        var load = Managers.Resource.LoadScript(\"{bundleName}\", \"{sourceName}\"); ");
                    sb.AppendLine("        load.Subscribe(_ =>{");

                    sb.AppendLine($"            if (_ == \"\") ");
                    sb.AppendLine("            {");
                    sb.AppendLine($"                Debug.LogWarning(\"{sourceNoneNumber} is empty\");");
                    sb.AppendLine($"                return;");
                    sb.AppendLine("            }");

                    sb.AppendLine($"            var resultScript = JsonUtility.FromJson<{sourceNoneNumber}ScriptAll>({strJsonTarget}); ");
                    sb.AppendLine("            for (int i = 0; i < resultScript.result.Length; i++)");
                    sb.AppendLine($"                list{sourceNoneNumber}Script.Add(resultScript.result[i]);");
                    sb.AppendLine("        });");
                    sb.AppendLine("        return load;");
                    sb.AppendLine("    }");
                    sb.AppendLine("}");
                    sb.AppendLine("");

                    WriteTextToFile(sb.ToString(), Path.Combine(_outputSource, $"DataManager.{sourceNoneNumber}.cs"));


                }
            }
            return true;
        }

        /// <summary>
        /// Gets the excel data reader for the specified file.
        /// </summary>
        /// <returns>The excel data reader for file or null if file type is invalid.</returns>
        /// <param name="filePath">File path.</param>
        private IExcelDataReader GetExcelDataReaderForFile(string filePath)
        {
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

            // Create the excel data reader
            IExcelDataReader excelReader;

            // Create regular expressions to detect the type of excel file
            Regex xlsRegex = new Regex(@"^(.*\.(xls$))");
            Regex xlsxRegex = new Regex(@"^(.*\.(xlsx$))");

            // Read the excel file depending on it's type
            if (xlsRegex.IsMatch(filePath))
            {
                // Reading from a binary Excel file ('97-2003 format; *.xls)
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else if (xlsxRegex.IsMatch(filePath))
            {
                // Reading from a OpenXml Excel file (2007 format; *.xlsx)
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else
            {
                outputLog("Excel To Json Converter: Unexpected files type: " + filePath);
                stream.Close();
                return null;
            }

            // Close the stream

            stream.Close();
            return excelReader;
        }

        /// <summary>
        /// Gets the Excel data from the specified file
        /// </summary>
        /// <returns>The excel data set or null if file is invalid.</returns>
        /// <param name="filePath">File path.</param>
        private DataSet GetExcelDataSet(string filePath)
        {
            // Get the excel data reader with the excel data
            IExcelDataReader excelReader = GetExcelDataReaderForFile(filePath);

            if (excelReader == null)
            {
                return null;
            }

            // Get the data from the excel file
            DataSet data = new DataSet();

            do
            {

                // Get the DataTable from the current spreadsheet
                DataTable table = GetExcelSheetData(excelReader);

                if (table != null)
                {
                    // Add the table to the data set
                    data.Tables.Add(table);
                }
            }
            while (excelReader.NextResult()); // Read the next sheet

            excelReader.Close();
            return data;
        }

        /// <summary>
        /// Gets the Excel data from current spreadsheet
        /// </summary>
        /// <returns>The spreadsheet data table.</returns>
        /// <param name="excelReader">Excel Reader.</param>
        private DataTable GetExcelSheetData(IExcelDataReader excelReader)
        {
            if (excelReader == null)
            {
                outputLog("Excel To Json Converter: Excel Reader is null. Cannot read data");
                return null;
            }

            // Ignore sheets which start with ~
            Regex sheetNameRegex = new Regex(@"^~.*$");
            if (sheetNameRegex.IsMatch(excelReader.Name))
            {
                return null;
            }

            // Create the table with the spreadsheet name
            DataTable table = new DataTable(excelReader.Name);
            table.Clear();

            string value = "";
            bool rowIsEmpty;

            // Read the rows and columns
            while (excelReader.Read())
            {
                DataRow row = table.NewRow();
                rowIsEmpty = true;

                for (int i = 0; i < excelReader.FieldCount; i++)
                {
                    // If the column is null and this is the first row, skip
                    // to next iteration (do not want to include empty columns)
                    if (excelReader.IsDBNull(i) &&
                        (excelReader.Depth == 1 || i > table.Columns.Count - 1))
                    {
                        continue;
                    }

                    value = excelReader.IsDBNull(i) ? "" : excelReader.GetString(i);

                    // If this is the first row, add the values as columns
                    if (excelReader.Depth == 1)
                    {
                        table.Columns.Add(value);
                    }
                    else // Otherwise, add as rows
                    {
                        row[table.Columns[i]] = value;
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        rowIsEmpty = false;
                    }
                }

                // Add the row to the table if it was not column headers and 
                // the row was not empty
                if (excelReader.Depth != 1 && !rowIsEmpty)
                {
                    table.Rows.Add(row);
                }
            }

            return table;
        }

        /// <summary>
        /// Gets the json data for the specified spreadsheet in the specified DataSet
        /// </summary>
        /// <returns>The spread sheet json.</returns>
        /// <param name="excelDataSet">Excel data set.</param>
        /// <param name="sheetName">Sheet name.</param>
        private string GetSpreadSheetJson(DataSet excelDataSet, string sheetName, ColumnDataCheckType checkType, out List<ETJColumn> colums)
        {
            // Get the specified table
            DataTable dataTable = excelDataSet.Tables[sheetName];

            colums = new List<ETJColumn>();

            // Remove empty columns
            for (int col = dataTable.Columns.Count - 1; col >= 0; col--)
            {
                bool removeColumn = true;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (!row.IsNull(col))
                    {
                        removeColumn = false;
                        break;
                    }
                }

                if (removeColumn)
                {
                    dataTable.Columns.RemoveAt(col);
                }
            }

            // Remove columns which start with '~'
            Regex columnNameRegex = new Regex(@"^~.*$");
            Regex columnDicNameRegex = new Regex(@"^!.*$");
            List<DataColumn> removeList = new List<DataColumn>();

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (columnNameRegex.IsMatch(dataTable.Columns[i].ColumnName))
                {
                    removeList.Add(dataTable.Columns[i]);
                    continue;
                }

                string columnName = dataTable.Columns[i].ColumnName;


                int findSlush = dataTable.Columns[i].ColumnName.IndexOf('/');
                if (findSlush != -1)
                {
                    columnName = dataTable.Columns[i].ColumnName.Substring(0, findSlush);
                    string filter = dataTable.Columns[i].ColumnName.Substring(findSlush + 1).ToLower();

                    if (checkType == ColumnDataCheckType.Client && filter.CompareTo("server") == 0)
                    {
                        removeList.Add(dataTable.Columns[i]);
                        continue;
                    }
                }


                int findDot = columnName.IndexOf('.');
                string tempDataType;
                if (findDot == -1)
                {
                    tempDataType = "int";
                    dataTable.Columns[i].ColumnName = columnName;
                    //                    dataTable.Columns[i].ColumnName = dataTable.Columns[i].ColumnName;
                }
                else
                {
                    string filter = columnName.Substring(findDot + 1).ToLower();
                    switch (filter)
                    {
                        case "int": tempDataType = "int"; break;
                        case "float": tempDataType = "float"; break;
                        case "string": tempDataType = "string"; break;
                        case "double": tempDataType = "double"; break;
                        default: tempDataType = columnName.Substring(findDot + 1); break;
                    }
                    dataTable.Columns[i].ColumnName = columnName.Substring(0, findDot);

                    //if( filter == "Fix")
                    //{
                    //    removeList.Add(dataTable.Columns[i]);
                    //}
                }

                if (columnDicNameRegex.IsMatch(dataTable.Columns[i].ColumnName))
                {
                    colums.Add(new ETJColumn()
                    {
                        key = true,
                        dataType = tempDataType,
                        column = dataTable.Columns[i].ColumnName.Substring(1)
                    });

                    dataTable.Columns[i].ColumnName = dataTable.Columns[i].ColumnName.Substring(1);
                }
                else
                {
                    colums.Add(new ETJColumn()
                    {
                        key = false,
                        dataType = tempDataType,
                        column = dataTable.Columns[i].ColumnName
                    });
                }


                
            }

            for (int i = 0; i < removeList.Count; i++)
            {
                dataTable.Columns.Remove(removeList[i]);
            }

            // Serialze the data table to json string
            return Newtonsoft.Json.JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        /// <summary>
        /// Writes the specified text to the specified file, overwriting it.
        /// Creates file if it does not exist.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="filePath">File path.</param>
        private void WriteTextToFile(string text, string filePath)
        {
            FileInfo dic = new FileInfo(filePath);
            if (false == dic.Directory.Exists)
            {
                dic.Directory.Create();
            }
            System.IO.File.WriteAllText(filePath, text);
        }

        private void WriteTextToFile(byte[] convertBytes, string filePath)
        {
            FileInfo dic = new FileInfo(filePath);
            if (false == dic.Directory.Exists)
            {
                dic.Directory.Create();
            }
            System.IO.File.WriteAllBytes(filePath, convertBytes);
        }

        /// <summary>
        /// Removes files which have not been modified since they were last processed
        /// from the process list
        /// </summary>
        /// <param name="excelFiles">Excel files.</param>
        private List<string> RemoveUnmodifiedFilesFromProcessList(List<string> excelFiles, string outputDirectory)
        {
            List<string> sheetNames;
            bool removeFile = true;

            // ignore sheets whose name starts with '~'
            Regex sheetNameRegex = new Regex(@"^~.*$");
            Regex columnDicNameRegex = new Regex(@"^!.*$");
            for (int i = excelFiles.Count - 1; i >= 0; i--)
            {
                sheetNames = GetSheetNamesInFile(excelFiles[i]);
                removeFile = true;

                for (int j = 0; j < sheetNames.Count; j++)
                {
                    if (sheetNameRegex.IsMatch(sheetNames[j]))
                    {
                        continue;
                    }

                    string outputFile = outputDirectory + "/" + sheetNames[j] + ".json";
                    if (!File.Exists(outputFile) ||
                        File.GetLastWriteTimeUtc(excelFiles[i]) > File.GetLastWriteTimeUtc(outputFile))
                    {
                        removeFile = false;
                    }
                }

                if (removeFile)
                {
                    excelFiles.RemoveAt(i);
                }
            }

            return excelFiles;
        }

        /// <summary>
        /// Gets the list of sheet names in the specified excel file
        /// </summary>
        /// <returns>The sheet names in file.</returns>
        /// <param name="filePath">File path.</param>
        private List<string> GetSheetNamesInFile(string filePath)
        {
            List<string> sheetNames = new List<string>();

            // Get the excel data reader with the excel data
            IExcelDataReader excelReader = GetExcelDataReaderForFile(filePath);

            if (excelReader == null)
            {
                return sheetNames;
            }

            do
            {
                // Add the sheet name to the list
                sheetNames.Add(excelReader.Name);
            }
            while (excelReader.NextResult()); // Read the next sheet

            return sheetNames;
        }

        public void NoticeLog(string str)
        {
            outputLog(str);
        }

    }

}
