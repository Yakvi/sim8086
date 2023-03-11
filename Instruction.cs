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
                case 0:
                {
                    
                } break;
                case 0b1000:
                {
                    if (byte1 >> 2 == 0b100010) // Reg/mem to/from register
                    {
                        opCode = OpCode.mov;
                        DecodeMovRegMem(mc, byte1);
                    }
                } break;
                case 0b1011: // Immediate to register
                {
                    opCode = OpCode.mov;
                    DecodeMovImmediateReg(mc, byte1);
                } break;
                case 0b1100: // Immediate to reg/mem
                {
                    opCode = OpCode.mov;
                    DecodeMovImmediateRegMem(mc, byte1);
                } break;
                case 0b1010: // Memory to accumulator or vice versa
                {
                    opCode = OpCode.mov;
                    DecodeMovMemAcc(mc, byte1);
                } break;
            }

            if (opCode == OpCode.Null) return;
        }

        #region Mov Instruction

        private void DecodeMovMemAcc(MachineCode mc, byte byte1)
        {
            d = byte1.GetBit(1); // 0: memory to accumulator 1: vice versa
            w = byte1.GetBit(0); // 0: byte,                 1: word 
            memAddress.CalculateDirect(mc, w);
            if (d)
            {
                asm = $"{opCode} {memAddress.printout}, ax";
            }
            else
            {
                asm = $"{opCode} ax, {memAddress.printout}";
            }
        }

        private void DecodeMovImmediateReg(MachineCode mc, byte byte1)
        {
            d = true;
            w = byte1.GetBit(3);
            var reg = (byte)(byte1 & 0b111);
            destReg = InterpretReg(reg);
            int value = w ? mc.GetNextWord() : mc.GetNextByte();
            asm = $"{opCode} {destReg}, {value}";
        }

        private void DecodeMovImmediateRegMem(MachineCode mc, byte byte1)
        {
            w = byte1.GetBit(0); // 0: byte,          1: word 
            var byte2 = mc.GetNextByte();

            var mode = GetModFlag(byte2);
            var rm = GetRegMemFlag(byte2);

            memAddress.Calculate(mc, mode, rm);

            if (w)
            {
                var data = mc.GetNextWord();
                asm = $"{opCode} {memAddress.printout}, word {data}";
            }
            else
            {
                var data = mc.GetNextByte();
                asm = $"{opCode} {memAddress.printout}, byte {data}";
            }
        }

        private void DecodeMovRegMem(MachineCode mc, byte byte1)
        {
            d = byte1.GetBit(1); // 0: REG is source, 1: REG is dest
            w = byte1.GetBit(0); // 0: byte,          1: word 

            var byte2 = mc.GetNextByte();

            var mode = GetModFlag(byte2);
            var reg = GetRegFlag(byte2);
            var rm = GetRegMemFlag(byte2);

            if (mode == Mode.Register)
            {
                sourceReg = InterpretReg(d ? rm : reg);
                destReg = InterpretReg(d ? reg : rm);
                asm = $"{opCode} {destReg}, {sourceReg}";
            }
            else
            {
                memAddress.Calculate(mc, mode, rm);

                if (d)
                {
                    destReg = InterpretReg(reg);
                    asm = $"{opCode} {destReg}, {memAddress.printout}";
                }
                else
                {
                    sourceReg = InterpretReg(reg);
                    asm = $"{opCode} {memAddress.printout}, {sourceReg}";
                }
            }
        }

        #endregion

        private static Mode GetModFlag(byte byte2) => (Mode)(byte2 >> 6);
        private static byte GetRegFlag(byte byte2) => (byte)((byte2 & 0b111000) >> 3);
        private static byte GetRegMemFlag(byte byte2) => (byte)(byte2 & 0b111);

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