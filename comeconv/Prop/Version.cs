﻿using System.Reflection;

namespace comeconv.Prop
{
    public class Ver
    {
        public static readonly string Version = "0.0.1.23";
        public static readonly string VerDate = "2023/08/30";

        public static string GetFullVersion()
        {
            return GetAssemblyName() + " Ver " + Version;
        }

        public static string GetAssemblyName()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            return assembly.Name;
        }
    }
}
