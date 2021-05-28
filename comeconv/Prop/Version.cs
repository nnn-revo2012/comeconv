using System.Reflection;

namespace comeconv.Prop
{
    public class Ver
    {
        public static readonly string Version = "0.0.121";
        public static readonly string VerDate = "2021/05/31";

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
