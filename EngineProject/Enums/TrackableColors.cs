using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Enums
{
    //Colors for Bot workflow
    public enum TrackableColors
    {
        Grey = 0,
        White = 1,
        Red = 2,
        Green = 3,
        Unknown = 999
    }

    //Color converter
    public static class TrackableColor
    {
        public static TrackableColors GetTrackableColor(Color color)
        {
            if (color.R <= 150 && color.G <= 150 && color.B <= 150) return TrackableColors.Grey;
            else if (color.R > 150 && color.G > 150 && color.B > 150) return TrackableColors.White;
            else if (color.R > 175 && color.G < 120 && color.B < 120) return TrackableColors.Red;
            else if (color.R < 120 && color.G > 175 && color.B < 120) return TrackableColors.Green;
            return TrackableColors.Unknown;
        }
    }
}
