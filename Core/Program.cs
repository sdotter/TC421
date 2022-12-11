namespace TC421
{
    using System;

    internal class Program
    {
        static void Main(string[] args)
        {
            Main main = new Main();
            Thread t = new Thread(main.DoWork);

            t.IsBackground = true;
            t.Start();

            while (true)
            {
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.C && keyInfo.Modifiers == ConsoleModifiers.Control)
                {
                    main.KeepGoing = false;
                    break;
                }
            }
            t.Join();
        }
    }
}