﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Giant.Test
{
    public class TestManager
    {
        private static Dictionary<string, ITest> activedTestes = new Dictionary<string, ITest>();

        public static ITest GetTest(string identity)
        {
            activedTestes.TryGetValue(identity, out var test);
            return test;
        }

        public static void Execute(string[] param)
        {
            if (param.Length == 0 || string.IsNullOrEmpty(param[0]))
            {
                Console.WriteLine("Error Param！");
                return;
            }

            ITest test = GetTest(param[0]);
            if (param.Length == 1)
            {
                test?.DoTest();
            }
            else
            {
                test?.DoTest(param[1]);
            }
        }

        public static void LoadTestes(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ActivedTestAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                if (activedTestes.ContainsKey(attribute.Identity))
                {
                    Console.WriteLine($"重复的测试 Identity {attribute.Identity}");
                    continue;
                }

                if (Activator.CreateInstance(type) is ITest testObj)
                {
                    activedTestes.Add(attribute.Identity, testObj);
                }
            }

            if (activedTestes.Count <= 0)
            {
                Console.WriteLine("没有激活的测试，请检查！");
            }

            Note();
        }

        private static void Note()
        {
            Console.WriteLine($"Loaded Testes：Count {activedTestes.Count}");
            foreach (var kv in activedTestes)
            {
                Console.WriteLine($"{kv.Key} : {kv.Value.GetType().Name}");
            }
        }
    }
}