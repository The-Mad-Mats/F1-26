
using System.Collections.Generic;

namespace F1
{
    public class HighLightPerSeason
    {
        public string SeasonName { get; set; }
        public List<HighLight> Drivers { get; set; } 
    }
    public class HighLight
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public bool Human { get; set; }
    }
}
