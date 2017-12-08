using System;
using System.Diagnostics;
using System.Linq;
using ConsoleApp1;
using Dapper;

namespace testsqlite_memory
{
    class Program
    {
        static void Main(string[] args)
        {

            var sqlite = new SqliteBase();


            sqlite.TestStr();
            
 

            Console.WriteLine("Hello World!");

            Console.WriteLine("press enter key to load sqlite db from file to memory");

            var progres = Process.GetCurrentProcess();

            var mem=progres.WorkingSet64/(1024d*1024d);// memory usage in Mb;

            Console.WriteLine($"Memory Usage :\t\t{mem} Mb.");

            Console.ReadLine();

          
            sqlite.InitFromFile();

            Console.WriteLine("load from file to memory finished.");

            progres = Process.GetCurrentProcess();

            var mem2 = progres.WorkingSet64 / (1024d * 1024d);// memory usage in Mb;
            Console.WriteLine($"Memory Usage :\t\t{mem2} Mb.");

            var incresedMem = mem2 - mem;

            Console.WriteLine($"The file in memory usage is : \t\t{incresedMem} Mb");



            using (var db = sqlite.GetConnection())
            {
                var result = db.Query("SELECT	flight_leg_id,flight_phase FROM	test1 ORDER BY id limit 0,1");

                var ret = result.FirstOrDefault()?.flight_leg_id;

                Console.WriteLine("get flight leg id : " + ret?.ToString());
            }

            Console.ReadLine();
        }
    }
}
