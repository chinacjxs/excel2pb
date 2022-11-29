using System.Collections.Generic;
using System.IO;

namespace excel2pb
{
    /// <summary>
    /// 缓存注册
    /// </summary>
    public class CacheRegistry : Singleton<CacheRegistry>
    {
        /// <summary>
        /// 内部缓存条目
        /// </summary>
        class InternalCached
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

        Dictionary<string, InternalCached> CachedAll = new Dictionary<string, InternalCached>();

        const string kStrFileName = "__cached.txt";

        CacheRegistry() { }

        /// <summary>
        /// 初始化 加载本地缓存
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
                        var item = new InternalCached();
                        item.Deserialize(reader);
                        CachedAll.Add(item.key, item);
                    }
                }
            }
        }

        /// <summary>
        /// 检测是否比缓存数据新
        /// </summary>
        /// <param name="key"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        public bool IsNewest(string key,string md5)
        {
            if(CachedAll.TryGetValue(key,out InternalCached item))
                return item.md5 != md5;
            return true;
        }

        /// <summary>
        /// 更新最新的数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="md5"></param>
        public void ApplyNewest(string key,string md5)
        {
            if (CachedAll.TryGetValue(key, out InternalCached item))
                item.md5 = md5;
            else
                CachedAll.Add(key, new InternalCached() { key = key,md5 = md5 });
        }

        /// <summary>
        /// 根据最新的数据清除无效缓存
        /// </summary>
        /// <param name="newest"></param>
        /// <returns></returns>
        public IEnumerable<string> ClearUnused(IEnumerable<string> newest)
        {
            List<string> removal = new List<string>();
            HashSet<string> hashSet = new HashSet<string>(newest);
            foreach (var item in CachedAll.Keys)
            {
                if (!hashSet.Contains(item))
                    removal.Add(item);
            }

            foreach (var item in removal)
                CachedAll.Remove(item);

            return removal;
        }

        /// <summary>
        /// 保存缓存数据到文件中
        /// </summary>
        public void Save()
        {
            string path = Path.Combine(GlobalSetting.Instance.outputDir, kStrFileName);
            using (StreamWriter writer = File.CreateText(path))
            {
                foreach (var item in CachedAll.Values)
                    item.Serialize(writer);
            }
        }
    }
}
