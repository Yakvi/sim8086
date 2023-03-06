using System;

namespace sim8086
{
    public class Instruction
    {
        public enum OpCode
        {
            Null,
            mov,
        }

        public enum RegMem
        {
            None,
            al,
            cl,
            dl,
            bl,
            ah,
            ch,
            dh,
            bh,
            ax,
            cx,
            dx,
            bx,
            sp,
            bp,
            si,
            di
        }

        public enum Mode
        {
            MemorySimple = 0b00,
            Memory8 = 0b01,
            Memory16 = 0b10,
            Register = 0b11
        }

        public readonly OpCode opCode;
        public bool d, w, s, v, z;
        public RegMem destReg = RegMem.None;
        public RegMem sourceReg = RegMem.None;
        public string asm;

        public Instruction(MachineCode mc)
        {
            var byte1 = mc.GetNextByte();

            switch (byte1 >> 4)
            {
                case 0b1000:
                {
                    if (byte1 >> 2 == 0b100010)
                    {
                        opCode = OpCode.mov;
                        InitMovRegMem(mc, byte1);
                    }
                } break;
                case 0b1011:
                {
                    opCode = OpCode.mov;
                    InitMovImmediate(mc, byte1);
                } break;
            }

            if (opCode == OpCode.Null) return;
        }

        private void InitMovImmediate(MachineCode mc, byte byte1)
        {
            w = byte1.GetBit(3);
            var reg = (byte)(byte1 & 0b111);
            destReg = InterpretReg(reg);
            int value = w ? mc.GetNextWord() : mc.GetNextByte();
            asm = $"{opCode.ToString()} {destReg.ToString()}, {value}";
        }

        private void InitMovRegMem(MachineCode mc, byte byte1)
        {
            d = byte1.GetBit(1);
            w = byte1.GetBit(0);

            var byte2 = mc.GetNextByte();

            // TOOD(yakvi): Do we need to extract these three? 
            var mode = (Mode)(byte2 >> 6);
            var reg = (byte)((byte2 & 0b111000) >> 3);
            var rm = (byte)(byte2 & 0b111);

            if (mode == Mode.MemorySimple && rm == 0b100) // BP
                mode = Mode.Memory16;

            sourceReg = InterpretReg(reg);
            switch (mode)
            {
                case Mode.MemorySimple:
                {
                } break;
                case Mode.Memory8:
                {
                } break;
                case Mode.Memory16:
                {
                } break;
                case Mode.Register:
                {
                    destReg = InterpretReg(rm);
                    asm = $"{opCode.ToString()} {destReg.ToString()}, {sourceReg.ToString()}";
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private RegMem InterpretReg(byte id)
        {
            return id switch
            {
                0b000 => w ? RegMem.ax : RegMem.al,
                0b001 => w ? RegMem.cx : RegMem.cl,
                0b010 => w ? RegMem.dx : RegMem.dl,
                0b011 => w ? RegMem.bx : RegMem.bl,
                0b100 => w ? RegMem.sp : RegMem.ah,
                0b101 => w ? RegMem.bp : RegMem.ch,
                0b110 => w ? RegMem.si : RegMem.dh,
                0b111 => w ? RegMem.di : RegMem.bh,
                _ => RegMem.None
            };
        }

        public string Print()
        {
            return asm + "\n";
        }
    }
}