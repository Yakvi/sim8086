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
            
            

            code.Print();
        }
    }
}
