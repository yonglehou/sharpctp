using System;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            if(ctpdemo.DataApi!=null) ctpdemo.DataApi.Dispose();
            if(ctpdemo.TraderApi!=null) ctpdemo.TraderApi.Dispose();
        }
    }
}