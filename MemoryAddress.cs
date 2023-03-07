using System;
using System.Collections.Generic;

namespace sim8086
{
    public struct MemoryAddress
    {
        public string printout;
        public int value;

        public static readonly string[] ModePrintouts = 
        {
            "bx + si", // 000
            "bx + di", // 001
            "bp + si", // 010
            "bp + di", // 011
            "si",      // 100
            "di",      // 101
            "bp",      // 110
            "bx"       // 111
        };

        public void Calculate(MachineCode mc, Instruction.Mode mode, byte rm)
        {
            printout = ModePrintouts[rm];
            switch (mode)
            {
                case Instruction.Mode.MemorySimple:
                {
                    if (rm == 0b110)
                    {
                        var data = mc.GetNextWord();
                    }
                } break;
                case Instruction.Mode.Memory8:
                {
                    var data = mc.GetNextByte();
                    if (data != 0) printout += $" + {data}";
                } break;
                case Instruction.Mode.Memory16:
                {
                    var data = mc.GetNextWord();
                    if (data != 0) printout += $" + {data}";
                } break;
                case Instruction.Mode.Register:
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}