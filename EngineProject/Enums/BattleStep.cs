using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Enums
{
    //Bot workflow steps
    public enum BattleStep
    {
        LookingForTarget = 0,
        Fighting = 1,
        Looting = 2,
        Escaping = 3,
        Stop = 4
    }
}
