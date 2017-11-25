using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGame
{
    class Expedition
    {
        private string _system;

        public Expedition(string system)
        {
            _system = system;
        }

        public string GetSystem()
        {
            return _system;
        }
    }
}
