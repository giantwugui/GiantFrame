﻿using Giant.Frame;
using System;
using System.Threading;
using System.Reflection;
using Giant.Data;
using Giant.DB;

namespace Server.Test
{
    partial class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //所有异步回调到主线程
                SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);

                //Test_Mongo mongo_test = new Test_Mongo();
                //mongo_test.Init();
                //mongo_test.TestMongo();

                //Assembly assembly = typeof(DataBaseService).Assembly;

                //Test_MySql test_MySql = new Test_MySql();
                //test_MySql.Init();
                //test_MySql.Test();

                //DataManager.Instance.LoadData();

                //Test_Redis.Instance.Init();

                Test_ARSortMap.Test();

                Console.WriteLine("Server.Test start complate");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            while (true)
            {
                Thread.Sleep(1);

                OneThreadSynchronizationContext.Instance.Update();

                //Console.ReadLine();

                //TestInsert();
            }
        }
       
    }
}
