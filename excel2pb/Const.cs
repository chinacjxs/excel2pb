using System.Collections.Generic;

namespace excel2pb
{
    public class Const
    {
        /// <summary>
        /// 表头长度 1字段作用域 2字段规则 3字段类型 4字段名称 5备注
        /// </summary>
        public const int kHeaderLength = 5;

        /// <summary>
        /// 注释
        /// </summary>
        public const string kStrComment = "#";

        /// <summary>
        /// 有效字段类型
        /// </summary>
        public static readonly HashSet<string> ScalarValueTypes = new HashSet<string>() {
            "double",
            "float",
            "int32",
            "int64",
            "uint32",
            "uint64",
            "sint32",
            "sint64",
            "fixed32",
            "fixed64",
            "sfixed32",
            "sfixed64",
            "bool",
            "string",
            //"bytes",
        };

        /// <summary>
        /// 有效的字段规则
        /// </summary>
        public static readonly HashSet<string> SpecifyingFieldRules = new HashSet<string>()
        {
            //"singular",
            "optional",
            "repeated",
        };
    }
}