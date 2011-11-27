using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util.Data.Peco;

namespace Test
{
    class Cls : PecoBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var obj = new Cls();
            obj.Id = 1;
            obj.Name = "aaaa";

            foreach (object fld in obj)
            {
                Console.WriteLine(fld.ToString());
            }

            Console.ReadKey();
        }
    }
}
