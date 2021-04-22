using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

namespace testsqlit_ememory
{
    public class TestExpandoPerformance
    {
        public void Test()
        {
            var rowCount = 100000;
            var colCount = 1000;

            var progres = Process.GetCurrentProcess();

            var memOri = progres.WorkingSet64 / (1024d * 1024d);// memory usage in Mb;

            var st1 = Stopwatch.StartNew();
            var objList = TestList(rowCount, colCount);
            st1.Stop();

            progres = Process.GetCurrentProcess();

            var mem2 = progres.WorkingSet64 / (1024d * 1024d);// memory usage in Mb;

            var listObjectMemory = mem2 - memOri;

            objList.Clear();
            objList = null;
            Console.WriteLine($"List<List<object>>\t\tInit Times:\t{st1.ElapsedMilliseconds}\t\tms,\tMemoryUse:\t{listObjectMemory}\t\tMb.");


            var st2 = Stopwatch.StartNew();
            
            var expList = TestDataTable(rowCount, colCount);

            st2.Stop();

            progres = Process.GetCurrentProcess();

            var mem3 = progres.WorkingSet64 / (1024d * 1024d);// memory usage in Mb;

            var listexpandoMemory = mem3 - mem2;

            expList.Clear();
            expList = null;

            //Console.WriteLine($"List<ExpandoObject>\t\tInit Times:\t{st2.ElapsedMilliseconds}\t\tms,\tMemoryUse:\t{listObjectMemory}\t\tMb.");
            Console.WriteLine($"DataTable\t\tInit Times:\t{st2.ElapsedMilliseconds}\t\tms,\tMemoryUse:\t{listObjectMemory}\t\tMb.");

            Console.ReadLine();
        }



        List<List<object>> TestList(int rowCount,int colCount)
        {
            //测试初始化性能最高,1w 条1000列 700毫秒
            var list=new List<List<object>>();

            for (int i = 0; i < rowCount; i++)
            {
                var l=new List<object>();
                for (var m = 0; m < colCount; m++)
                {
                   l.Add(m);
                }

                list.Add(l);
            }

            return list;
        }

        List<ExpandoObject> TestExpando(int rowCount, int colCount)
        {
            //测试性能最差 1w 条1000列 75秒
            var list =new List<ExpandoObject>() ;

            for (int i = 0; i < rowCount; i++)
            {
                var l = new ExpandoObject() as IDictionary<string, object>;
                for (var m = 0; m < colCount; m++)
                {
                    //l[$"{m}"] = m;
                   // l.TryAdd(m.ToString(), m);
                    l[m.ToString()] = m;
                }

                list.Add(l as ExpandoObject);
            }

            return list;
        }

        DataTable TestDataTable(int rowCount, int colCount)
        {
            //测试性能,1w 条1000列, 7秒
            var list = new DataTable();

            for(int c=0;c < colCount; c++)
            {
                list.Columns.Add(c.ToString());
            }

            for (int i = 0; i < rowCount; i++)
            {
                var row = list.NewRow();

                for (var m = 0; m < colCount; m++)
                {
                    row[m.ToString()] = m;
                }
            }

            return list;
            //return list;
        }
    }
}
