using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PictureCleanUp
{
    class Program
    {
        static void Main(string[] args)
        {
            var newDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var moves = new List<string>();
            foreach (var a in args)
            {
                CleanUp(a, newDirectories, moves);
            }

            foreach (var nd in newDirectories)
            {
                Console.WriteLine(nd);
            }
            foreach (var m in moves)
            {
                Console.WriteLine(m);
            }
        }

        static void CleanUp(string path, HashSet<string> newDirectories, List<string> moves)
        {
            if (Directory.Exists(path))
            {
                var d = Path.GetFileName(path);
                if ((d.Length > 4) && char.IsDigit(d[0]))
                {
                    var p = Path.GetDirectoryName(path);

                    string prefix = d.Substring(0, 4);
                    newDirectories.Add("mkdir " + p + "\\" + prefix);
                    moves.Add("move " + path + " " + p + "\\" + prefix + "\\" + d);
                }
                else
                {
                    foreach (var c in Directory.EnumerateDirectories(path))
                    {
                        CleanUp(c, newDirectories, moves);
                    }
                }
            }
        }
    }
}
