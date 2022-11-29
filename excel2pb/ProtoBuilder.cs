using System.Collections.Generic;
using System.IO;
using System.Text;

namespace excel2pb
{
    /// <summary>
    /// 协议字段
    /// </summary>
    public class ProtoField
    {
        /// <summary>
        /// 字段规则
        /// </summary>
        public string Rule;

        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type;

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name;
    }

    public class ProtoBuilder
    {
        readonly string headers = null;
        readonly string indention = "    ";
        readonly StringBuilder sb = new StringBuilder();

        public ProtoBuilder()
        {
            headers = "syntax = \"proto3\";\n";
            if (!string.IsNullOrEmpty(GlobalSetting.Instance.mPackageName))
                headers += string.Format("package {0};\n", GlobalSetting.Instance.mPackageName);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            sb.Clear();
        }
        
        /// <summary>
        /// 消息开始
        /// </summary>
        /// <param name="messageName"></param>
        void BeginMessage(string messageName)
        {
            sb.Append("message ");
            sb.Append(messageName);
            sb.Append("\n{");
        }

        /// <summary>
        /// 消息结束
        /// </summary>
        void EndMessage()
        {
            sb.Append("\n}\n");
        }

        /// <summary>
        /// 从文件中读取消息(忽略头)
        /// </summary>
        /// <param name="path"></param>
        public void NewMessage(string path)
        {
            bool isAvailable = false;
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!isAvailable && lines[i].StartsWith("message"))
                    isAvailable = true;

                if (isAvailable)
                    sb.AppendLine(lines[i]);
            }
        }

        /// <summary>
        /// 创建一条新消息
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="field"></param>
        public void NewMessage(string messageName, ProtoField field)
        {
            BeginMessage(messageName);
            NewField(1, field);
            EndMessage();
        }

        /// <summary>
        /// 创建一条新消息
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="fields"></param>
        public void NewMessage(string messageName, IEnumerable<ProtoField> fields)
        {
            BeginMessage(messageName);
            int idx = 1;
            foreach (var item in fields)
                NewField(idx++, item);
            EndMessage();
        }

        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="message"></param>
        public void Append(string message)
        {
            sb.Append(message);
        }

        /// <summary>
        /// 新建行
        /// </summary>
        public void NewLine()
        {
            sb.AppendLine();
        }

        /// <summary>
        /// 添加消息字段
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="rule"></param>
        void NewField(int index, string type,string name,string rule)
        {
            sb.AppendFormat("\n{0}{1} {2} {3} = {4};", indention,rule,type,name,index);
        }

        /// <summary>
        /// 添加消息字段
        /// </summary>
        /// <param name="index"></param>
        /// <param name="field"></param>
        void NewField(int index,ProtoField field)
        {
            NewField(index, field.Type, field.Name, field.Rule);
        }

        /// <summary>
        /// 获取完整协议
        /// </summary>
        /// <returns></returns>
        public string GetProto()
        {
            return string.Format("{0}\n{1}\n",headers,sb.ToString());
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            return sb.ToString();
        }
    }
}