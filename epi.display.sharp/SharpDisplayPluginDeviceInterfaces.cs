using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Epi.Display.Sharp
{

    public interface IHasPower
    {
        void PowerOn();
        void PowerOff();
    }

    public interface IHasPowerToggle
    {
        void PowerToggle();
    }

    public interface IHasInput
    {
        void SelectInput(ushort input);
    }
}