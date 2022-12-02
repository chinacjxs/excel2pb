using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace excel2pb
{
    /// <summary>
    /// 文件处理节点
    /// </summary>
    public class ExcelProcessNode
    {
        /// <summary>
        /// 完整文件路径
        /// </summary>
        public string filePath;

        /// <summary>
        /// 文件散列码
        /// </summary>
        public string md5;

        /// <summary>
        /// 文件名(不包含后缀)
        /// </summary>
        public string fileName;

        /// <summary>
        /// 导出的表名
        /// </summary>
        public string tableName;
    }

    /// <summary>
    /// 解析结果
    /// </summary>
    public class ExcelParseResult
    {
        /// <summary>
        /// 第一个有效列索引
        /// </summary>
        public int FirstCellNum = -1;

        /// <summary>
        /// 最后一个有效列索引
        /// </summary>
        public int LastCellNum = -1;

        /// <summary>
        /// 第一个有效行索引
        /// </summary>
        public int FirstRowNum = -1;

        /// <summary>
        /// 最有一个有效行索引
        /// </summary>
        public int LastRowNum = -1;

        /// <summary>
        /// 有效表格数据
        /// </summary>
        public DataTable DataTable = new DataTable();

        /// <summary>
        /// 协议字段信息
        /// </summary>
        public List<ProtoField> ProtoFields = new List<ProtoField>();

        /// <summary>
        /// 使用中的索引
        /// </summary>
        public HashSet<int> UsedIndexes = new HashSet<int>();
    }

    /// <summary>
    /// 解析器
    /// </summary>
    public class ExcelParser : Singleton<ExcelParser>
    {
        public ExcelParseResult Parse(ExcelProcessNode node)
        {
            ExcelParseResult result = new ExcelParseResult();
            using (FileStream fileStream = File.OpenRead(node.filePath))
            {
                XSSFWorkbook book = new XSSFWorkbook(fileStream);
                ISheet sheet = book.GetSheetAt(0);
                if (sheet == null)
                    Utility.Exception(node.fileName,"发生错误 未找到有效数据表");
                ProcessHead(sheet,node,result);
                ProcessBody(sheet,node,result);
            }
            return result;
        }

        void ProcessHead(ISheet sheet, ExcelProcessNode node ,ExcelParseResult result)
        {
            List<IRow> headers = new List<IRow>();
            for (int i = 0; i < Const.kHeaderLength; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null)
                    Utility.Exception(node.fileName, "<{0}> 发生错误 检查到空数据 行号 {1}", sheet.SheetName,i + 1);
                headers.Add(row);
            }

            result.LastCellNum = headers[0].LastCellNum + 1;
            string[] columnValues = new string[Const.kHeaderLength];
            for (int i = headers[0].FirstCellNum; i <= headers[0].LastCellNum; i++)
            {
                for (int j = 0; j < Const.kHeaderLength; j++)
                    columnValues[j] = GetCellString(headers[j].GetCell(i));

                if (string.IsNullOrEmpty(columnValues[0]))
                {
                    result.LastCellNum = i;
                    break;
                }

                if (columnValues[0].StartsWith(Const.kStrComment))
                    continue;

                if (GlobalSetting.Instance.mExcludeColumn.Contains(columnValues[0]))
                    continue;

                if (!Const.SpecifyingFieldRules.Contains(columnValues[1]))
                    Utility.Exception(node.fileName, "<{0}> 发生错误 检查到无效的字段规则'{1}' 行号 {2} 列号 {3}", sheet.SheetName,columnValues[1],2,i + 1);

                if (!Const.ScalarValueTypes.Contains(columnValues[2]))
                    Utility.Exception(node.fileName, "<{0}> 发生错误 检查到无效的数据类型'{1}' 行号 {2} 列号 {3}", sheet.SheetName,columnValues[2],3, i + 1);

                if (!Utility.IsLowerCamelCaseNaming(columnValues[3]))
                    Utility.Exception(node.fileName, "<{0}> 发生错误 检查到无效的字段名称'{1}' 行号 {2} 列号 {3}", sheet.SheetName,columnValues[3],4, i + 1);

                if (result.FirstCellNum == -1)
                    result.FirstCellNum = i;

                result.ProtoFields.Add(new ProtoField()
                {
                     Rule = columnValues[1],
                     Type = columnValues[2],
                     Name = columnValues[3],
                });

                result.UsedIndexes.Add(i);

                if(result.DataTable.Columns.Contains(columnValues[3]))
                    Utility.Exception(Path.GetFileName(node.fileName), "<{0}> 发生错误 检查到重复的字段名称 '{1}' 行号 {2} 列号 {3}", sheet.SheetName,columnValues[3],4 ,i + 1);

                result.DataTable.Columns.Add(columnValues[3]);
            }

            if(result.FirstCellNum == -1)
                Utility.Exception(node.fileName, "<{0}> 发生错误 未找到有效表头数据", sheet.SheetName);

            result.FirstRowNum = Const.kHeaderLength;
            result.LastRowNum = sheet.LastRowNum + 1;
            for (int i = Const.kHeaderLength; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);;
                if (row == null)
                {
                    result.LastRowNum = i;
                    break;
                }
                else
                {
                    string firstCellValue = GetCellString(row.GetCell(result.FirstCellNum));
                    if(string.IsNullOrEmpty(firstCellValue))
                    {
                        result.LastRowNum = i;
                        break;
                    }
                }
            }
        }

        void ProcessBody(ISheet sheet, ExcelProcessNode node,ExcelParseResult result)
        {
            for (int i = result.FirstRowNum; i < result.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                string firstCellValue = GetCellString(row.GetCell(result.FirstCellNum));
                if (firstCellValue.StartsWith(Const.kStrComment))
                    continue;

                DataRow dataRow = result.DataTable.NewRow();
                for (int j = result.FirstCellNum,index = 0; j < result.LastCellNum ; j++)
                {
                    if (!result.UsedIndexes.Contains(j)) continue;

                    var cell = row.GetCell(j);
                    string value = GetCellString(cell);
                    dataRow[index++] = value;
                }
                result.DataTable.Rows.Add(dataRow);
            }
        }

        string GetCellString(ICell cell)
        {
            if (cell == null)
                return null;

            cell.SetCellType(CellType.String);
            return cell.StringCellValue.Trim();
        }
    }
}
