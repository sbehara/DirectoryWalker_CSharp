using System;

using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
namespace DirFileSize
{
    public class Program
    {
        public static readonly string dbName = ConfigurationManager.AppSettings.Get("DBName");
        public static readonly string devUserName = ConfigurationManager.AppSettings.Get("DevUsername");
        public static readonly string devPassword = ConfigurationManager.AppSettings.Get("DevPassword");
        public static readonly string devserverName = ConfigurationManager.AppSettings.Get("DevServerName");
        public static readonly string prodUserName = ConfigurationManager.AppSettings.Get("ProdUsername");
        public static readonly string prodPassword = ConfigurationManager.AppSettings.Get("ProdPassword");
        public static readonly string prodServerName = ConfigurationManager.AppSettings.Get("ProdServerName");
        public static readonly string bcpBatchSize = ConfigurationManager.AppSettings.Get("BCPBatchSize");        
        public static readonly string importFileName = ConfigurationManager.AppSettings.Get("ImportFileName");
        public static readonly string exportFileName = ConfigurationManager.AppSettings.Get("ExportFileName");
        public static readonly string pathToBCPProgram = ConfigurationManager.AppSettings.Get("PathToBCPProgram");
        public static readonly string pathToRootSource = ConfigurationManager.AppSettings.Get("PathToRootSource");
        public static readonly string importLogFileName = ConfigurationManager.AppSettings.Get("ImportLogFileName");
        public static readonly string exportLogFileName = ConfigurationManager.AppSettings.Get("ExportLogFileName");
        public static readonly string bcpOutputFileName = ConfigurationManager.AppSettings.Get("BCPOutputFileName");
        public static readonly string importFileDelimiter = ConfigurationManager.AppSettings.Get("ImportFileDelimiter");
        public static readonly string exportFileDelimiter = ConfigurationManager.AppSettings.Get("ExportFileDelimiter");
        public static readonly string notFoundFileName = ConfigurationManager.AppSettings.Get("NotFoundRecordsFileName");
        public static readonly string foundButFailedFileName = ConfigurationManager.AppSettings.Get("FoundButFailedRecordsFileName");
        public static readonly string pathToRootDestination = ConfigurationManager.AppSettings.Get("PathToRootDestination");
        public static readonly string pathToRootMerchants = ConfigurationManager.AppSettings.Get("PathToRootMerchants");
        public static readonly string pathToLoadDataCSVs = ConfigurationManager.AppSettings.Get("PathToLoadDataCSVs");
        public static readonly string loadTableName = ConfigurationManager.AppSettings.Get("LoadTableName");

        static void Main(string[] args)
        {
            //string connectionString = String.Format("Data Source={0}; Initial Catalog={1}; User ID={2}; Password={3}",
            //    devserverName, dbName, userName, password);
            if (args.Length < 1)
            {
                Console.WriteLine("Usage:\n 1. DirFileSize GenData\n 2. DirFileSize LoadDataToDev\n 3. DirFileSize LoadDataToProd");
            }
            else if (args[0].Equals("GenData", StringComparison.OrdinalIgnoreCase))
            {

                System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
                /*
                long bcpRet = BulkReadTableAndOutputToCsv(connectionString);
                timer.Stop();
                Console.WriteLine("bcp time is: "+ timer.ElapsedMilliseconds.ToString());
                timer.Reset();
                timer.Start();
                bcpRet = ReadCSVAndGetFileSize(importFileName);*/
                string folderPath = pathToRootMerchants;
                //long retval = ReadDirEnumerateFolders(folderPath);
                ReadMerchantsDirAndGenerateDirList();
                ReadSubDirFilesAndGenerateFileInfoCSVs();
                //long retVal = ReadDirEnumerateFiles(folderPath);
                timer.Stop();
                //Console.WriteLine("Total size is: " + bcpRet);
                Console.WriteLine("Process took: " + timer.ElapsedMilliseconds.ToString());
            }
            else if (args[0].Equals("LoadDataToDev", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
                ReadCSVsAndLoadToTable();
                timer.Stop();
                //Console.WriteLine("Total size is: " + bcpRet);
                Console.WriteLine("Process took: " + timer.ElapsedMilliseconds.ToString());
            }
            else if (args[0].Equals("LoadDataToProd", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
                ReadCSVsAndLoadToTable(true);
                timer.Stop();
                //Console.WriteLine("Total size is: " + bcpRet);
                Console.WriteLine("Process took: " + timer.ElapsedMilliseconds.ToString());
            }

        }

        public static long ReadCSVsAndLoadToTable(bool prodDb = false)
        {
            try
            {
                if (string.IsNullOrEmpty(pathToLoadDataCSVs))
                {
                    Console.WriteLine("PathToLoadDataCSVs is bad");
                }
                else
                {
                    Console.WriteLine("Attempting to read files from: " + pathToLoadDataCSVs);
                }
                string[] csvFilesToLoad = System.IO.Directory.GetFiles(pathToLoadDataCSVs);
                foreach (string csvFile in csvFilesToLoad)
                {
                    string loadFileName = csvFile;//string.Empty;
                    string command = string.Empty;
                    if (prodDb)
                    {
                        command = string.Format("{0} in {1} -c -t{2} -S {3} -U {4} -P {5}", loadTableName, loadFileName, exportFileDelimiter,
                         prodServerName, prodUserName, prodPassword);

                    }
                    else
                    {
                        command = string.Format("{0} in {1} -c -t{2} -S {3} -U {4} -P {5}", loadTableName, loadFileName, exportFileDelimiter,
                             devserverName, devUserName, devPassword);
                    }
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = pathToBCPProgram;
                    proc.StartInfo.Arguments = command;
                    bool success = false;
                    try
                    {
                        success = proc.Start();
                        proc.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("bcp command failed: " + success.ToString());
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load file names from: " + pathToLoadDataCSVs + " : " + ex.Message);
            }
            return 0;
        }

        public static long ReadSubDirFilesAndGenerateFileInfoCSVs()
        {
            long retVal = 0;
            try
            {
                string[] dirListings = System.IO.Directory.GetFiles(pathToRootDestination, "*.txt");


                // for each *XXX.txt
                foreach (string dirListFile in dirListings)
                {
                    string[] lowLevelDirList = File.ReadAllLines(dirListFile);
                    int dirNameOffset = dirListFile.LastIndexOf("\\");
                    string fileInfoOutput = string.Empty;
                    if (dirNameOffset > -1) fileInfoOutput = pathToRootDestination + dirListFile.Substring(dirNameOffset) + ".csv";

                    ConcurrentBag<string> lowLevelFolderList = new ConcurrentBag<string>();
                    Parallel.ForEach(lowLevelDirList, (lowLevelDir) =>
                    //foreach (string lowLevelDir in lowLevelDirList)
                    {
                        lowLevelFolderList.Add(lowLevelDir);
                    });

                    ConcurrentBag<string> actualFileInfo = new ConcurrentBag<string>();
                    //ConcurrentDictionary<string, string> actualFileInfo = new ConcurrentDictionary<string, string>();
                    Parallel.ForEach(lowLevelFolderList, (curLowLevelFolder) =>
                    //foreach (string curLowLevelFolder in lowLevelFolderList)
                    {
                        try
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(curLowLevelFolder);
                            FileInfo[] fileInfoArray = dirInfo.GetFiles();
                            foreach (FileInfo fileInfo in fileInfoArray)
                            {
                                string fileOtherInfo = fileInfo.LastWriteTime + exportFileDelimiter + fileInfo.Length;
                                try
                                {
                                    string delimitedOutputString = getDelimitedString(fileInfo.FullName, fileOtherInfo);
                                    //if (!actualFileInfo.TryAdd(fileInfo.FullName, fileOtherInfo))
                                    //{
                                    //}
                                    actualFileInfo.Add(delimitedOutputString);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Exception in TryAdd: " + ex.Message);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception in Parallel ForEach curLowLevelFolder: " + curLowLevelFolder + " : " + ex.Message);
                        }
                    });

                    using (StreamWriter sw = new StreamWriter(fileInfoOutput))
                    {
                        foreach (string fileData in actualFileInfo)
                        {
                            sw.WriteLine(fileData);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            return retVal;
        }
        public static string getDelimitedString(string fileFullName, string otherData)
        {
            string outputLine = fileFullName;
            string newOut = outputLine.TrimStart(pathToRootSource.ToCharArray());
            string[] words = newOut.Split('\\');
            List<string> strList = new List<string>();
            strList.Add(words[2]); //MerchantID
            strList.Add(newOut); // Full path

            string fileNameOnly = string.Empty;
            int fNamePos = newOut.LastIndexOf("\\");
            if (fNamePos > -1) fileNameOnly = newOut.Substring(fNamePos);
            if (fileNameOnly.Length > 1) fileNameOnly = fileNameOnly.Substring(1);
            string extn = string.Empty;
            int extnPos = newOut.LastIndexOf('.');
            if (extnPos > -1)
            {
                extn = newOut.Substring(extnPos);
            }
            strList.Add(fileNameOnly);
            strList.Add(extn);
            strList.Add(otherData);
            string delimitedString = string.Join(exportFileDelimiter, strList.ToArray());
            return delimitedString;
        }
        static long ReadMerchantsDirAndGenerateDirList()
        {
            //DirectoryInfo dirInfo = new DirectoryInfo(pathToRootMerchants);
            //DirectoryInfo[] directories = dirInfo.GetDirectories("*", SearchOption.AllDirectories);
            string logDirName = string.Empty;
            ConcurrentBag<string> topDirList = new ConcurrentBag<string>();
            try
            {
                string[] directories = System.IO.Directory.GetDirectories(pathToRootMerchants);//, "*", SearchOption.AllDirectories);
                string dirListFile = pathToRootDestination + "\\TopDirList.log";

                using (StreamWriter sw = new StreamWriter(dirListFile))
                {
                    foreach (string dirName in directories)
                    {
                        logDirName = dirName;
                        if (!dirName.Contains("256XXX"))
                        {
                            topDirList.Add(dirName);
                        }
                        sw.WriteLine(dirName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception : " + logDirName + " : " + ex.Message);
            }

            string logTopDir = string.Empty;
            //Parallel.ForEach(topDirList, (topDir) =>
            foreach (string topDir in topDirList)
            {
                logTopDir = topDir;
                string dirName = String.Empty;
                string secLevelDirListFile = String.Empty;
                int nameOffset = topDir.LastIndexOf("\\");
                if (nameOffset > -1) dirName = topDir.Substring(nameOffset);
                ConcurrentBag<string> secDirNames = new ConcurrentBag<string>();
                try
                {
                    string[] secondLeveDirs = System.IO.Directory.GetDirectories(topDir);
                    if (dirName.Length > 0)
                    {
                        secLevelDirListFile = pathToRootDestination + dirName + ".txt";
                        //foreach (string secDir in secondLeveDirs)
                        Parallel.ForEach(secondLeveDirs, (secDir) =>
                    {
                        //using (StreamWriter sw = new StreamWriter(secLevelDirListFile))
                        //{
                        //    sw.WriteLine(secDir);
                        //}
                        secDirNames.Add(secDir);
                    });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception parallel: " + logTopDir + " : " + ex.Message);
                }
                using (StreamWriter sw = new StreamWriter(secLevelDirListFile))
                {
                    foreach (string secDir in secDirNames)
                    {
                        sw.WriteLine(secDir);
                    }
                }
            }//);

            return 0;
        }

        static long BulkReadTableAndOutputToCsv(string connString)
        {
            //top100string query = "\"Select top 100 ad.Id, ad.MerchantID, ad.Path, ad.Name, Replace(Replace(ad.Description, CHAR(13),''), CHAR(10), ' '), ad.CreateDate from cms.dbo.archivedocument ad with (nolock) where ad.MerchantId is not null order by ad.MerchantID\" ";
            string query = "\"Select ad.Id, ad.MerchantID, ad.Path, ad.Name, Replace(Replace(ad.Description, CHAR(13),''), CHAR(10), ' '), ad.CreateDate from cms.dbo.archivedocument ad with (nolock) where ad.MerchantId is not null order by ad.MerchantID\" ";
            string queryOut = string.Format("queryout {0} -c -t{1} -S {2} -U {3} -P {4} -e {5}", importFileName, importFileDelimiter, devserverName, devUserName, devPassword, bcpOutputFileName);
            string command = query + queryOut;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = pathToBCPProgram;
            proc.StartInfo.Arguments = command;
            bool success = false;
            try
            {
                success = proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("bcp command failed: " + success.ToString());
                Console.WriteLine(ex.Message);
            }
            return 0;
        }


        static long ReadCSVAndGetFileSize(string fileName)
        {
            long totalSize = 0;
            long lineNumber = 0;
            long flushCounter = 0;
            string inputFilePath = importFileName;
            string outputFilepath = exportFileName;
            StreamWriter outputFile = new StreamWriter(outputFilepath);
            StreamWriter notFoundFiles = new StreamWriter(notFoundFileName);
            StreamWriter programOutputFile = new StreamWriter(bcpOutputFileName);
            StreamWriter notEnumeratedFiles = new StreamWriter(foundButFailedFileName);
            ConcurrentDictionary<string, long> fileNameAndSizeMap = new ConcurrentDictionary<string, long>();
            ConcurrentBag<string> notFoundFileList = new ConcurrentBag<string>();
            ConcurrentBag<string> notEnumeratedFileList = new ConcurrentBag<string>();
            try
            {
                Parallel.ForEach(File.ReadLines(inputFilePath), (currentLine, _, lineNum) =>
                {
                    string currentFile = pathToRootSource + currentLine.Split(importFileDelimiter[0])[2];
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(currentFile);
                    if (fileInfo.Exists)
                    {
                        if (!fileNameAndSizeMap.TryAdd(currentLine, fileInfo.Length))
                        {
                            notEnumeratedFileList.Add(currentLine + exportFileDelimiter + "-999");
                        }
                    }
                    else
                    {
                        notFoundFileList.Add(currentLine + exportFileDelimiter + "-999");
                    }
                }
                );

                foreach (var item in fileNameAndSizeMap)
                {
                    lineNumber++;
                    flushCounter++;
                    outputFile.WriteLine(item.Key + exportFileDelimiter + item.Value);
                    totalSize += item.Value;
                    if (flushCounter > 10000)
                    {
                        outputFile.Flush();
                        flushCounter = 0;
                    }
                }
                outputFile.Flush();
                programOutputFile.WriteLine("Files found: " + lineNumber + "; size of all files in bytes: " + totalSize);
                programOutputFile.Flush();
                foreach (var item in notFoundFileList)
                {
                    notFoundFiles.WriteLine(item);
                }
                notFoundFiles.Flush();
                foreach (var item in notEnumeratedFileList)
                {
                    notEnumeratedFiles.WriteLine(item);
                }
                notEnumeratedFiles.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CurrentLineNumber: ", lineNumber);
                Console.WriteLine(ex.Message, ex.StackTrace);
            }
            finally
            {
                outputFile.Close();
                notFoundFiles.Close();
                programOutputFile.Close();
                notEnumeratedFiles.Close();
            }
            return totalSize;
        }

        static long GetDirSizeInclSubFolders(string folderPath)
        {
            //string[] fileNames = System.IO.Directory.GetFiles(folderPath, "*.pdf", System.IO.SearchOption.AllDirectories);
            long totalSize = 0;
            long lineNumber = 0;
            long flushCounter = 0;
            //string inputFilePath = @"C:\Projects\DirListingSize\InputFile.txt";
            //string outputFilepath = @"C:\Projects\DirListingSize\OutputFile.txt";
            string inputFilePath = @"InputFileList.txt";
            string outputFilepath = @"OutputFileList.txt";
            StreamWriter outputFile = new StreamWriter(outputFilepath);
            //StreamWriter notFoundFiles = new StreamWriter(@"C:\Projects\DirListingSize\NotFoundFiles.txt");
            //StreamWriter notEnumeratedFiles = new StreamWriter(@"C:\Projects\DirListingSize\FailedEnumerationFiles.txt");
            StreamWriter notFoundFiles = new StreamWriter(@"NotFoundFiles.txt");
            StreamWriter notEnumeratedFiles = new StreamWriter(@"FailedEnumerationFiles.txt");
            ConcurrentDictionary<string, long> fileNameAndSizeMap = new ConcurrentDictionary<string, long>();
            ConcurrentBag<string> notFoundFileList = new ConcurrentBag<string>();
            ConcurrentBag<string> notEnumeratedFileList = new ConcurrentBag<string>();
            try
            {
                string[] fileList = File.ReadAllLines(inputFilePath);
                Parallel.ForEach(fileList, (currentFile) =>
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(currentFile);
                    if (fileInfo.Exists)
                    {
                        if (!fileNameAndSizeMap.TryAdd(fileInfo.FullName, fileInfo.Length))
                        {
                            notEnumeratedFileList.Add(currentFile);
                        }
                    }
                    else
                    {
                        notFoundFileList.Add(currentFile);
                    }
                }
                );

                foreach (var item in fileNameAndSizeMap)
                {
                    lineNumber++;
                    flushCounter++;
                    outputFile.WriteLine(item.Key + "\t" + item.Value + " bytes");
                    totalSize += item.Value;
                    if (flushCounter > 10000)
                    {
                        outputFile.Flush();
                        flushCounter = 0;
                    }
                }
                outputFile.WriteLine("Total size of all " + lineNumber + " files in bytes: " + totalSize);
                outputFile.Close();
                foreach (var item in notFoundFileList)
                {
                    notFoundFiles.WriteLine(item);
                }
                notFoundFiles.Close();
                foreach (var item in notEnumeratedFileList)
                {
                    notEnumeratedFiles.WriteLine(item);
                }
                notEnumeratedFiles.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CurrentLineNumber: ", lineNumber);
                Console.WriteLine(ex.Message, ex.StackTrace);
            }
            return totalSize;
        }
        static long ReadTableAndOutputData(string connString)
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection(connString);
            try
            {
                sqlConn.Open();
                Console.WriteLine("Connection opened successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sqlConn.Close();
            }

            return 0;
        }
        static long ReadFilesRecursiveFullDepth(DirectoryInfo dirInfo)
        {
            FileInfo[] fileInfoArray = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            //Console.WriteLine(fileInfoArray.Length);

            ConcurrentBag<string> fileFoundData = new ConcurrentBag<string>();
            string folderPath = dirInfo.FullName;
            string outputFileName = folderPath.Substring(folderPath.LastIndexOf('\\'));
            outputFileName = pathToRootDestination + outputFileName + ".csv";

            int lineNumber = 0;
            //bool logMsg = false;
            //if (dirInfo.Name == "100XXX") logMsg = true;
            StreamWriter outputWriter = new StreamWriter(outputFileName);
            string outputLine = "";
            string newOut = "";
            string extn = "";
            string delimitedString = string.Empty;
            List<string> strList = new List<string>();
            try
            {
                foreach (FileInfo fileInfo in fileInfoArray)
                {
                    strList.Clear();
                    extn = string.Empty;
                    newOut = string.Empty;
                    outputLine = string.Empty;
                    delimitedString = string.Empty;
                    try
                    {
                        outputLine = fileInfo.FullName;
                        newOut = outputLine.TrimStart(pathToRootSource.ToCharArray());
                        string[] words = newOut.Split('\\');
                        strList.Add(words[2]);
                        strList.Add(newOut);

                        int extnPos = newOut.LastIndexOf('.');
                        /*if (extnPos == -1)
                        {
                            Console.WriteLine("Wait here: -1");
                        }*/
                        if (extnPos != -1)
                        {
                            extn = newOut.Substring(extnPos);
                        }
                        strList.Add(extn);
                        strList.Add(fileInfo.LastWriteTime.ToString());
                        strList.Add(fileInfo.Length.ToString());
                        delimitedString = string.Join(exportFileDelimiter, strList.ToArray());
                        fileFoundData.Add(delimitedString);
                        //strList.Clear();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Wait here " + ex.Message);
                    }
                }
                /*if (logMsg)
                {
                    Console.WriteLine("stop here for a while");
                }*/
                foreach (var fileData in fileFoundData)
                {
                    lineNumber++;
                    outputWriter.WriteLine(fileData);
                    //if (lineNumber > 100000)
                    //{
                    //    outputWriter.Flush();
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("outputLine: " + outputLine + " : Exception : " + ex.Message);
            }
            finally
            {
                outputWriter.Close();
            }
            return fileInfoArray.Length;
        }
        static long ReadFilesParallelRecursiveFullDepth(DirectoryInfo dirInfo)
        {
            FileInfo[] fileInfoArray = dirInfo.GetFiles("*", SearchOption.AllDirectories);

            ConcurrentBag<string> fileFoundData = new ConcurrentBag<string>();
            string folderPath = dirInfo.FullName;
            string outputFileName = folderPath.Substring(folderPath.LastIndexOf('\\'));
            outputFileName = pathToRootDestination + outputFileName + ".csv";

            //int lineNumber = 0;
            StreamWriter outputWriter = new StreamWriter(outputFileName);
            try
            {
                Parallel.ForEach(fileInfoArray, (fileInfo) =>
                //foreach (FileInfo fileInfo in fileInfoArray)
                {
                    string extn = "";
                    string newOut = "";
                    string outputLine = "";
                    string delimitedString = string.Empty;
                    List<string> strList = new List<string>();

                    try
                    {
                        outputLine = fileInfo.FullName;
                        newOut = outputLine.TrimStart(pathToRootSource.ToCharArray());
                        string[] words = newOut.Split('\\');
                        strList.Add(words[2]);
                        strList.Add(newOut);

                        int extnPos = newOut.LastIndexOf('.');
                        if (extnPos != -1)
                        {
                            extn = newOut.Substring(extnPos);
                        }
                        strList.Add(extn);
                        strList.Add(fileInfo.LastWriteTime.ToString());
                        strList.Add(fileInfo.Length.ToString());
                        delimitedString = string.Join(exportFileDelimiter, strList.ToArray());
                        fileFoundData.Add(delimitedString);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Wait here " + ex.Message);
                    }
                });

                foreach (var fileData in fileFoundData)
                {
                    //lineNumber++;
                    outputWriter.WriteLine(fileData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("outputFileName: " + outputFileName + " : Exception : " + ex.Message);
            }
            finally
            {
                outputWriter.Close();
            }
            return fileInfoArray.Length;
        }
        static long ReadDirEnumerateFiles(string folderPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            DirectoryInfo[] directories = dirInfo.GetDirectories();
            /*foreach (DirectoryInfo pwd in directories)
            {
                ReadFilesRecursiveFullDepth(pwd);
            }*/
            Parallel.ForEach(directories, (pwd) =>
            {
                //ReadFilesRecursiveFullDepth(pwd);
                ReadFilesParallelRecursiveFullDepth(pwd);
            });
            return 0;
        }
        static long ReadDirEnumerateFolders(string folderPath)
        {
            //string[] fileNames = System.IO.Directory.GetFiles(folderPath, "*.pdf", System.IO.SearchOption.AllDirectories);
            //string[] dirNames = System.IO.Directory.GetDirectories(folderPath);
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            //DirectoryInfo[] directories = dirInfo.GetDirectories("*", SearchOption.AllDirectories);
            DirectoryInfo[] directories = dirInfo.GetDirectories();
            foreach (DirectoryInfo pwd in directories)
            {
                ReadFilesRecursiveFullDepth(pwd);
            }
            Parallel.ForEach(directories, (pwdDir) =>
            {

            });
            FileInfo[] fileInfo = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            string dirList = pathToRootDestination + "\\DirList.txt";
            string curDir = "";
            long numLines = 0;
            long totalNumLines = 0;
            string[] dirNames = { };
            //StreamWriter outDirFile = new StreamWriter(dirList);
            try
            {
                foreach (string dirName in dirNames)
                {
                    //outDirFile.WriteLine(dirName);
                    curDir = dirName;
                    numLines = ReadDirContentsRecursive(dirName);
                    totalNumLines += numLines;
                }
                //outDirFile.Flush();*/
                /*Parallel.ForEach(dirNames, (topLevelDir) =>
                {
                    ReadDirContentsRecursive(topLevelDir);
                });*/
            }
            catch (Exception ex)
            {
                Console.WriteLine("curDir and totalNumlines are: " + curDir + totalNumLines.ToString() + " message: " + ex.Message);
            }
            finally
            {
                //outDirFile.Close();
            }
            return 0;
        }
        public static long ReadDirContentsRecursive(string folderPath)
        {
            ConcurrentDictionary<string, long> fileNameAndSizeMap = new ConcurrentDictionary<string, long>();
            ConcurrentBag<string> fileFoundData = new ConcurrentBag<string>();
            string outputFileName = folderPath.Substring(folderPath.LastIndexOf('\\'));
            outputFileName = pathToRootDestination + outputFileName + ".csv";
            int lineNumber = 0;
            StreamWriter outputWriter = new StreamWriter(outputFileName);
            try
            {
                string[] subFolderList = System.IO.Directory.GetDirectories(folderPath);

                Parallel.ForEach(subFolderList, (currentDir) =>
                {
                    string[] fileList = System.IO.Directory.GetFiles(currentDir);
                    foreach (string fileName in fileList)
                    {
                        System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
                        if (fileInfo.Exists)
                        {
                            string outputLine = fileInfo.FullName;
                            string newOut = outputLine.TrimStart(pathToRootSource.ToCharArray());
                            string[] words = newOut.Split('\\');
                            List<string> strList = new List<string>();
                            strList.Add(words[2]);
                            strList.Add(newOut);
                            strList.Add(newOut.Substring(newOut.LastIndexOf('.')));
                            strList.Add(fileInfo.LastWriteTime.ToString());
                            strList.Add(fileInfo.Length.ToString());
                            string delimitedString = string.Join(exportFileDelimiter, strList.ToArray());
                            fileFoundData.Add(delimitedString);
                        }
                        else
                        {
                            Console.WriteLine("Not found");
                        }
                    }
                });

                foreach (var data in fileFoundData)
                {
                    outputWriter.WriteLine(data);
                    lineNumber++;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("CurrentLineNumber: ", lineNumber);
                Console.WriteLine(ex.Message, ex.StackTrace);
            }
            finally
            {
                outputWriter.Close();
            }
            //return totalSize;*/
            return lineNumber;
        }
    }
}
