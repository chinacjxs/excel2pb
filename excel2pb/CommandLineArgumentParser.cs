using System.Collections.Generic;
using System.Linq;

namespace excel2pb
{
    /// <summary>
    /// 命令行参数
    /// </summary>
    public class CommandLineArgument
    {
        readonly List<CommandLineArgument> _arguments;

        readonly string _argument;

        readonly int _index;

        public CommandLineArgument(List<CommandLineArgument> args,int index,string argument)
        {
            _arguments = args;
            _index = index;
            _argument = argument;
        }

        public CommandLineArgument Next
        {
            get
            {
                if (_index < _arguments.Count - 1)
                    return _arguments[_index + 1];
                return null;
            }
        }

        public CommandLineArgument Previous
        {
            get
            {
                if (_index > 0)
                    return _arguments[_index - 1];
                return null;
            }
        }

        public CommandLineArgument Take()
        {
            return Next;
        }

        public IEnumerable<CommandLineArgument> Take(int count)
        {
            var list = new List<CommandLineArgument>();
            var parent = this;
            for (var i = 0; i < count; i++)
            {
                var next = parent.Next;
                if (next == null)
                    break;

                list.Add(next);

                parent = next;
            }

            return list;
        }

        public static implicit operator string(CommandLineArgument argument)
        {
            if(argument != null)
                return argument._argument;
            return null;
        }

        public override string ToString()
        {
            return _argument;
        }
    }

    /// <summary>
    /// 命令行参数解析
    /// </summary>
    public class CommandLineArgumentParser
    {
        readonly List<CommandLineArgument> _arguments;

        public CommandLineArgumentParser(string[] args)
        {
            // 初始化
            _arguments = new List<CommandLineArgument>();
            for (int i = 0; i < args.Length; i++)
                _arguments.Add(new CommandLineArgument(_arguments, i, args[i]));
        }

        public static CommandLineArgumentParser Parse(string[] args)
        {
            return new CommandLineArgumentParser(args);
        }

        public CommandLineArgument Get(string argumentName)
        {
            return _arguments.FirstOrDefault(p => p == argumentName);
        }

        public bool Has(string argumentName)
        {
            return _arguments.Any(p => p == argumentName);
        }

        public IEnumerable<CommandLineArgument> GetEnumerator()
        {
            foreach (var temp in _arguments)
                yield return temp;
        }
    }
}