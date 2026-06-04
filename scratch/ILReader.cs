using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

class Program
{
    static void Main(string[] args)
    {
        string dllPath = @"C:\Program Files (x86)\Steam\steamapps\common\tModLoader\tModLoader.dll";
        if (!File.Exists(dllPath))
        {
            Console.WriteLine("tModLoader.dll not found!");
            return;
        }

        Console.WriteLine("Loading assembly...");
        var assembly = AssemblyDefinition.ReadAssembly(dllPath);

        var main = assembly.MainModule.Types.FirstOrDefault(t => t.FullName == "Terraria.Main");
        if (main != null)
        {
            var drawInv = main.Methods.FirstOrDefault(m => m.Name == "DrawInventory");
            if (drawInv != null)
            {
                Console.WriteLine("=== ItemSlot.Draw calls in DrawInventory ===");
                int matchCount = 0;
                for (int i = 0; i < drawInv.Body.Instructions.Count; i++)
                {
                    var inst = drawInv.Body.Instructions[i];
                    if (inst.OpCode == OpCodes.Call && inst.Operand is MethodReference mr && mr.Name == "Draw" && mr.DeclaringType.Name == "ItemSlot")
                    {
                        matchCount++;
                        Console.WriteLine($"Match {matchCount} at index {i}:");
                        // Print 5 instructions before and after
                        int start = Math.Max(0, i - 5);
                        int end = Math.Min(drawInv.Body.Instructions.Count - 1, i + 5);
                        for (int j = start; j <= end; j++)
                        {
                            var target = drawInv.Body.Instructions[j];
                            string marker = (j == i) ? "=> " : "   ";
                            Console.WriteLine($"{marker}[{j}] {target.OpCode} {target.Operand}");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
