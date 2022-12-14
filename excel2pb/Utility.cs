using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace excel2pb
{
    public static class Utility
    {
        /// <summary>
        /// 小驼峰命名
        /// </summary>
        static readonly string CamelCasePattern = @"[a-z]+((\d)|([A-Z0-9][a-z0-9]+))*([A-Z])?";

        /// <summary>
        /// 获取指定目录下的文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string[] GetFiles(string path,Func<FileInfo,bool> filter)
        {
            List<string> ret = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                FileInfo[] fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (var file in fileInfos)
                {
                    if (filter == null || filter(file))
                        ret.Add(file.FullName);
                }
            }
            return ret.ToArray();
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Log(string fileName,string format = null, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString());
            if (!string.IsNullOrEmpty(fileName))
                sb.AppendFormat(" [{0}] ", fileName);
            else
                sb.Append(" ");
            sb.Append(string.Format(format == null ? string.Empty : format, args));
            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// 打印异常
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Exception(string fileName,string format = null,params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString());
            sb.Append(" ");
            if (!string.IsNullOrEmpty(fileName))
                sb.AppendFormat(" [{0}] ",fileName);
            sb.AppendFormat(string.Format(format == null ? string.Empty : format, args));
            throw new Exception(sb.ToString());
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileMd5(string filePath)
        {
            string hash = null;
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                hash = ComputeMd5Hash(fileStream);
            }
            return hash;
        }

        /// <summary>
        /// 计算MD5
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static string ComputeMd5Hash(Stream inputStream)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(inputStream);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// 验证命名合法性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsValidNaming(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            Regex regex = new Regex(CamelCasePattern);
            return regex.IsMatch(name);
        }

        /// <summary>
        /// 转JSON
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string DataTableToJsonStr(ExcelParseResult result)
        {
            StringBuilder sb = new StringBuilder();
            DataTable dt = result.DataTable;
            var fields = result.ProtoFields;

            if (dt != null)
            {
                sb.Append("{");
                if (dt.Rows.Count > 0)
                {
                    sb.Append("\"items\":[");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        sb.Append("{");
                        bool insertDot1 = false;
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            ProtoField field = fields[j];
                            var data = dt.Rows[i][j].ToString();

                            if (string.IsNullOrEmpty(data))
                                continue;

                            data =  Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(data, false).Replace("\"","\\\"");

                            if (field.Type == "bool")
                                data = data.ToLower();

                            if (insertDot1)
                                sb.Append(",");

                            if (field.Rule == "optional")
                            {
                                if (field.Type == "string")
                                    sb.AppendFormat("\"{0}\":\"{1}\"",field.Name,data);
                                else
                                    sb.AppendFormat("\"{0}\":{1}",field.Name,data);
                            }

                            if (field.Rule == "repeated")
                            {
                                var arr = Regex.Matches(data, @"[^,\s][^\,]*[^,\s]*");
                                sb.AppendFormat("\"{0}\":[", field.Name);
                                bool insertDot2 = false;
                                for (int k = 0; k < arr.Count; k++)
                                {
                                    string val = arr[k].Value;
                                    if (string.IsNullOrEmpty(val))
                                        continue;

                                    if (insertDot2 == true)
                                        sb.Append(",");

                                    if (field.Type == "string")
                                        sb.AppendFormat("\"{0}\"",val);
                                    else
                                        sb.Append(val);

                                    insertDot2 = true;
                                }
                                sb.Append("]");
                            }
                            insertDot1 = true;
                        }
                        sb.Append("}");

                        if (i != dt.Rows.Count - 1)
                            sb.Append(",");
                    }
                    sb.Append("]");
                }
                sb.Append("}");
                return sb.ToString();
            }
            else
                return null;
        }
    }
}
