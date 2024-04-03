using System;
using System.IO;
using InterpreterApp.Analysis;
using System.Diagnostics;
using InterpreterApp.src;

namespace CodeInterpreter
{
    internal static class Program
    {
        static void Main()
        {
         string codeFilePath = "D:\\BSCS-3\\BSCS3 - SECOND SEMESTER\\CS322 - PROGRAMMING LANGUAGES\\code.txt"; 
         
            string codeWithCarriageReturns = File.ReadAllText(codeFilePath);
            string code = codeWithCarriageReturns.Replace("\r", "");


            Console.WriteLine(code);


            try
            {
                // Execute the interpreter
                Interpreter program = new Interpreter(code);
                program.Execute();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.StackTrace);
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
