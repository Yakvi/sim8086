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
            
            var fs = new FileStream(filename, FileMode.Open);
            var br = new BinaryReader(fs);

            length = (int)fs.Length;
            data = br.ReadBytes(length);
            position = 0;
        }
        public void SeekBackwards(int bytes = 1) => position = Math.Max(position - bytes, 0);
        public void SeekForwards(int bytes = 0) => position = Math.Min(position + bytes, length);

        public void Print()
        {
            Console.WriteLine($"; {filename} disassembly");
            Console.WriteLine($"bits 16");
        }
    }
}