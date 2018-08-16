using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Words
{
    public class FileCache
    {
        private readonly string _directory;
        public FileCache(string directory)
        {
            _directory = directory;
        }


        public IEnumerable<string> SearchKeys(string pattern) {

            return Directory.GetFiles(_directory, pattern);

        }
        public T Get<T>(string cacheKey)
        {
            var cacheFilePath = GetCacheFilePath(cacheKey);
            if (!File.Exists(cacheFilePath)) return default(T);
            var json = File.ReadAllText(cacheFilePath, Encoding.UTF8);
            var value = JsonConvert.DeserializeObject<T>(json);
            return value;
        }
        public IEnumerable<T> GetAll<T>(string[] cacheKeys)
        {
            var values = new List<T>();
            foreach (var cacheKey in cacheKeys)
            {
                var value = Get<T>(cacheKey);
                if (value == null) continue;
                values.Add(value);

            }
            return values;
        }

        public void Add<T>(string cacheKey, T value)
        {
            var cacheFilePath = GetCacheFilePath(cacheKey);
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            File.WriteAllText(cacheFilePath, json, Encoding.UTF8);
        }

        public void Clear(string cacheKey)
        {
            var cacheFilePath = GetCacheFilePath(cacheKey);
            if (File.Exists(cacheFilePath)) File.Delete(cacheFilePath);
        }
        public void Flush()
        {
            if (Directory.Exists(_directory))
            {
                var di = new DirectoryInfo(_directory);

                foreach (var file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (var dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
        }

        private string GetCacheFilePath(string cacheKey)
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
            return Path.Combine(_directory, cacheKey + ".json");
        }
    }
}
