using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;

//TODO: Clean up all the comments, move some code to their own functions and files, cry at 6 year old code
//Also update maybe the .net version or see about options that might be friendlier to linux
//Also maybe do more crash prevention/handling
namespace CSModLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Mod
    {
        public string Version = "";
        public string ModName = "";
        public string DoukutsuClubID = "";
        public string DownloadLink = "";
        public string VersionLink = "";

        public string DoukutsuFile = "";
        public string ConfigFile = "";
        public List<string> FilesToUpdate = new List<string>();
        public List<string> FilesToAdd = new List<string>();
        public string Folder = "";

        //TODO: i'm sure this could be better
        public Mod(string version, string modName, string doukutsuClubId, string downloadLink, string versionLink, string doukutsuFile, string configFile, List<string> filesU, List<string> filesA, string folder)
        {
            Version = version;
            ModName = modName;
            DoukutsuClubID = doukutsuClubId;
            DownloadLink = downloadLink;
            VersionLink = versionLink;
            DoukutsuFile = doukutsuFile;
            ConfigFile = configFile;
            FilesToUpdate = filesU;
            FilesToAdd = filesA;
            Folder = folder;
        }
    }

    //TODO: maybe merge this with Mod
    public class JSONMod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public JSONModVersion Version { get; set; }
        public JSONModLinks Links { get; set; }
        public JSONModFiles Files { get; set; }
    }
    public class JSONModVersion
    {
        public string VersionNum { get; set; }
        public string UpdateLog { get; set; }
    }
    public class JSONModLinks
    {
        public string DoukutsuClubID { get; set; }
        public string DownloadLink { get; set; }
        public string VersionLink { get; set; }
    }
    public class JSONModFiles
    {
        public string DoukutsuFile { get; set; }
        public string ConfigFile { get; set; }
        public string[] FilesToUpdate { get; set; }
        public string[] FilesToAdd { get; set; }
    }

    public partial class MainWindow : Window
    {
        //TODO: just store this in a current mod object or something
        public static string version = "";

        public static string modName = "";
        public static string modAuthor = "";
        public static string modDesc = "";
        public static string doukutsuClubID = "";
        public static string downloadLink = "";
        public static string versionLink = "";

        public static string doukutsuFile = "";
        public static string configFile = "";
        public static List<string> filestoupdate = new List<string>();
        public static List<string> filestoadd = new List<string>();
        public static string folder = "";


        public static uint waffles = 0;
        public static bool waffleshown = false;

        public MainWindow()
        {
            //TODO: Search for JSON files instead (since that's what they are) and check their contents instead to validate if it's a modinfo file
            //Also something is going wrong where the current folder is getting loaded as well
            //ALSO just allow user to define their mods folder!!!!!!!!!!!
            var foundFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.cmf", SearchOption.AllDirectories);


            InitializeComponent();
            foreach (var file in foundFiles)
            {
                string modname = new DirectoryInfo(file).Parent.Name; //TODO: Dont use the folder name oh god

                //foundmods.Add(System.IO.Path.GetDirectoryName(file)); 
                ListBoxItem item = new ListBoxItem();
                item.Content = modname;//System.IO.Path.GetDirectoryName(file);
                folder = modname;
                Modlist.Items.Add(item);
            }
        }

        private void Modlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem folder1 = (ListBoxItem)Modlist.SelectedItem;
            folder = folder1.Content.ToString();
            GetInfo(Directory.GetFiles(folder, "*.cmf")[0]);

            ModTitle.Text = modName;
            ModInfo.Text = modDesc;
            VersionBox.Text = $"Version: v{version}";
            AuthorBox.Text = $"Made by: {modAuthor}";
            PlayButton.IsEnabled = true;
            ConfigButton.IsEnabled = true;
            UpdateButton.IsEnabled = true;

            if (!File.Exists($"{folder}/{doukutsuFile}"))
            {
                PlayButton.IsEnabled = false;
            }
            if (string.IsNullOrWhiteSpace(downloadLink) || downloadLink.Substring(downloadLink.Length - 3) != "zip")
            {
                UpdateButton.IsEnabled = false;
            }

            if (modName == "{ERROR:NULL}")
            {
                ModInfo.Text = "Something has gone wrong while reading the .cmf file, is the syntax correct?";
                VersionBox.Text = "";
                AuthorBox.Text = "";
                ModTitle.Text = "CSModLauncher";
                PlayButton.IsEnabled = false;
                UpdateButton.IsEnabled = false;
            }
        }


        private void GetInfo(string filepath)
        {
            //I have absolutely no shame in what I'm doing here -- (what was i ashamed of?)
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string cmf = File.ReadAllText(filepath);
                JSONMod jsonmod = serializer.Deserialize<JSONMod>(cmf);

                modName = jsonmod.Name;
                modAuthor = jsonmod.Author;
                modDesc = jsonmod.Description;
                version = jsonmod.Version.VersionNum;
                doukutsuClubID = jsonmod.Links.DoukutsuClubID;
                downloadLink = jsonmod.Links.DownloadLink;
                versionLink = jsonmod.Links.VersionLink;
                doukutsuFile = jsonmod.Files.DoukutsuFile;
                configFile = jsonmod.Files.ConfigFile;
                filestoadd = jsonmod.Files.FilesToAdd.ToList();
                filestoupdate = jsonmod.Files.FilesToUpdate.ToList();
            }
            catch
            {
                modName = "{ERROR:NULL}";
            }
        }

        //whats going on here wtf
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Thread PlayThread = new Thread(new ThreadStart(Play));
            PlayThread.Start();
            Thread ExitThread = new Thread(new ThreadStart(Exit));
            Close();
        }


        private void Play()
        {
            Process.Start($"{folder}\\{doukutsuFile}");
        }

        private void Exit()
        {
            Thread.Sleep(100); //why is this here
            Close();
        }


        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(configFile))
            {
                Process.Start($"{folder}\\{configFile}");
            }
            else
            {
                ConfigWindow fd = new ConfigWindow();
                fd.ShowDialog();
            }
            //string configlocation = $"{folder}\\doconfig.exe";
            //Process.Start(configlocation);
        }



        private void CreateNewCmf_Click(object sender, RoutedEventArgs e)
        {
            AddCMFWindow ncmfw = new AddCMFWindow();
            ncmfw.ShowDialog();
        }


        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //alternatiive to autoupdate, could just turn into a doukutsu club link opener if I scrap the update feature.
            if (!string.IsNullOrWhiteSpace(doukutsuClubID))
            {
                Process.Start($"https://doukutsuclub.knack.com/database#my-mods/mod-details/{doukutsuClubID}");
            }
            else
            {
                Title = "Updating, please wait...";
                Thread UpdateThread = new Thread(new ThreadStart(UpdatePrompt));
                UpdateThread.Start();
            }
        }

        //TODO: Move these update functions to it's own file, maybe delete it given how bad it is lol
        //Also move some of these to a general function to access them from the doukutsu club solution
        private void UpdatePrompt()
        {
            //TODO: EWW!!!!!!
            Mod mod = new Mod(version, modName, doukutsuClubID, downloadLink, versionLink, doukutsuFile, configFile, filestoupdate, filestoadd, folder);
            bool versionfound = false;

            string newversion = "";
            string newupdatelog = "";
            if (!string.IsNullOrWhiteSpace(mod.VersionLink)) //if json doesnt exist, use xml instead for backwards compatibility
            {
                try
                {
                    XmlReader reader = XmlReader.Create(mod.VersionLink);
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            if (reader.Name == "versionno")
                            {
                                if (reader.Read())
                                {
                                    newversion = reader.Value.Trim();
                                    versionfound = true;
                                }
                            }
                            if (reader.Name == "updatelog")
                            {
                                if (reader.Read())
                                {
                                    newupdatelog = reader.Value.Trim();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    versionfound = false;
                }
            }

            if (!versionfound)
            {
                MessageBoxResult query = MessageBox.Show($"A link to a version file has not been given, or there is something wrong with the linked file containing the version. Do you wish to update anyway? Updates may take a while.\n\nMod source: {mod.DownloadLink}", "Mod Updater", MessageBoxButton.YesNo);
                if (query == MessageBoxResult.Yes)
                {
                    UpdateMod(mod);
                }
            }

            else
            {
                if (Convert.ToDouble(mod.Version) < Convert.ToDouble(newversion))
                {
                    MessageBoxResult query = MessageBox.Show($"A new version has been found: v{newversion}. The current version is v{mod.Version}.\nDo you wish to update? Updates may take a while.\n\nUpdate log: {newupdatelog}\nMod source: {mod.DownloadLink}", "Mod Updater", MessageBoxButton.YesNo);
                    if (query == MessageBoxResult.Yes)
                    {
                        UpdateMod(mod);
                    }
                }

                else
                {
                    MessageBox.Show("No new version found.", "Mod Updater");
                }
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                Title = "CSModLauncher";
                Modlist.SelectedIndex = 0;
            }));
        }

        public static void UpdateMod(Mod mod)
        {
            //TODO: Does this actually check if a mod's files are in a folder in a zip rather than just ifles in a zip??
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(mod.DownloadLink, $"{mod.Folder}/dl.temp.zip");
                }
            }
            catch
            {
                MessageBox.Show("Mod download failed.", "Error!");
                return;
            }
            Console.WriteLine("Extracting zip file...");

            ZipArchive zip = ZipFile.OpenRead($"{mod.Folder}/dl.temp.zip");
            IReadOnlyCollection<ZipArchiveEntry> entries = zip.Entries;
            string entry = entries.ToArray()[0].FullName.Replace("/", "");
            zip.Dispose();
            ZipFile.ExtractToDirectory($"{mod.Folder}/dl.temp.zip", mod.Folder);

            string newmodfolder = mod.ModName;
            if (Directory.Exists(entry))
            {
                newmodfolder = entry;
            }

            Console.WriteLine("Backing up old data...");
            Directory.CreateDirectory($"{mod.Folder}/old_backup");
            try { CopyDirectory($"{mod.Folder}/data", $"{mod.Folder}/old_backup/data"); }
            catch { Console.WriteLine("data folder not found."); }
            foreach (string file in mod.FilesToUpdate)
            {
                try { File.Copy(file, $"{mod.Folder}/old_backup/{file}"); }
                catch { Console.WriteLine($"\"{file}\" not found."); }
            }

            Console.WriteLine("Updating...");
            try { Directory.Delete($"{mod.Folder}/data", true); }
            catch { }
            foreach (string file in mod.FilesToUpdate)
            {
                try { File.Delete(file); }
                catch { }
            }

            try { CopyDirectory($"{mod.Folder}/{newmodfolder}/data", $"{mod.Folder}/data"); }
            catch { }
            foreach (string file in mod.FilesToUpdate)
            {
                try { File.Copy($"{mod.Folder}/{newmodfolder}/{file}", $"{mod.Folder}/{file}"); }
                catch { Console.WriteLine($"{file} has not been found, skipping..."); }
            }
            foreach (string file in mod.FilesToAdd)
            {
                if (!File.Exists($"{mod.Folder}/{file}")) { Console.WriteLine($"{file} not detected, adding..."); File.Copy($"{mod.Folder}/{newmodfolder}/{file}", $"{mod.Folder}/{file}"); }
            }

            File.Delete($"{mod.Folder}/{newmodfolder}.cmf");
            try { File.Copy(Directory.GetFiles($"{mod.Folder}/{newmodfolder}", "*.cmf")[0], $"{mod.Folder}/{newmodfolder}.cmf"); }
            catch { }

            Console.WriteLine("Cleaning up temporary files...");
            Directory.Delete($"{mod.Folder}/{newmodfolder}", true);
            Directory.Delete($"{mod.Folder}/old_backup", true);
            File.Delete($"{mod.Folder}/dl.temp.zip");

            MessageBox.Show("The update has been completed!", "Mod Updater");
            return;
        }

        public static void CopyDirectory(string sourcedirectory, string destination)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(sourcedirectory);
            DirectoryInfo[] directories = dirinfo.GetDirectories();
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            FileInfo[] files = dirinfo.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destination, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (DirectoryInfo subdirectory in directories)
            {
                string temppath = Path.Combine(destination, subdirectory.Name);
                CopyDirectory(subdirectory.FullName, temppath);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.W)
                {
                    Random rand = new Random();
                    waffles += Convert.ToUInt32(rand.Next(1, 6));
                    MessageBox.Show($"You have been blessed by the waffle lord, you now have {waffles} waffles", "Waffle");
                    if (waffles > 50 && !waffleshown)
                    {
                        waffleshown = true;
                        MessageBox.Show("If you're waiting to see a waffle then it's time to stop.", "Waffle");
                    }
                    if (waffles > 100)
                    {
                        MessageBox.Show("You should stop.", "Waffle");
                    }
                }
            }
        }
    }
}
