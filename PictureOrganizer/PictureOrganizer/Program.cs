using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PictureOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                WriteHelp();
            }
            else
            {
                try
                {
                    int arg = 0;
                    bool quick = false;

                    CheckForQuick(args, ref arg, ref quick);
                    string source = args[arg++];
                    CheckForQuick(args, ref arg, ref quick);
                    string destination = args[arg++];
                    CheckForQuick(args, ref arg, ref quick);
                    string trueDestination = (args.Length > arg) ? args[arg] : destination;

                    List<FileData> sourceFiles = GatherFiles(source, false);
                    List < FileData > destinationFiles = GatherFiles(destination, true);

                    for (int i = 0; (i < sourceFiles.Count); ++i)
                    {
                        if ((i % 1000) == 0)
                            Console.WriteLine(i);

                        FileData file = sourceFiles[i];

                        if ((!IsDuplicate(quick, file, destinationFiles, destinationFiles.Count)) &&
                            (!IsDuplicate(quick, file, sourceFiles, i)))
                        {
                            CopyFile(file, trueDestination);
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Failure: {0}", e.Message);
                    Console.WriteLine();
                    WriteHelp();
                }
            }
        }

        private static void CheckForQuick(string[] args, ref int arg, ref bool quick)
        {
            if ((args.Length > arg) && ((args[arg] == "-q") || (args[arg] == "-Q")))
            {
                quick = true;
                ++arg;
            }
        }

        private static void WriteHelp()
        {
            Console.WriteLine("Usage: PictureOrganizer [-q] <source> <destination> [<temporary destination>]");
            Console.WriteLine();
            Console.WriteLine("Every file under <source> will be compared with every file under <destination>.");
            Console.WriteLine("If a file under <source> does not have a duplicate, then it will be copied to");
            Console.WriteLine("<temporary destination> (if provided) or <destination>(if not). Files will be");
            Console.WriteLine("placed in a directory with the name yyyy-mm-dd (based on the date the picture");
            Console.WriteLine("was taken) and renamed, if needed, to prevent overwriting files.");
            Console.WriteLine();
            Console.WriteLine("if the -q flag is specified, then a fast comparison is done. Otherwise, the files");
            Console.WriteLine("are compared byte by byte.");
        }

        private static void CopyFile(FileData file, string destinationRoot)
        {
            const int DateTakenPropId = 0x0132;

            string extension = Path.GetExtension(file.Path).ToLowerInvariant();

            FileInfo fi = new System.IO.FileInfo(file.Path);

            DateTime dateTaken = fi.CreationTime < fi.LastWriteTime ? fi.CreationTime : fi.LastWriteTime;

            if ((extension == ".jpg") || (extension == ".png"))
            {
                using (Image img = Image.FromFile(file.Path))
                {
                    try
                    {
                        byte[] dt = img.GetPropertyItem(DateTakenPropId).Value;
                        string date = System.Text.Encoding.Default.GetString(dt);

                        // The last character may be NULL => remove it
                        if (date.Substring(date.Length - 1)[0] == '\0')
                            date = date.Substring(0, date.Length - 1);

                        int index = date.IndexOf(' ');
                        if (index > 0)
                            date = date.Substring(0, index);
                        date = date.Replace(":", "/");

                        dateTaken = DateTime.Parse(date);
                    }
                    catch
                    {
                    }
                }
            }

            string newPath = Path.Combine(destinationRoot, dateTaken.ToString("yyyy-MM-dd"));

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            string fileName = Path.GetFileName(file.Path);

            string newFile = Path.Combine(newPath, fileName);
            int offset = 0;
            while (File.Exists(newFile))
            {
                string newFileName = Path.GetFileNameWithoutExtension(fileName) + "-" + (offset++).ToString() + Path.GetExtension(fileName);
                newFile = Path.Combine(newPath, newFileName);
            }
            File.Copy(file.Path, newFile, true);

            Console.WriteLine("{0} -> {1}", file.Path, newFile);
        }

        private static bool IsDuplicate(bool quick, FileData file, List<FileData> destinationFiles, int count)
        {
            int low = 0;
            int high = count;
            while (low < high)
            {
                int middle = (low + high) / 2;

                if (file.Length < destinationFiles[middle].Length)
                {
                    high = middle;
                }
                else if (file.Length > destinationFiles[middle].Length)
                {
                    low = middle + 1;
                }
                else
                {
                    while ((--middle >= 0) && (destinationFiles[middle].Length == file.Length))
                    {
                    }

                    while ((++middle < count) && (file.Length == destinationFiles[middle].Length))
                    {
                        if (IsCopy(quick, file, destinationFiles[middle]))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        private static bool IsCopy(bool quick, FileData file1, FileData file2)
        {
            const int blockSize = 4096;
            byte[] block1 = new byte[blockSize];
            byte[] block2 = new byte[blockSize];

            using (var stream1 = new FileStream(file1.Path, FileMode.Open, FileAccess.Read))
            {
                using (var stream2 = new FileStream(file2.Path, FileMode.Open, FileAccess.Read))
                {
                    if (stream1.Length != stream2.Length)
                        return false;

                    while (true)
                    {
                        int count1 = stream1.Read(block1, 0, blockSize);
                        int count2 = stream2.Read(block2, 0, blockSize);

                        if (count1 != count2)
                            return false;

                        if (count1 == 0)
                            return true;

                        for (int i = 0; (i < count1); ++i)
                        {
                            if (block1[i] != block2[i])
                            return false;
                        }

                        if (quick)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        private static List<FileData> GatherFiles(string path, bool isDestination)
        {
            List<FileData> files = new List<FileData>();

            GatherFiles(path, isDestination, files);

            files.Sort((FileData left, FileData right) => { return left.Length.CompareTo(right.Length); });

            return files;
        }

        private static void GatherFiles(string path, bool isDestination, List<FileData> files)
        {
            foreach (string file in Directory.EnumerateDirectories(path))
            {
                GatherFiles(file, isDestination, files);
            }

            foreach (string file in Directory.EnumerateFiles(path))
            {
                if (Path.GetFileName(file) != "Thumbs.db")
                {
                    var fd = FileData.Create(file, isDestination);
                    if (fd != null)
                    {
                        files.Add(fd);
                    }
                }
            }
        }

        class FileData
        {
            public readonly string Path;
            public readonly long Length;
            public readonly bool IsDestination;

            public static FileData Create(string path, bool isDestination)
            {

                FileInfo fi = new System.IO.FileInfo(path);
                long length = fi.Length;

                if (length == 0)
                    return null;
                else
                    return new FileData(path, isDestination, length);
            }

            public FileData(string path, bool isDestination, long length)
            {
                this.Path = path;

                this.Length = length;

                this.IsDestination = isDestination;
            }
        }
    }
}
