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

        public enum Register
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
        public Register destReg = Register.None;
        public Register sourceReg = Register.None;
        public MemoryAddress memAddress;
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
            d = byte1.GetBit(1); // 0: REG is source, 1: REG is dest
            w = byte1.GetBit(0); // 0: byte,          1: word 

            var byte2 = mc.GetNextByte();

            // TOOD(yakvi): Do we need to extract these three? 
            var mode = (Mode)(byte2 >> 6);
            var reg = (byte)((byte2 & 0b111000) >> 3);
            var rm = (byte)(byte2 & 0b111);
            
            switch (mode)
            {
                case Mode.Register:
                {
                    sourceReg = InterpretReg(d ? rm : reg);
                    destReg = InterpretReg(d ? reg : rm);
                    asm = $"{opCode} {destReg}, {sourceReg}";
                } break;
                case Mode.MemorySimple:
                case Mode.Memory8:
                case Mode.Memory16:
                {
                    if (d)
                    {
                        destReg = InterpretReg(reg);
                        memAddress.Calculate(mc, mode, rm);
                        asm = $"{opCode} {destReg}, [{memAddress.printout}]";
                    }
                    else
                    {
                        sourceReg = InterpretReg(reg);
                        memAddress.Calculate(mc, mode, rm);
                        asm = $"{opCode} [{memAddress.printout}], {sourceReg}";
                    }
                } break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private Register InterpretReg(byte id)
        {
            return id switch
            {
                0b000 => w ? Register.ax : Register.al,
                0b001 => w ? Register.cx : Register.cl,
                0b010 => w ? Register.dx : Register.dl,
                0b011 => w ? Register.bx : Register.bl,
                0b100 => w ? Register.sp : Register.ah,
                0b101 => w ? Register.bp : Register.ch,
                0b110 => w ? Register.si : Register.dh,
                0b111 => w ? Register.di : Register.bh,
                _ => Register.None
            };
        }

        public string Print()
        {
            return asm + "\n";
        }
    }
}