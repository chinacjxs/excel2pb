using System.Collections.Generic;
using System.IO;

namespace excel2pb
{
    /// <summary>
    /// 缓存管理
    /// </summary>
    public class CacheRegistry : Singleton<CacheRegistry>
    {
        CacheRegistry() { }

        /// <summary>
        /// 缓存记录
        /// </summary>
        class CacheRecord
        {
            public string key;

            public string md5;

            public void Serialize(TextWriter writer)
            {
                writer.WriteLine(key + "," + md5);
            }

            public void Deserialize(TextReader reader)
            {
                string[] arr = reader.ReadLine().Split(',');
                key = arr[0];
                md5 = arr[1];
            }
        }

        const string kStrFileName = "__cached.txt";

        Dictionary<string, CacheRecord> records = new Dictionary<string, CacheRecord>();

        /// <summary>
        /// 初始化 加载缓存
        /// </summary>
        public void Initialize()
        {
            string path = Path.Combine(GlobalSetting.Instance.outputDir, kStrFileName);
            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    while (!reader.EndOfStream)
                    {
                        var item = new CacheRecord();
                        item.Deserialize(reader);
                        records.Add(item.key, item);
                    }
                }
            }
        }

        /// <summary>
        /// 检测是否是最新记录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        public bool IsNewest(string key,string md5)
        {
            if(records.TryGetValue(key,out CacheRecord item))
                return item.md5 != md5;
            return true;
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="md5"></param>
        public void ApplyNewest(string key,string md5)
        {
            if (records.TryGetValue(key, out CacheRecord item))
                item.md5 = md5;
            else
                records.Add(key, new CacheRecord() { key = key,md5 = md5 });
        }

        /// <summary>
        /// 清除无效记录
        /// </summary>
        /// <param name="newest"></param>
        /// <returns></returns>
        public IEnumerable<string> ClearUnused(IEnumerable<string> newest)
        {
            List<string> removal = new List<string>();
            HashSet<string> hashSet = new HashSet<string>(newest);
            foreach (var item in records.Keys)
            {
                if (!hashSet.Contains(item))
                    removal.Add(item);
            }

            foreach (var item in removal)
                records.Remove(item);

            return removal;
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        public void Save()
        {
            string path = Path.Combine(GlobalSetting.Instance.outputDir, kStrFileName);
            using (StreamWriter writer = File.CreateText(path))
            {
                foreach (var item in records.Values)
                    item.Serialize(writer);
            }
        }
    }
}
