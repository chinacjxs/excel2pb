using System.Collections.Generic;
using System.IO;
using System;
using Google.Protobuf;
using System.Diagnostics;
using System.Linq;

namespace excel2pb
{
    public class AppMain : Singleton<AppMain>
    {
        AppMain() { }

        public void Run(string[] args)
        {
            Stopwatch sw1 = Stopwatch.StartNew();
            Stopwatch sw2 = new Stopwatch();

            CommandLineArgumentParser commandLine = CommandLineArgumentParser.Parse(args);
            GlobalSetting.Instance.Initialize(commandLine);
            CacheRegistry.Instance.Initialize();
            
            ProtoBuilder mTemporaryBuilder = new ProtoBuilder();
            ProtoBuilder mCombinedBuilder = new ProtoBuilder();

            ExcelProcessNode[] nodes = GetProcessNodeAll();
            int  processedCount = 0;
            bool needCombineProto = !string.IsNullOrEmpty(GlobalSetting.Instance.mCombinedName);

            foreach (var item in nodes)
            {
                string protoFilePath = Path.Combine(GlobalSetting.Instance.protoDir, item.tableName + ".proto");
                if (CacheRegistry.Instance.IsNewest(item.tableName, item.md5) ||
                    GlobalSetting.Instance.mForceRebuild)
                {
                    sw2.Restart();
                    processedCount = processedCount + 1;

                    Utility.Log(item.fileName, "开始解析文件数据");
                    ExcelParseResult excelParseResult = ExcelParser.Instance.Parse(item);
                    mTemporaryBuilder.Clear();
                    mTemporaryBuilder.NewMessage(item.tableName + "_item", excelParseResult.ProtoFields);
                    mTemporaryBuilder.NewMessage(item.tableName, new ProtoField()
                    {
                        Name = "items",
                        Rule = "repeated",
                        Type = item.tableName + "_item",
                    });
                    File.WriteAllText(protoFilePath, mTemporaryBuilder.GetProto());

                    string jsonFilePath = Path.Combine(GlobalSetting.Instance.jsonDir, item.tableName + ".json");
                    string byteFilePath = Path.Combine(GlobalSetting.Instance.bytesDir, item.tableName + ".bytes");
                    string csharpFilePath = Path.Combine(GlobalSetting.Instance.csharpDir, item.tableName);
                    string pbFilePath = null;
                    if(!string.IsNullOrEmpty(GlobalSetting.Instance.pbDir))
                        pbFilePath = Path.Combine(GlobalSetting.Instance.pbDir, item.tableName + ".pb");

                    string jsonStr = Utility.DataTableToJsonStr(excelParseResult);
                    File.WriteAllText(jsonFilePath, jsonStr);

                    string command = GlobalSetting.Instance.GetProtocStr(
                        GlobalSetting.Instance.protoDir, protoFilePath,csharpFilePath, pbFilePath);
                    ProtocRunner.Instance.Run(command);

                    Utility.Log(item.fileName, "数据解析完毕,耗时:{0}s", sw2.ElapsedMilliseconds / 1000.0f);

                    sw2.Restart();
                    Utility.Log(item.fileName, "开始序列化文件数据");
                    FileInfo[] csharpFilePaths = Utility.GetFiles(csharpFilePath, (p) =>
                    {
                        if (p.Extension.ToLower() == ".cs")
                            return true;
                        return false;
                    });

                    List<string> csharpCodes = new List<string>();
                    foreach (var csFilePath in csharpFilePaths)
                        csharpCodes.Add(File.ReadAllText(csFilePath.FullName));
                    DynamicClassLoader.Compile(item.tableName, csharpCodes);

                    string fullClassName = GlobalSetting.Instance.GetFullClassName(item.tableName);
                    var classType = DynamicClassLoader.GetExtractClass(item.tableName, fullClassName);
                    IMessage message = Activator.CreateInstance(classType) as IMessage;
                    IMessage obj = JsonParser.Default.Parse(File.ReadAllText(jsonFilePath), message.Descriptor);
                    using (FileStream fileStream = File.OpenWrite(byteFilePath))
                    {
                        obj.WriteTo(fileStream);
                    }

                    CacheRegistry.Instance.ApplyNewest(item.tableName, item.md5);
                    Utility.Log(item.fileName, "文件数据序列化完毕,耗时:{0}s\n", sw2.ElapsedMilliseconds / 1000.0f);
                }

                if (needCombineProto)
                    mCombinedBuilder.NewMessage(protoFilePath);
            }

            if (processedCount > 0 && needCombineProto)
            {
                string combinedProtoFilePath = Path.Combine(GlobalSetting.Instance.combiDir, GlobalSetting.Instance.mCombinedName + ".proto");
                string combinedPbFilePath = null;
                if (!string.IsNullOrEmpty(GlobalSetting.Instance.combiDir))
                    combinedPbFilePath = Path.Combine(GlobalSetting.Instance.combiDir, GlobalSetting.Instance.mCombinedName + ".pb");

                File.WriteAllText(combinedProtoFilePath, mCombinedBuilder.GetProto());

                string command = GlobalSetting.Instance.GetProtocStr(
                    GlobalSetting.Instance.combiDir, combinedProtoFilePath,GlobalSetting.Instance.combiDir,combinedPbFilePath);
                ProtocRunner.Instance.Run(command);
            }

            var removals =  CacheRegistry.Instance.ClearUnused(nodes.Select((p)=> p .tableName));     
            foreach (var item in removals)
            {
                string protoFilePath = Path.Combine(GlobalSetting.Instance.protoDir, item + ".proto");
                string jsonFilePath = Path.Combine(GlobalSetting.Instance.jsonDir, item + ".json");
                string byteFilePath = Path.Combine(GlobalSetting.Instance.bytesDir, item + ".bytes");
                string csharpFilePath = Path.Combine(GlobalSetting.Instance.csharpDir, item);

                File.Delete(protoFilePath);
                File.Delete(jsonFilePath);
                File.Delete(byteFilePath);
                Directory.Delete(csharpFilePath, true);
            }
            CacheRegistry.Instance.Save();

            Utility.Log(null, "成功 ! 本次共处理{0}个文件,耗时:{1}s", processedCount,sw1.ElapsedMilliseconds / 1000.0f);
        }

        ExcelProcessNode[] GetProcessNodeAll()
        {
            List<ExcelProcessNode> nodes = new List<ExcelProcessNode>();

            FileInfo[] paths = Utility.GetFiles(GlobalSetting.Instance.inputDir,(p)=> {
                if (p.Extension.ToLower() == ".xlsx" &&
                  (p.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    return true;
                return false;
            });

            HashSet<string> vs = new HashSet<string>();

            foreach (var path in paths)
            {
                string md5 = Utility.GetFileMd5(path.FullName);
                string fileName = Path.GetFileName(path.Name);
                string tableName = GlobalSetting.Instance.GetTableName(path.Name);

                if (vs.Contains(tableName))
                    Utility.Exception(fileName, "发生错误 检查到重复文件名");
                vs.Add(tableName);

                nodes.Add(new ExcelProcessNode()
                {
                    md5 = md5,
                    filePath = path.FullName,
                    fileName = fileName,
                    tableName = tableName,
                });
            }

            return nodes.ToArray();
        }
    }
}
