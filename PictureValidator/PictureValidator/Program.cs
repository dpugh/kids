using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PictureValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PictureValidator <source> <destination>");
            }
            else
            {
                List<FileData> sourceFiles = GatherFiles(args[0], false);
                List<FileData> destinationFiles = GatherFiles(args[1], true);

                for (int i = 0; (i < sourceFiles.Count); ++i)
                {
                    if ((i % 1000) == 0)
                        Console.WriteLine(i);

                    FileData file = sourceFiles[i];

                    FileData duplicate = FindDuplicate(file, destinationFiles, destinationFiles.Count);
                    if (duplicate == null)
                    {
                        Console.WriteLine("Unable to find\t{0}", file.Path);
                    }
                    else
                    {
                        Console.WriteLine("\t\t{0}\tis a duplicate", duplicate.Path);
                    }
                }
            }
        }

        private static FileData FindDuplicate(FileData file, List<FileData> destinationFiles, int count)
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
                        if (IsCopy(file, destinationFiles[middle]))
                        {
                            return destinationFiles[middle];
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        private static bool IsCopy(FileData file1, FileData file2)
        {
            const int blockSize = 4096;
            byte[] block1 = new byte[blockSize];
            byte[] block2 = new byte[blockSize];

            using (var stream1 = new FileStream(file1.Path, FileMode.Open, FileAccess.Read))
            {
                using (var stream2 = new FileStream(file2.Path, FileMode.Open, FileAccess.Read))
                {
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
            int c = files.Count;

            foreach (string file in Directory.EnumerateDirectories(path))
            {
                GatherFiles(file, isDestination, files);
            }

            foreach (string file in Directory.EnumerateFiles(path))
            {
                files.Add(new FileData(file, isDestination));
            }
            if (isDestination && (c == files.Count))
            {
                Console.WriteLine("{0} is empty", path);
            }
        }

        class FileData
        {
            public readonly string Path;
            public readonly long Length;
            public readonly bool IsDestination;

            public FileData(string path, bool isDestination)
            {
                this.Path = path;

                FileInfo fi = new System.IO.FileInfo(path);

                this.Length = fi.Length;

                this.IsDestination = isDestination;
            }
        }
    }
}
