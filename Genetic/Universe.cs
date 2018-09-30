using System;
using System.Collections.Generic;

namespace Genetic
{
    class Universe
    {
        //List of avaible worlds
        public List<World> Worlds_In_Universe;

        //Number of the session
        public int Session_Number;

        public Universe (int _Session, List<World> _Worlds)
        {
            Worlds_In_Universe = _Worlds;
            Session_Number = _Session;
        }

    }
}