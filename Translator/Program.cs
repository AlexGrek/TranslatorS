using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the filename: ");
            var path = Console.ReadLine();
            var lexer = new LexAn();
            /*
            try
            {
                using (FileBuffer file = new FileBuffer(path))
                {

                }
            }
            catch (FileNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }*/

            Console.ReadKey();
        }
    }
}
