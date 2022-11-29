using System.Collections.Generic;
using System.IO;
using System.Text;

namespace excel2pb
{
    public class GlobalSetting : Singleton<GlobalSetting>
    {
        GlobalSetting() { }

        #region Base Directory

        public string inputDir;

        public string outputDir;

        public string protoDir;

        public string jsonDir;

        public string csharpDir;

        public string bytesDir;

        public string combiDir;

        public string pbDir;

        #endregion

        string mExcludeColumnStr;

        public bool mForceRebuild;

        public string mTablePrefix = "st_";

        public string mCombinedName;

        public string mPackageName = "XlsxProtos";

        public HashSet<string> mExcludeColumn = new HashSet<string>();

        public void Initialize(CommandLineArgumentParser parser)
        {
            var argument = parser.Get("--input-dir");
            inputDir = argument.Take();

            argument = parser.Get("--output-dir");
            outputDir = argument != null ? argument.Take() : inputDir;

            argument = parser.Get("--force-rebuild");
            mForceRebuild = argument != null ? true : mForceRebuild;

            argument = parser.Get("--proto-merge");
            mCombinedName = argument != null ? argument.Take() : mCombinedName;

            argument = parser.Get("--package-name");
            mPackageName = argument != null ? argument.Take() : mPackageName;

            argument = parser.Get("--column-exclude");
            mExcludeColumnStr = argument != null ? argument.Take() : mExcludeColumnStr;

            if (!string.IsNullOrEmpty(mExcludeColumnStr))
            {
                string[] arr = mExcludeColumnStr.Split('|');
                foreach (var item in arr)
                    mExcludeColumn.Add(item);
            }

            argument = parser.Get("--table-prefix");
            mTablePrefix = argument != null ? argument.Take() : mTablePrefix;

            if (parser.Has("--descriptor_set_out"))
                pbDir = Path.Combine(outputDir, "pb");

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            protoDir = Path.Combine(outputDir, "proto");
            if (!Directory.Exists(protoDir))
                Directory.CreateDirectory(protoDir);

            jsonDir = Path.Combine(outputDir, "json");
            if (!Directory.Exists(jsonDir))
                Directory.CreateDirectory(jsonDir);

            bytesDir = Path.Combine(outputDir, "bytes");
            if (!Directory.Exists(bytesDir))
                Directory.CreateDirectory(bytesDir);

            combiDir = Path.Combine(outputDir,"combi");
            if (!Directory.Exists(combiDir))
                Directory.CreateDirectory(combiDir);

            csharpDir = Path.Combine(outputDir, "csharp");
            if (!Directory.Exists(csharpDir))
                Directory.CreateDirectory(csharpDir);
        }

        /// <summary>
        /// 获取Protoc命令字符串
        /// </summary>
        /// <param name="protoPath"></param>
        /// <param name="protoFilePath"></param>
        /// <param name="languageDir"></param>
        /// <param name="pbFilePath"></param>
        /// <returns></returns>
        public string GetProtocStr(string protoPath,string protoFilePath,string languageDir,string pbFilePath = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  --proto_path {0} ",protoPath);

            if (!string.IsNullOrEmpty(languageDir))
            {
                if (!Directory.Exists(languageDir))
                    Directory.CreateDirectory(languageDir);

                sb.AppendFormat(" --csharp_out {0} ", languageDir);
            }

            if(!string.IsNullOrEmpty(pbFilePath))
            {
                string dir = Path.GetDirectoryName(pbFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                sb.AppendFormat(" --descriptor_set_out {0} ",pbFilePath);
            }

            sb.AppendFormat(" {0} ",protoFilePath);

            return sb.ToString();
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetTableName(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(mTablePrefix))
                fileName = mTablePrefix + fileName;
            return fileName.ToLower();
        }

        /// <summary>
        /// 获取完整的类名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetFullClassName(string tableName)
        {
            string className = tableName;
            if (!string.IsNullOrEmpty(mPackageName))
                className = mPackageName + "." + className;
            return className;
        }
    }
}