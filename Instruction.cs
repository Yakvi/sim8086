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
        
        public OpCode opCode;
        public bool d, w, s, v, z;
        public RegMem dest = RegMem.None;
        public RegMem source = RegMem.None;

        public Instruction(MachineCode mc)
        {
            var byte1 = mc.GetNextByte();
            
            opCode = InterpretOpCode(byte1);
            if (opCode == OpCode.Null) return;
            
            d = byte1.GetBit(6);
            w = byte1.GetBit(7);
            
            // TODO(yakvi): This might be wrong depending on opcode
            var byte2 = mc.GetNextByte();

            // TOOD(yakvi): Do we need to extract these three? 
            var mode = (Mode)(byte2 >> 6);
            var reg = (byte)((byte2 & 0b00111000) >> 3);
            var rm = (byte)(byte2 & 0b111);

            dest = InterpretRegMem(rm);
            if (mode == Mode.Register)
            {
                source = InterpretRegMem(reg);
            }
        }

        private OpCode InterpretOpCode(byte byte1)
        {
            var result = OpCode.Null;
            switch (byte1 >> 2)
            {
                case 0b100010:
                {
                    result = OpCode.mov;
                } break;
            }
            
            return result;
        }
        
        private RegMem InterpretRegMem(byte id)
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
            string result = "";
            switch (opCode)
            {
                case OpCode.mov:
                {
                    result += $"{opCode.ToString()} {dest.ToString()}, {source.ToString()}";
                } break;
            }

            result += "\n";
            return result;
        }
    }
}