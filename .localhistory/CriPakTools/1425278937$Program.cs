using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CriPakTools
{
    class Program
    {
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("CriPakTools");
            Console.WriteLine("Based off Falo's code relased on Xentax forums (see readme.txt), modded by Nanashi3 from FuwaNovels.\nInsertion code by EsperKnight. Updated by GreenCat77.");

            if (args.Length == 0)
            {
                Console.WriteLine("CriPakTools Usage:");
                Console.WriteLine("CriPakTools.exe IN_FILE - Displays all contained chunks.");
                Console.WriteLine("CriPakTools.exe IN_FILE extract EXTRACT_ME [TO_HERE] - Extracts a file [to the specified file output].");
                Console.WriteLine("CriPakTools.exe IN_FILE extract ALL - Extracts all files.");
                Console.WriteLine("CriPakTools.exe IN_FILE replace REPLACE_ME REPLACE_WITH [[REPLACE_ME_2 REPLACE_WITH_2] [REPLACE_ME_3 REPLACE_WITH_3]] - Replaces all REPLACE_ME files with their respective REPLACE_WITH files. (if there is an odd number then the last REPLACE_ME is ignored)");
                Console.WriteLine("CriPakTools.exe IN_FILE index INDEX_FILE - Indexes the archive, saving the index file to the specified file output.");
                return;
            }

            string cpk_name = args[0];
            string selectedFunction = args[1].ToUpper();

            CPK cpk = new CPK(new Tools());
            cpk.ReadCPK(cpk_name);

            BinaryReader oldFile = new BinaryReader(File.OpenRead(cpk_name));


            if (selectedFunction == "EXTRACT" && args.Length >= 3)
            {
                string extractMe = args[2];

                List<FileEntry> selectedEntries = new List<FileEntry>();
                List<FileEntry> totalEntries = cpk.FileTable.OrderBy(x => x.FileOffset).ToList();

                for (int i = 0; i < totalEntries.Count; i++)
                {
                    bool doTheThing = false;

                    if (extractMe.ToUpper() == "ALL" && totalEntries[i].FileType == "FILE")
                    {
                        doTheThing = true;
                    }
                    else if (totalEntries[i].DirName != null && ((string)totalEntries[i].DirName + "/" + (string)totalEntries[i].FileName).ToUpper() == extractMe.ToUpper())
                    {
                        doTheThing = true;
                    }
                    else if (((string)totalEntries[i].FileName).ToUpper() == extractMe.ToUpper())
                    {
                        doTheThing = true;
                    }

                    if (doTheThing) selectedEntries.Add(totalEntries[i]);
                }

                //entries = (extractMe.ToUpper() == "ALL") ? cpk.FileTable.Where(x => x.FileType == "FILE").ToList() : cpk.FileTable.Where(x => ((x.DirName != null) ? x.DirName + "/" : "") + x.FileName.ToString().Trim().ToLower() == extractMe.Trim().ToLower()).ToList();



                if (selectedEntries.Count == 0)
                {
                    Console.WriteLine("Cannot find " + extractMe + ".");
                    throw new Exception("AAAGHGHHHHH");
                }

                for (int i = 0; i < selectedEntries.Count; i++)
                {
                    if (!String.IsNullOrEmpty((string)selectedEntries[i].DirName))
                    {
                        Directory.CreateDirectory(selectedEntries[i].DirName.ToString());
                    }

                    oldFile.BaseStream.Seek((long)selectedEntries[i].FileOffset, SeekOrigin.Begin);
                    string isComp = Encoding.ASCII.GetString(oldFile.ReadBytes(8));
                    oldFile.BaseStream.Seek((long)selectedEntries[i].FileOffset, SeekOrigin.Begin);

                    byte[] chunk = oldFile.ReadBytes(Int32.Parse(selectedEntries[i].FileSize.ToString()));
                    if (isComp == "CRILAYLA")
                    {
                        chunk = cpk.DecompressCRILAYLA(chunk, Int32.Parse(selectedEntries[i].ExtractSize.ToString()));
                    }

                    File.WriteAllBytes((selectedEntries.Count == 1 && args.Length >= 4) ? args[3] : ((selectedEntries[i].DirName != null) ? selectedEntries[i].DirName + "/" : "") + (selectedEntries[i].FileName.ToString()), chunk);
                }
            }
            else if (args.Length >= 4 && selectedFunction == "REPLACE")
            {
                //if (args.Length < 3)
                //{
                //    Console.WriteLine("Usage for insertion CriPakTools IN_CPK REPLACE_THIS REPLACE_WITH [OUT_CPK]");
                //    return;
                //}

                //string ins_name = args[1];
                //string replace_with = args[2];

                List<Tuple<string, string>> replaceNames = new List<Tuple<string, string>>();

                for (int i = 2; i < args.Length; i += 2)
                {
                    if ((i + 1) < args.Length)
                    {
                        replaceNames.Add(new Tuple<string, string>(args[i], args[i + 1]));
                    }
                }

                string outputName = cpk_name + ".tmp";
                //if (args.Length >= 4)
                //{
                //    outputName = args[3];
                //}

                BinaryWriter newCPK = new BinaryWriter(File.OpenWrite(outputName));

                //List<FileEntry> selectedEntries = new List<FileEntry>();
                List<FileEntry> totalEntries = cpk.FileTable.OrderBy(x => x.FileOffset).ToList();

                //for (int i = 0; i < totalEntries.Count; i++)
                //{
                //    bool doTheThing = false;

                //    for (int j = 0; j < replaceNames.Count; j++)
                //    {
                //        if (totalEntries[i].DirName != null && ((string)totalEntries[i].DirName + "/" + (string)totalEntries[i].FileName).ToUpper() == replaceNames[j].Item2.ToUpper())
                //        {
                //            doTheThing = true;
                //        }
                //        else if (((string)totalEntries[i].FileName).ToUpper() == replaceNames[j].Item2)
                //        {
                //            doTheThing = true;
                //        }
                //    }
                    

                //    if (doTheThing) selectedEntries.Add(totalEntries[i]);
                //}

                int consoleLine = Console.CursorTop;
                int numReplaced = 0;

                for (int i = 0; i < totalEntries.Count; i++)
                {

                    if (i > 0)
                    {
                        int numLnClr = Console.CursorTop - consoleLine;
                        Console.SetCursorPosition(0, consoleLine);

                        Console.WriteLine(new string(' ', Console.WindowWidth * numLnClr));

                        Console.SetCursorPosition(0, consoleLine);
                    }

                    Console.WriteLine("Updating file "+ (i + 1) + " of " + totalEntries.Count + ": \"" + ((totalEntries[i].DirName != null) ? (string)totalEntries[i].DirName + "/" : "") + totalEntries[i].FileName.ToString().Trim() + "\"...");
                    Console.WriteLine(new string(' ', Console.WindowWidth));  
                    

                    if (totalEntries[i].FileType != "CONTENT")
                    {
                        totalEntries[i].FileOffset = (ulong)newCPK.BaseStream.Position;

                        bool replacedFile = false;

                        for (int j = 0; j < replaceNames.Count; j++)
                        {
                            bool doTheThing = false;

                            if (totalEntries[i].DirName != null && ((string)totalEntries[i].DirName + "/" + (string)totalEntries[i].FileName).ToUpper() == replaceNames[j].Item1.ToUpper())
                            {
                                doTheThing = true;
                            }
                            else if (((string)totalEntries[i].FileName).ToUpper() == replaceNames[j].Item1.ToUpper())
                            {
                                doTheThing = true;
                            }

                            if (doTheThing)
                            {
                                byte[] newbie = File.ReadAllBytes(replaceNames[j].Item2);
                                totalEntries[i].FileSize = Convert.ChangeType(newbie.Length, totalEntries[i].FileSizeType);
                                totalEntries[i].ExtractSize = Convert.ChangeType(newbie.Length, totalEntries[i].FileSizeType);
                                cpk.UpdateFileEntry(totalEntries[i]);
                                newCPK.Write(newbie);

                                replacedFile = true;                                

                                ConsoleColor prevColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Blue;

                                int numLnClr = Console.CursorTop - consoleLine;
                                Console.SetCursorPosition(0, consoleLine);

                                Console.WriteLine(new string(' ', Console.WindowWidth * numLnClr));

                                Console.SetCursorPosition(0, consoleLine);

                                Console.WriteLine("Replaced file " + (++numReplaced) + " of " + replaceNames.Count + ": \"" + ((totalEntries[i].DirName != null) ? (string)totalEntries[i].DirName + "/" : "") + totalEntries[i].FileName.ToString().Trim() + "\".");

                                consoleLine = Console.CursorTop;

                                Console.ForegroundColor = prevColor;

                                break;
                            }
                        }

                        if (!replacedFile)
                        {
                            cpk.UpdateFileEntry(totalEntries[i]);

                            oldFile.BaseStream.Seek((long)totalEntries[i].FileOffset, SeekOrigin.Begin);
                            byte[] chunk = oldFile.ReadBytes(Int32.Parse(totalEntries[i].FileSize.ToString()));
                            newCPK.Write(chunk);
                        }

                        if ((newCPK.BaseStream.Position % 0x800) > 0)
                        {
                            long cur_pos = newCPK.BaseStream.Position;
                            for (int j = 0; j < (0x800 - (cur_pos % 0x800)); j++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }
                    }
                    else
                    {
                        // Content is special.... just update the position
                        cpk.UpdateFileEntry(totalEntries[i]);
                    }
                }

                cpk.WriteCPK(newCPK);
                cpk.WriteITOC(newCPK);
                cpk.WriteTOC(newCPK);
                cpk.WriteETOC(newCPK);
                cpk.WriteGTOC(newCPK);

                newCPK.Close();
                oldFile.Close();

                File.Delete(cpk_name);
                File.Move(cpk_name + ".tmp", cpk_name);
            }
            else if (selectedFunction == "INDEX" && args.Length == 3)
            {
                Console.WriteLine("Not implemented yet sry ;-;");
            }
            else if (args.Length == 2)
            {
                List<FileEntry> entries = cpk.FileTable.OrderBy(x => x.FileOffset).ToList();
                for (int i = 0; i < entries.Count; i++)
                {
                    Console.WriteLine(((entries[i].DirName != null) ? entries[i].DirName + "/" : "") + entries[i].FileName);
                }
            }
        }
    }
}
