using System;
using System.IO;

namespace sim8086
{
    public class MachineCode
    {
        private string filename;
        private byte[] data;
        private int length;
        private int position;
        
        public MachineCode(string filename)
        {
            this.filename = filename;
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Error: file {filename} not found.");
                return;
            }
            
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fs);

            length = (int)fs.Length;
            data = br.ReadBytes(length);
            position = 0;
            
            fs.Close();
        }
        public void SeekBackwards(int bytes = 1) => position = Math.Max(position - bytes, 0);
        public void SeekForwards(int bytes = 1) => position = Math.Min(position + bytes, length - 1);

        public byte CurrentByte => data[position];
        public byte GetNextByte()
        {
            var result = CurrentByte;
            SeekForwards();
            return result;
        }

        public short GetNextWord()
        {
            var byteLo = CurrentByte;
            SeekForwards();
            var byteHi = CurrentByte;
            SeekForwards();
            return (short)((byteHi << 8) + byteLo);
        }

        public short GetData(bool isSigned, bool isWord)
        {
            short result = 0;
            if (isSigned)
            {
                if (isWord)
                {
                    result = (sbyte)GetNextByte();
                }
            }
            else
            {
                result = isWord ? GetNextWord() : GetNextByte();
            }

            return result;
        }
        
        public string Print()
        {
            string result = $"; {filename} disassembly\n\n";
            result += "bits 16\n\n";

            var instruction = GetNextInstruction();
            while (instruction.opCode != Instruction.OpCode.Null) 
            {
                result += instruction.Print();
                instruction = GetNextInstruction();
            }

            return result;
        }

        private Instruction GetNextInstruction()
        {
            var result = new Instruction(this);
            return result;
        }
    }
}