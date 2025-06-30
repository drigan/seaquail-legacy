using SeaQuailDiagramTool.Domain.Models;
using SeaQuailDiagramTool.Domain.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeaQuailDiagramTool.Infrastructure
{
    public class LocalFilePersistenceService<T> : IPersistenceService<T> where T : IHaveID
    {
        private readonly string dataDirectory;
        private readonly string fileName;
        private readonly object lockObject = new object();

        public LocalFilePersistenceService()
        {
            dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalData");
            fileName = $"{typeof(T).Name}.json";
            
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }

        private string FilePath => Path.Combine(dataDirectory, fileName);

        private List<T> LoadData()
        {
            lock (lockObject)
            {
                if (!File.Exists(FilePath))
                {
                    return new List<T>();
                }

                try
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
                }
                catch
                {
                    return new List<T>();
                }
            }
        }

        private void SaveData(List<T> data)
        {
            lock (lockObject)
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
        }

        public async Task<IEnumerable<T>> Filter(Expression<Func<T, bool>> criteria)
        {
            return await Task.Run(() =>
            {
                var data = LoadData();
                return data.AsQueryable().Where(criteria);
            });
        }

        public async Task<T> GetById(Guid id)
        {
            return await Task.Run(() =>
            {
                var data = LoadData();
                return data.FirstOrDefault(item => item.Id == id);
            });
        }

        public async Task<T> Save(T item)
        {
            return await Task.Run(() =>
            {
                var data = LoadData();
                
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                }

                var existingIndex = data.FindIndex(x => x.Id == item.Id);
                if (existingIndex >= 0)
                {
                    data[existingIndex] = item;
                }
                else
                {
                    data.Add(item);
                }

                SaveData(data);
                return item;
            });
        }

        public async Task Delete(Guid id)
        {
            await Task.Run(() =>
            {
                var data = LoadData();
                data.RemoveAll(item => item.Id == id);
                SaveData(data);
            });
        }
    }
} 