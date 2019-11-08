using System;
using System.IO;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Console = System.Console;

namespace MMFDumper
{

    class Program
    {


        static void Main(string[] args)
        {
            // command lines arguments are 
            // MMFDumper filename [maxLengh=(0|length|default=256)]
            if (1 > args.Length || args.Length > 2)
            {
                DisplayHelp();
                return;
            }

            // if command line contains a call for help
            if (Regex.IsMatch(args[0]+args[1], @"^[\/\-\\](\?|help)"))
            {
                DisplayHelp();
                return;
            }

            // check and parse length parameter
            long maxLen = 256;
            if (args.Length == 2)
            {
                var match = Regex.Match(args[1], @"[\-\/\\]maxLength=(\d+)", RegexOptions.IgnoreCase);
                if (!match.Success || match.Groups.Count != 2)
                {
                    DisplayHelp();
                    return;
                }

                var value = match.Groups[1].Value;

                var bParsed = long.TryParse(value, out maxLen);
                if (!bParsed || maxLen < 0)
                {
                    DisplayHelp();
                    return;
                }
            }


            HexDump(args[0], maxLen);

#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("MMFDumper dumps content of a memory mapped file");
            Console.WriteLine();
            Console.WriteLine("usage: MmfDumper filename [/maxLength=(0|length)]");
            Console.WriteLine("         filename=name of file in MMF namespace");
            Console.WriteLine("         /maxLength=0: dump whole file contents ");
            Console.WriteLine("         /maxLength=1024: display first 1024 bytes (multiples of 16)");
            Console.WriteLine("         omitted maxLength parameter uses 256 bytes");
            Console.WriteLine();
        }

        private static void HexDump(string fileName, long maxDumpLength)
        {
            const int chunkSize = 16;

            try
            {
                // access MMF as simple as possible
                using (var mmf = MemoryMappedFile.OpenExisting(fileName))
                {
                    Console.WriteLine($"Dumping contents of file {fileName}");

                    using (var mmvStream = mmf.CreateViewStream())
                    {
                        var fileLength = mmvStream.Length;
                        Console.WriteLine($"File length = {fileLength}");

                        if (fileLength > maxDumpLength && maxDumpLength > 0)
                        {
                            Console.WriteLine($"Dumping first {maxDumpLength} bytes.");
                            fileLength = maxDumpLength;
                        }

                        // read in bytes chunks
                        for (long nLine = 0; nLine <= (fileLength - 1) / chunkSize; nLine++)
                        {
                            var bytes = new byte[chunkSize];
                            for (int readByte = 0;
                                readByte < chunkSize && nLine * chunkSize + readByte < fileLength;
                                readByte++)
                            {
                                var read = mmvStream.ReadByte();
                                if (read == -1)
                                {
                                    nLine = fileLength;
                                    break;
                                }

                                bytes[readByte] = (byte) read;
                            }

                            // format bytes as hex and as text
                            var hex = new StringBuilder();
                            var txt = new StringBuilder();
                            for (var splitters = 0; splitters < chunkSize / 4; ++splitters)
                            {
                                for (var nByte = 0; nByte < chunkSize / 4; ++nByte)
                                {
                                    var txtChar = (char) bytes[nByte + splitters * 4];

                                    // change non printable characters
                                    if (bytes[nByte + splitters * 4] < 32)
                                        txtChar = '_';

                                    hex.Append(string.Format("{0:x2} ", bytes[nByte + splitters * 4]));
                                    txt.Append(string.Format("{0,1}", txtChar));
                                }

                                hex.Append(" ");
                                txt.Append(" ");
                            }

                            var strOut = new StringBuilder();
                            strOut.Append(hex);
                            strOut.Append(txt);

                            // write the line to the console
                            Console.WriteLine(strOut.ToString());
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"File '{fileName}' was not found");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access to '{fileName}' was denied.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
