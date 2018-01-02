using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGame
{
    class Attack
    {
        public string attacker;
        public string attackedPlanet;
        public string attackTime;
        public bool moon;
        public bool safe;

        public Attack(string _attacker, string _attackedPlanet, string _attackTime, bool _moonAttacked = false)
        {
            attacker = _attacker;
            attackedPlanet = _attackedPlanet;
            attackTime = _attackTime;
            moon = _moonAttacked;
            safe = false;
        }
    }
}
