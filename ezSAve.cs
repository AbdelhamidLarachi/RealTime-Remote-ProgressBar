using System;
using System.Collections.Generic; // used for data types
using System.Diagnostics; // used to calculate time
using System.IO; // used to manage files and directories
using System.Linq;
using System.Net.Sockets;
using System.Threading; // used for progression tests (not required)
using System.Threading.Tasks;
using Newtonsoft.Json; // used for json


class RealTimeJson
{
    // initialize variables for real time json file (Used once a backup is executed)

    public DateTime creationTime { get; set; }
    public DateTime lastWriteTime { get; set; }
    public string extension { get; set; }
    public string path { get; set; }
    public string name { get; set; }
    public long CurrentFileSize { get; set; }
    public int remaining_files { get; set; }
    public string progression { get; set; }
    public string bytesRemaining { get; set; }

}
class Logs
{
    // (used for backup logs, created in each backup creation)

    public string taskname { get; set; }
    public string source { get; set; }
    public string destination { get; set; }
    public int backupType { get; set; }
    public long filesize { get; set; }
    public DateTime Horodotage { get; set; }
    public double time { get; set; }
}

class Newbackup
{
    // (used for backup logs, created in each backup creation)

    public string taskname { get; set; }
    public string source { get; set; }
    public string destination { get; set; }
    public int backupType { get; set; }
}

class BackupDirectory
{

    // initialize variables for real time Json

    static long copiedBytes = 0;
    public static long totalFolderSize = 0;
    public static int filesNBcount = 0;

    // initialize variables for sequential methode

    public static string taskname;
    public static int backupType;
    public static double time;
    public static int jsonLength;
    public static dynamic BackupsList;

    // check background app to persue process
    public static bool pursue;

    // share total files to copy in sequential backup (for progressbar)
    public static int SeqtotalFiles;
    public static bool seqType;
    public static int backupscounter;
    public static string alltotalFiles;

    public static List<int> thPriority = new List<int>();

    public static void intro()
    {
    // ask for 1st steps
    restartIntro:

        string createdBackups = "/Users/nginx-iwnl/Desktop/hidden/CreatedBackups";
        File.AppendAllText(createdBackups, ""); // generate a json file if not found


        string json = File.ReadAllText(createdBackups); // read text from json file
        json = json.Replace("][", ",");

        // using type DYNAMIC to control objects over class name
        BackupsList = JsonConvert.DeserializeObject<dynamic>(json);
        jsonLength = json.Length;

        int alltotalFilesINT=0;
        foreach (var Backup in BackupsList)   // get total backups files
        {
            int getTotalfiles = Directory.GetFiles(Convert.ToString(Backup.source)).Length;
            alltotalFilesINT += getTotalfiles;
                }

         alltotalFiles = alltotalFilesINT.ToString();

        Console.WriteLine(alltotalFiles); // total files in backups


        test();
        Console.WriteLine("1- Create a new backup \n");
        Console.WriteLine("2- Execute an existing backup \n");

        string operationTypeString = Console.ReadLine();
        int operationType = Convert.ToInt32(operationTypeString);
        backupscounter = 0;




        if (jsonLength > 0) // check created backups, must not be over 5 times
        {
            foreach (var Backup in BackupsList)
            {
                backupscounter++;//Back
            }
        }


        switch (operationType)
        {
            case 1:

                if (backupscounter >= 5)
                {
                    Console.WriteLine("You have reached the maximum number of backups... \n");
                    goto restartIntro;
                }
                else
                {
                    Console.WriteLine("Creating a new backup... \n");
                    getinfo();

                }
                break;
            case 2:
                if (jsonLength > 0)
                {

                    foreach (var Backup in BackupsList)
                    {
                        Console.WriteLine("- " + Backup.taskname);
                    }

                    Console.WriteLine("\nChoose the Backup Type :\n");
                    Console.WriteLine("1- Specific Backup : ");
                    Console.WriteLine("* Execute certein backup from backups list. \n");
                    Console.WriteLine("2- Sequential Bakcup : ");
                    Console.WriteLine("* Execute all backups from list. \n");

                    string BackupTypeString = Console.ReadLine();

                    int BackupTypeInt = Convert.ToInt32(BackupTypeString);
                    ExistingBackup(BackupTypeInt, BackupsList);
                }
                else
                {
                    Console.WriteLine("You have no backups please create some\n");
                    goto restartIntro;
                }
                break;
            default:     // default backup (incorrect input).
                Console.WriteLine("Choose 1 or 2 : \n");
                goto restartIntro;

        }
    }

    public static void ExistingBackup(int BackupTypeInt, dynamic Backups)    // used to lunch the Backups
    {
        switch (BackupTypeInt)
        {
            case 1:
                Console.WriteLine("\nSpecific chosen..\n");
                Console.WriteLine("Enter The name of your backup :\n");

            incorrectTaskname:

                taskname = Console.ReadLine();
                pursue = false;

                Process[] checkProcess = Process.GetProcessesByName("Calculator");

                if (checkProcess.Length > 0)
                {
                    pursue = true;
                }

                while (pursue == true)
                {
                    Console.WriteLine("Cannot execute backup while having a background process!");
                    Thread.Sleep(3000);
                    Process[] recheck = Process.GetProcessesByName("Calculator");
                    if (recheck.Length == 0)
                    {
                        pursue = false;
                    }
                }

                foreach (var Backup in Backups)
                {
                    if (Backup.taskname == taskname)
                    {
                        string Source = Backup.source;
                        string Destination = Backup.destination;

                        DirectoryInfo source = new DirectoryInfo(Source);
                        DirectoryInfo destination = new DirectoryInfo(Destination);

                        if (Backup.backupType == 1)
                        {
                            Stopwatch watch = new Stopwatch();
                            watch.Start();

                            Differential(source, destination);

                            watch.Stop();
                            time = watch.Elapsed.TotalMilliseconds;
                            WriteLogs(source, destination, taskname, time);
                        }

                        else if (Backup.backupType == 2)
                        {
                            Stopwatch watch = new Stopwatch();
                            watch.Start();

                            Copy(Source, Destination);

                            watch.Stop();
                            time = watch.Elapsed.TotalMilliseconds;
                            WriteLogs(source, destination, taskname, time);
                        }
                    }
                }



                break;
            case 2:
                Console.WriteLine("\nSequential chosen...\n");


                foreach (var Backup in Backups)
                {
                    // using default thread is sufficient inside foreach loop

                    string Source = Backup.source;
                    string Destination = Backup.destination;

                    DirectoryInfo source = new DirectoryInfo(Source);
                    DirectoryInfo destination = new DirectoryInfo(Destination);

                    setPriority(source, destination);   // get priority by extension from each backup



                };


                for (int i = 0; i <= backupscounter; i++)
                {
                    bool isEmpty = !thPriority.Any();  // check if list contains elements

                    if (isEmpty)
                    {
                        Console.WriteLine("\nSuccess : All prioretaires files are copied!\n");
                        // all elements has been cleared
                    }
                    else
                    {
                        var maxIndex = thPriority.IndexOf(thPriority.Max()); // get the index with max priority value

                        string Source = Backups[maxIndex].source;
                        string Destination = Backups[maxIndex].destination;   // get info about supperior backup

                        Console.WriteLine(Destination);   // make sure getting the right priority

                        DirectoryInfo source = new DirectoryInfo(Source);
                        DirectoryInfo destination = new DirectoryInfo(Destination);

                        copyPriorityFiles(source, destination);
                        thPriority.RemoveAt(maxIndex);   // remove index after copying all priority files
                    }

                };

                Parallel.ForEach((IEnumerable<dynamic>)Backups, Backup =>
                {
                    string Source = Backup.source;
                    string Destination = Backup.destination;

                    DirectoryInfo source = new DirectoryInfo(Source);
                    DirectoryInfo destination = new DirectoryInfo(Destination);
                    Console.WriteLine("Thread Id: {0}", Thread.CurrentThread.ManagedThreadId);
                    copyNonPriorityFiles(source, destination);   // get priority by extension from each backup

                });

                Console.WriteLine("\nSuccess : All NON prioretaires files are copied!\n");


                break;
            default:
                //  incorrect backup name (incorrect input).
                Console.WriteLine("Backup not found! type the name again :\n");
                goto incorrectTaskname;
        }
    }

    public static void listFolders(string WorkingDir)  // display sequential Backups in main Backup
    {
        DirectoryInfo BackupList = new DirectoryInfo(WorkingDir);
        DirectoryInfo[] List = BackupList.GetDirectories();

        foreach (DirectoryInfo Backups in List)
        {
            Console.WriteLine(Backups.Name);    // Display by only their names not the full path
        }
    }
    public static void FileSize(string source)
    {
        DirectoryInfo target = new DirectoryInfo(source);
        foreach (FileInfo fi in target.GetFiles("*.*", SearchOption.AllDirectories))
        {
            totalFolderSize += fi.Length;
        }
    }

    public static void getinfo()
    {

        Console.WriteLine("\nIn order to create a backup, please give it a name :");
        string BackupName = Console.ReadLine();

    insertCorrectSourcePath: //go back in case the source path doesn't exist!

        Console.WriteLine("\nIn order to backup, please type the source file's path :");
        string sourceDirectory = Console.ReadLine();




        if (Directory.Exists(sourceDirectory))
        {
            FileSize(sourceDirectory);
            Console.WriteLine("\nCorrect path!. [path=" + sourceDirectory + "]");
            string sourceFolderName = Path.GetFileName(sourceDirectory);
            Console.WriteLine("Source Folder name :" + sourceFolderName);
        }
        else
        {
            Console.WriteLine("\nPlease type a correct source path!");
            goto insertCorrectSourcePath;
        }

    insertCorrectDestinationPath: //go back in case the target path do not exist!
        Console.WriteLine("\nType a target path for backup :");

        string DestinationDirectory = Console.ReadLine();

        if (Directory.Exists(DestinationDirectory))
        {
            Console.WriteLine("Target path selected. [path=" + DestinationDirectory + "]");
            taskname = BackupName;   // get taskname from target path
            Console.WriteLine("Backup Folder name :" + taskname);
        }
        else
        {
            taskname = BackupName;   // get taskname from target path
            Console.WriteLine("Backup Folder name :" + taskname + "\n");
            Console.WriteLine("\nYour Destination path doesn't exist, Do you want to create it?! Y/N");

            DirectoryInfo target = new DirectoryInfo(DestinationDirectory);
            string YORN = Console.ReadLine();
            if (YORN == "Y" || YORN == "y")
            {
                if (!target.Exists)
                {
                    target.Create();
                }
            }
            else { goto insertCorrectDestinationPath; }
        }


        Console.WriteLine("\n Please select a backup Type :");
        Console.WriteLine(" 1 - Differential backup :\n ");
        Console.WriteLine("a backup of all changes made since the last full backup.\n");
        Console.WriteLine("About : \n");
        Console.WriteLine("* More efficient use of storage space");
        Console.WriteLine("* Slower restore \n");
        Console.WriteLine(" 2 - Mirror backup : \n");
        Console.WriteLine("A straight copy of the selected folders and files at a given instant in time. \n");
        Console.WriteLine("About : \n");
        Console.WriteLine("* The backup is clean and does not contain old and obsolete files");
        Console.WriteLine("* Fastest backup with no compression \n ");

    insertCorrectType: //go back in case the incorrect input!

        Console.WriteLine("1 or 2 : \n");

        string backupTypeString = Console.ReadLine();
        backupType = Convert.ToInt32(backupTypeString);
        if (backupType != 1 && backupType != 2)
        {
            goto insertCorrectType;
        }

        // reinstialise objects to meet directory info parameters

        DirectoryInfo source = new DirectoryInfo(sourceDirectory);
        DirectoryInfo destination = new DirectoryInfo(DestinationDirectory);

        Stopwatch watch = new Stopwatch();
        watch.Start();
        watch.Stop();
        time = watch.Elapsed.TotalMilliseconds; // asked to give time in ms

        Console.WriteLine("\nWould you like to save your new backup?");
        Console.WriteLine("Y/N? \n");
        string seqListner = Console.ReadLine();
        if (seqListner == "Y" || seqListner == "y")
        {
            CreateBackup(source, destination, taskname, backupType); // store backup logs in json format
        }
        Console.WriteLine("\n\n");

        intro();
    }

    public static void Copy(string sourceDirectory, string targetDirectory)
    {

        DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
        DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

        // Calculate total folder size

        DirectoryInfo dir = new DirectoryInfo(sourceDirectory);

        foreach (FileInfo fi in dir.GetFiles("*.*", SearchOption.AllDirectories))
        {
            totalFolderSize += fi.Length;
        }

        CopyAll(diSource, diTarget);

    }

    public static void WriteLogs(DirectoryInfo source, DirectoryInfo destination, string taskname, double timer)
    {
        // logs for executed backups

        string logfilepath = "/Users/nginx-iwnl/Desktop/hidden/ExecutedBackups";
        List<Logs> saveData = new List<Logs>();

        saveData.Add(new Logs()
        {
            taskname = taskname,
            Horodotage = DateTime.Now,
            source = source.FullName,
            backupType = backupType,
            destination = destination.FullName,
            filesize = totalFolderSize,
            time = timer
        });

        string json = JsonConvert.SerializeObject(saveData.ToArray(), Formatting.Indented); //Formatting.Indented is used for a pretty json file
                                                                                            // Write string to file in json format
        File.AppendAllText(logfilepath, json); // call parent target folder to save json
                                               // appendAllText to add string instead of replace

    }
    // logs for created backups

    public static void CreateBackup(DirectoryInfo source, DirectoryInfo destination, string taskname, int backupType)
    {
        string BackupsFile = "/Users/nginx-iwnl/Desktop/hidden/CreatedBackups";
        List<Newbackup> saveData = new List<Newbackup>();

        saveData.Add(new Newbackup()
        {
            taskname = taskname,
            source = source.FullName,
            destination = destination.FullName,
            backupType = backupType
        });

        string json = JsonConvert.SerializeObject(saveData.ToArray(), Formatting.Indented); //Formatting.Indented is used for a pretty json file
                                                                                            // Write string to file in json format
        File.AppendAllText(BackupsFile, json);
    }



    public static void RealTimeJson(FileInfo fi, int totalFolderFiles, DirectoryInfo destination)
    {
        List<RealTimeJson> folderData = new List<RealTimeJson>();

        // Serialize real-time data to json file

        folderData.Add(new RealTimeJson()
        {
            path = fi.FullName,
            name = fi.Name,
            CurrentFileSize = fi.Length,
            creationTime = fi.CreationTime,
            lastWriteTime = fi.LastWriteTime,
            extension = fi.Extension,
            remaining_files = totalFolderFiles - filesNBcount,
            progression = filesNBcount + " out of " + totalFolderFiles,
            bytesRemaining = copiedBytes + " out of " + totalFolderSize
        });
        string json = JsonConvert.SerializeObject(folderData.ToArray(), Formatting.Indented); //Formatting.Indented is used for a pretty json file

        // Write string to file in json format
        File.WriteAllText(destination.Parent.FullName + "/json", json); // call parent target folder to save json
                                                                        // writeAllText to replace the text used for real time

        //  "/json" to name json file

    }



    public static void setPriority(DirectoryInfo source, DirectoryInfo target)
    {

        int Thpriority = 0;

        foreach (FileInfo fi in source.GetFiles())
        {
            if (fi.Extension == ".docx")
            {
                Thpriority++;
            }
        }

        thPriority.Add(Thpriority);
    }


    public static void copyPriorityFiles(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (FileInfo fi in source.GetFiles())
        {
            if (fi.Extension == ".docx")
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
        }
    }

    public static void test()
    {
        for (int i = 0; i < 100; i++)
        {
            TcpClient client = new TcpClient("172.20.10.2", 4523);
            BinaryWriter bw = new BinaryWriter(client.GetStream());
            bw.Write(alltotalFiles);
            Console.WriteLine("sending..");
            Thread.Sleep(1000);
        }
    }
    public static void copyNonPriorityFiles(DirectoryInfo source, DirectoryInfo target)
    {

        foreach (FileInfo fi in source.GetFiles())
        {
            if (fi.Extension != ".docx")
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);

            }
        }
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }

    public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {

        int totalFolderFiles = source.GetFiles("*", SearchOption.AllDirectories).Length;

        // Copy each file into the new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            Thread.Sleep(2500);  // a second sleep for each file is copied (just to look cool :D)
            filesNBcount++;
            copiedBytes += fi.Length; // increment the size of each copied file.
            if (seqType)
            {
                drawTextProgressBar(SeqtotalFiles); // Backup progress Bar
            }
            else
            {
                drawTextProgressBar(totalFolderFiles); // Backup progress Bar

            }

            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            // Serialize real-time data to json file
            RealTimeJson(fi, totalFolderFiles, target);
            // Write string to file in json format
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            filesNBcount++;
            drawTextProgressBar(totalFolderFiles); //  Completing progress Bar for sub files

            DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }

    public static void Differential(DirectoryInfo source, DirectoryInfo destination)
    {
        // Copy files.  
        bool Emptydestination = true;
        FileInfo[] files = source.GetFiles();
        FileInfo[] destFiles = destination.GetFiles();
        int totalFolderFiles = 0;
        long totalSize = 0;

        if (Directory.GetFileSystemEntries(destination.FullName).Length != 0)
        {
            Emptydestination = false;
        }


        // Claculating the totalt number of files to copy
        foreach (FileInfo file in files)
        {
            foreach (FileInfo fileD in destFiles)
            {

                // Copy only modified files
                if (file.Name == fileD.Name && file.LastWriteTime > fileD.LastWriteTime)
                {
                    totalFolderFiles++;
                    totalSize = totalSize + fileD.Length;
                }
                // Copy all new files  
                else if (!File.Exists(Path.Combine(destination.FullName, file.Name)))
                {
                    totalFolderFiles++;
                    totalSize = totalSize + fileD.Length;
                }

            }
        }


        foreach (FileInfo file in files)
        {
            foreach (FileInfo fileD in destFiles)
            {

                Emptydestination = false;
                // Copy only modified files
                if (file.Name == fileD.Name && file.LastWriteTime > fileD.LastWriteTime)
                {
                    file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
                    filesNBcount++;
                    RealTimeJson(file, totalFolderFiles, destination);
                }
                // Copy all new files  
                else if (!File.Exists(Path.Combine(destination.FullName, file.Name)))
                {
                    file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
                    filesNBcount++;
                    RealTimeJson(file, totalFolderFiles, destination);

                }
            }
        }
        if (Emptydestination == true)
        {
            CopyAll(source, destination);
        }
        // Process subdirectories.  
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            filesNBcount++;
            DirectoryInfo nextTargetSubDir =
                destination.CreateSubdirectory(diSourceSubDir.Name);
            Differential(diSourceSubDir, nextTargetSubDir);
        }
    }


    private static void drawTextProgressBar(int totalFolderFiles)
    {
        //draw empty progress bar
        Console.CursorLeft = 0;
        Console.Write("["); //start
        Console.CursorLeft = 32;
        Console.Write("]"); //end
        Console.CursorLeft = 1;
        float onechunk = 30.0f / totalFolderFiles;

        //draw filled part
        int position = 1;
        for (int i = 0; i < onechunk * filesNBcount; i++)
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.CursorLeft = position++;
            Console.Write(" ");
        }

        //draw unfilled part
        for (int i = position; i <= 31; i++)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.CursorLeft = position++;
            Console.Write(" ");
        }

        //draw totals
        Console.CursorLeft = 35;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(filesNBcount.ToString() + " of " + totalFolderFiles.ToString() + "    "); //blanks at the end remove any excess
    }

    public static void Main()
    {

        Console.WriteLine(" ______                                    ");
        Console.WriteLine("|  ____|                                   ");
        Console.WriteLine("| |__   __ _ ___ _   _ ___  __ ___   _____ ");
        Console.WriteLine("|  __| / _` / __| | | / __|/ _` \\ \\ / / _ \\");
        Console.WriteLine("| |___| (_| \\__ \\ |_| \\__ \\ (_| |\\ V /  __/");
        Console.WriteLine("|______\\__,_|___/\\__, |___/\\__,_| \\_/ \\___|");
        Console.WriteLine("                  __/ |                    ");
        Console.WriteLine("                 |___/                    Spaghetti v1\n");

        intro();
    }
}