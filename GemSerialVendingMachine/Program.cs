using Serial;
using System;

namespace GemSerialVendingMachine
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine();
                Console.Write("Gem Serial => ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(SerialMaker.MakeSerialString());
                Console.WriteLine();
                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
