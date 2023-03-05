using System;
using System.Collections;
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
        
        public void Print()
        {
            Console.WriteLine($"; {filename} disassembly\n");
            Console.WriteLine("bits 16\n");

            var instruction = GetNextInstruction();
            while (instruction.opCode != Instruction.OpCode.Null) 
            {
                instruction.Print();
                instruction = GetNextInstruction();
            }
        }

        private Instruction GetNextInstruction()
        {
            var result = new Instruction(this);
            return result;
        }
    }
}