using System;
using System.IO;

namespace sim8086
{
    static class Program
    {
        static void Main(string[] args)
        {
            var filename = args[0];
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Error: file {filename} not found.");
                return;
            }

            var code = new MachineCode(filename);
            
            var asm = code.Print();
            Console.WriteLine(asm);

            // using (var sw = new StreamWriter("test.asm"))
            // {
            //     sw.Write(asm);
            // }
        }
    }
}
