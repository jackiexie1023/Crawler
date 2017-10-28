using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            ImgCrawler crawler = new ImgCrawler();
            crawler.Start();
            Console.Read();
        }
    }
}
