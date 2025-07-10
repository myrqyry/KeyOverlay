using System;
using System.IO;

namespace KeyOverlay
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppWindow window;
            try
            {
                Console.WriteLine("Creating AppWindow...");
                window = new AppWindow();
                Console.WriteLine("AppWindow created successfully, starting run loop...");
                window.Run();
                Console.WriteLine("Run loop completed");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                Console.WriteLine($"Stack trace: {e.StackTrace}");
                using var sw = new StreamWriter("errorMessage.txt");
                sw.WriteLine(e.Message);
                sw.WriteLine(e.StackTrace);
                Console.WriteLine("Error details written to errorMessage.txt");
                return;
            }
            
            try
            {
                window.Dispose(); // Dispose of resources when done
                Console.WriteLine("Resources disposed successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during disposal: {e}");
            }
        }
    }
}