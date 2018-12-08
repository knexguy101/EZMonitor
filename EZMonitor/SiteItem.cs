using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZMonitor
{
    public struct SiteItem
    {
        public string Name;
        public string Color;
        public bool SoldOut;

        public SiteItem(string name, string color, bool soldout)
        {
            this.Name = name;
            this.Color = color;
            this.SoldOut = soldout; 
        }
    }
}
