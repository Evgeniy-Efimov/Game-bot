using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Infrastructure
{
    //Helps access list from parallel processes
    public class ThreadSafeQueue<T>
    {
        private List<T> list { get; set; }
        private int MaxCount { get; set; }

        public ThreadSafeQueue(int maxCount = 100) : base()
        {
            list = new List<T>();
            MaxCount = maxCount;
        }

        public T PullFirst()
        {
            lock (list)
            {
                if (!list.Any()) return default(T);
                var item = list.First();
                list.Remove(item);
                return item;
            }
        }

        public T GetFirst()
        {
            lock (list)
            {
                if (!list.Any()) return default(T);
                return list.First();
            }
        }

        public T GetLast()
        {
            lock (list)
            {
                if (!list.Any()) return default(T);
                return list.Last();
            }
        }

        public List<T> GetCopy()
        {
            lock (list)
            {
                return list.ToList();
            }
        }

        public T GetAt(int index)
        {
            lock (list)
            {
                return list[index];
            }
        }

        public void RemoveAt(int index)
        {
            lock (list)
            {
                list.RemoveAt(index);
            }
        }

        public void Add(T item)
        {
            lock (list)
            {
                list.Add(item);
                if (list.Count() > MaxCount) PullFirst();
            }
        }
    }
}
