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

    /// <summary>
    /// Proto Builder
    /// </summary>
    public class ProtoBuilder
    {
        const string kStrIndention = "    ";
        StringBuilder m_ProtoHeader = new StringBuilder();
        StringBuilder m_ProtoBody = new StringBuilder();

        public ProtoBuilder()
        {
            m_ProtoHeader.AppendLine("syntax = \"proto3\";");
            if (!string.IsNullOrEmpty(GlobalSetting.Instance.mPackageName))
                m_ProtoHeader.AppendFormat("package {0};\n",GlobalSetting.Instance.mPackageName);
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
                    m_ProtoBody.AppendLine(lines[i]);
            }
        }

        /// <summary>
        /// 创建一条新消息
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="field"></param>
        public void NewMessage(string messageName, ProtoField field)
        {
            NewMessage(messageName, new ProtoField[] { field });
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
        /// 新建行
        /// </summary>
        public void NewLine()
        {
            m_ProtoBody.AppendLine();
        }

        /// <summary>
        /// 获取完整协议
        /// </summary>
        /// <returns></returns>
        public string GetProto()
        {
            return string.Format("{0}\n{1}", m_ProtoHeader.ToString(), m_ProtoBody.ToString());
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <returns></returns>
        public string GetBody()
        {
            return m_ProtoBody.ToString();
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            m_ProtoBody.Clear();
        }
        
        /// <summary>
        /// 消息开始
        /// </summary>
        /// <param name="messageName"></param>
        void BeginMessage(string messageName)
        {
            m_ProtoBody.Append("\n");
            m_ProtoBody.Append("message ");
            m_ProtoBody.Append(messageName);
            m_ProtoBody.Append("\n{");
        }

        /// <summary>
        /// 消息结束
        /// </summary>
        void EndMessage()
        {
            m_ProtoBody.Append("\n}");
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
            m_ProtoBody.AppendFormat("\n{0}{1} {2} {3} = {4};", kStrIndention,rule,type,name,index);
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
    }
}