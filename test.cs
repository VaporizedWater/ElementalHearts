using System;
using System.Reflection;

class Program {
    static void Main() {
        Assembly tml = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\tModLoader\tModLoader.dll");
        Type modPlayer = tml.GetType("Terraria.ModLoader.ModPlayer");
        foreach(var method in modPlayer.GetMethods()) {
            if (method.Name.Contains("Craft") || method.Name.Contains("Material") || method.Name.Contains("Recipe")) {
                Console.WriteLine(method.Name + " " + method.ReturnType.Name);
            }
        }
    }
}
