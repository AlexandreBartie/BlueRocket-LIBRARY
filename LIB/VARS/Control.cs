﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace BlueRocket.LIBRARY.Lib.Vars
{
    public static class myControl
    {
        public static void TurnOnOff(bool prmON, Control prmObjectA, Control prmObjectB) => TurnOnOff(prmON, prmObjectA, prmObjectB, prmAtive: true);
        public static void TurnOnOff(bool prmON, Control prmObjectA, Control prmObjectB, bool prmAtive)
        {
            prmObjectA.Visible = prmON && prmAtive; prmObjectB.Visible = !prmON && prmAtive;
        }

    }
}
