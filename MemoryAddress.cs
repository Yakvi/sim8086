using System;
using System.Collections.Generic;

namespace sim8086
{
    public struct MemoryAddress
    {
        public bool isSet;
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
            var printoutRaw = ModePrintouts[rm];
            switch (mode)
            {
                case Instruction.Mode.MemorySimple:
                {
                    if (rm == 0b110) // bp here is always offset 16
                    {
                        CalculateDirect(mc, true);
                        return;
                    }
                    // Fallthrough otherwise
                } break;
                case Instruction.Mode.Memory8:
                {
                    var data = mc.GetNextByte();
                    var isNegative = data.GetBit(7);
                    var sign = isNegative ? "" : " +";
                    if (data != 0) printoutRaw += $"{sign} {(sbyte)data}";
                } break;
                case Instruction.Mode.Memory16:
                {
                    var data = mc.GetNextWord();
                    var isNegative = data.GetBit(15);
                    var sign = isNegative ? "" : " +";
                    if (data != 0) printoutRaw += $"{sign} {data}";
                } break;
                case Instruction.Mode.Register:
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            printout = $"[{printoutRaw}]";
            isSet = true;
        }

        public void CalculateDirect(MachineCode mc, bool isWide)
        {
            var printoutRaw = "";
            if (isWide)
            {
                var data = mc.GetNextWord();
                printoutRaw = $"{data}";
            }
            else
            {
                var data = mc.GetNextByte();
                printoutRaw = $"{data}";
            }
            printout = $"[{printoutRaw}]";
            isSet = true;
        }
    }
}