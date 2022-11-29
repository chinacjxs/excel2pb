using System;

namespace excel2pb
{
    /// <summary>
    /// 单例模板类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : class
    {
        static T __instance;

        static readonly object __lock = new object();

        public static T Instance
        {
            get
            {
                if (__instance == null)
                {
                    lock (__lock)
                    {
                        if(__instance == null)
                            __instance = Activator.CreateInstance(typeof(T),true) as T;
                    }
                }
                return __instance;
            }
        }
    }
}