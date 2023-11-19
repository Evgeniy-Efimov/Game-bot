using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineProject.Infrastructure;
using EngineProject.Managers;

namespace EngineProject.Structures
{
    //Skill settings
    public class Skill
    {
        public string Name { get; set; }
        public string InputKey { get; set; }
        public DateTime LastUsedTimestamp { get; set; }
        public int SecondsDelay { get; set; }
        public bool UseImmediately { get; set; }
        public bool UseIfOverweight { get; set; }

        public Skill(string name, string inputKey, int secondsDelay, bool useImmediately, bool useIfOverweight)
        {
            Name = name;
            InputKey = inputKey;
            LastUsedTimestamp = useImmediately ? DateTime.Now.AddSeconds(-secondsDelay) : DateTime.Now;
            SecondsDelay = secondsDelay;
            UseImmediately = useImmediately;
            UseIfOverweight = useIfOverweight;
        }

        public void ResetTimestamps()
        {
            LastUsedTimestamp = UseImmediately ? DateTime.Now.AddSeconds(-SecondsDelay) : DateTime.Now;
        }
    }
}
