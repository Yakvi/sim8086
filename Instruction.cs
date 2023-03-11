using System;

namespace sim8086
{
    public class Instruction
    {
        public enum OpCode
        {
            Null,
            add,
            cmp,
            mov,
            sub,
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

        public OpCode opCode;
        public bool d, w, s, v, z;
        public Register destReg = Register.None;
        public Register sourceReg = Register.None;
        public MemoryAddress memAddress;
        public string asm;
        public short data;

        public Instruction(MachineCode mc)
        {
            var byte1 = mc.GetNextByte();

            switch (byte1 >> 4)
            {
                case 0b0000: // add
                case 0b0010: // sub
                case 0b0011: // cmp
                {
                    opCode = (byte)((byte1 & 0b00111000) >> 3) switch
                    {
                        0b000 => OpCode.add,
                        0b101 => OpCode.sub,
                        0b111 => OpCode.cmp,
                        _ => OpCode.Null
                    };
                    switch (byte1 >> 2)
                    {
                        case 0b000000:
                        case 0b001010: 
                        case 0b001110: 
                            RegMemWithRegistry(mc, byte1);
                            break;
                        case 0b000001: 
                        case 0b001011: 
                        case 0b001111: 
                            DecodeAddSubCmpImmediateToAccumulator(mc, byte1);
                            break;
                    }
                } break;
                case 0b1000: // mov
                {
                    switch (byte1 >> 2)
                    {
                        // mov, reg/mem to/from register
                        case 0b100010:
                            opCode = OpCode.mov;
                            RegMemWithRegistry(mc, byte1);
                            break;
                        case 0b100000:
                        {
                            DecodeAddSubCmpImmediateRegMem(mc, byte1);
                        }break;
                    }
                } break;
                case 0b1011: // mov, immediate to register
                {
                    opCode = OpCode.mov;
                    DecodeMovImmediateReg(mc, byte1);
                } break;
                case 0b1100: // mov, immediate to reg/mem
                {
                    opCode = OpCode.mov;
                    DecodeMovImmediateRegMem(mc, byte1);
                } break;
                case 0b1010: // mov, memory to accumulator or vice versa
                {
                    opCode = OpCode.mov;
                    DecodeMovImmediateToAccumulator(mc, byte1);
                } break;
            }

            if (opCode == OpCode.Null) return;
        }

        #region Add Sub Cmp instructions
        
        private void DecodeAddSubCmpImmediateRegMem(MachineCode mc, byte byte1)
        {
            s = byte1.GetBit(1); // 0: no sign extension, 1: sign extend if w=1
            w = byte1.GetBit(0); // 0: byte,              1: word 

            var byte2 = mc.GetNextByte();
            opCode = (byte)((byte2 & 0b00111000) >> 3) switch
            {
                0b000 => OpCode.add,
                0b101 => OpCode.sub,
                0b111 => OpCode.cmp,
                _ => OpCode.Null
            };
            var mode = GetModFlag(byte2);
            var rm = GetRegMemFlag(byte2);

            if (mode == Mode.Register)
            {
                data = mc.GetData(s, w);
               
                destReg = InterpretReg(rm);
                
                asm = $"{opCode} {destReg}, {((w && s) ? (byte)data : data)}";
            }
            else
            {
                memAddress.Calculate(mc, mode, rm);
                
                data = mc.GetData(s, w);
                
                if (w)
                {
                    asm = $"{opCode} word {memAddress.printout}, {data}";
                }
                else
                {
                    asm = $"{opCode} byte {memAddress.printout}, {data}";
                }
            }
        }
        
        private void DecodeAddSubCmpImmediateToAccumulator(MachineCode mc, byte byte1)
        {
            w = byte1.GetBit(0); // 0: byte, 1: word 
            
            if (w)
            {
                data = mc.GetNextWord();
            }
            else
            {
                data = (short)(sbyte)mc.GetNextByte();
            }
            asm = $"{opCode} {(w ? "ax" : "al")}, {data}";

        }

        #endregion

        #region Mov Instruction
        
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
                data = mc.GetNextWord();
                asm = $"{opCode} {memAddress.printout}, word {data}";
            }
            else
            {
                data = mc.GetNextByte();
                asm = $"{opCode} {memAddress.printout}, byte {data}";
            }
        }
        
        private void DecodeMovImmediateToAccumulator(MachineCode mc, byte byte1)
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

        #endregion

        #region Common patterns

        private void RegMemWithRegistry(MachineCode mc, byte byte1)
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

        #region Utility

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

        #endregion
    }
}