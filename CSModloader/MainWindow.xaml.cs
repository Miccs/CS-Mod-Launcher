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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using DataFormats = System.Windows.DataFormats;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

//TODO: Clean up all the comments, move some code to their own functions and files, cry at 6 year old code
//Also update maybe the .net version or see about options that might be friendlier to linux
//Also maybe do more crash prevention/handling
namespace CSModLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class NamePath
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    //either find an use for this or merge with NamePath
    public class ModListInfo
    {
        public string Name {  get; set; }
        public string Path {  get; set; }
    }

    public partial class MainWindow : Window
    {
        //TODO: just store this in a current mod object or something
        public static JSONMod _currentmod;
        public static List<ModListInfo> modlist = new List<ModListInfo>();

        public static Config _config = new Config();

        public static uint waffles = 0;
        public static bool waffleshown = false;

        public MainWindow()
        {
            //TODO: Search for JSON files instead (since that's what they are) and check their contents instead to validate if it's a modinfo file
            //Also something is going wrong where the current folder is getting loaded as well
            //ALSO just allow user to define their mods folder!!!!!!!!!!!
            var foundFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.cmf", SearchOption.AllDirectories);

            if (File.Exists("config.json"))
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string configJSON = File.ReadAllText("config.json");
                _config = serializer.Deserialize<Config>(configJSON);
            }
            else
            {
                //uh oh we got a new person here!
                MessageBoxResult result = MessageBox.Show("Would you like to set a default mods folder? This will allow drag&drop functionality.",
                                                          "Welcome!",
                                                          MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.Description = "Select a default folder...";
                        folderDialog.ShowNewFolderButton = true;
                        folderDialog.SelectedPath = Environment.CurrentDirectory;

                        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _config.ModsFolder = folderDialog.SelectedPath;
                        }
                    }
                }
                
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                File.WriteAllText("config.json", serializer.Serialize(_config));
            }

            InitializeComponent();

            Update_ModList();
        }

        private void Update_ModList()
        {
            foreach (var mod in _config.Mods) {
                Modlist.Items.Add(new ListBoxItem() { Content = mod.Name, ToolTip = mod.Path });
            }
        }

        private void Add_Mod(ModListInfo mod)
        {
            _config.Mods.Add(mod);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            File.WriteAllText("config.json", serializer.Serialize(_config));

            Update_ModList();
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            //Add folder support, also move everything to allow for other ways to add something
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (string.IsNullOrWhiteSpace(_config.ModsFolder))
                {
                    MessageBox.Show("You have not yet selected a default mod folder, you can select one in (Placeholder)",
                                    "CSModLoader", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                var fileType = Path.GetExtension(filePath.ToLower());

                if (fileType == ".zip")
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var folderPath = Path.Combine(_config.ModsFolder, fileName);

                    if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

                    //ZipFile.ExtractToDirectory(filePath, folderPath);
                    var archive = ZipFile.OpenRead(filePath);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var entrypath = Path.Combine(folderPath, entry.FullName);

                        if (entrypath.EndsWith("\\") || entrypath.EndsWith("/"))
                        {
                            if (!Directory.Exists(entrypath)) { Directory.CreateDirectory(entrypath); }
                        }
                        else { entry.ExtractToFile(entrypath, true); }
                    }

                    var gameFileDialog = new OpenFileDialog()
                    {
                        Filter = "Game executable (.exe)|*.exe",
                        InitialDirectory = folderPath,
                        Title = "Select the game executable",
                    };

                    var result = gameFileDialog.ShowDialog();


                    var mod = new JSONMod()
                    {
                        Name = fileName,
                        Filepath = folderPath,
                        Files = new JSONModFiles() { DoukutsuFile = result == true ? gameFileDialog.FileName : "" },

                    };

                    var serializer = new JavaScriptSerializer();
                    var modJSON = serializer.Serialize(mod);
                    File.WriteAllText(folderPath + "\\modInfo.json", modJSON);

                    Add_Mod(new ModListInfo() { Name = fileName, Path = folderPath });
                }
            }
        }

        private void Modlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)Modlist.SelectedItem;
            var path = Path.Combine((string)item.ToolTip, "modInfo.json");

            _currentmod = GetInfo(path);


            ModTitle.Text = _currentmod.Name;
            ModInfo.Text = _currentmod.Description;
            AuthorBox.Text = $"Made by: {_currentmod.Author}";
            PlayButton.IsEnabled = true;
            ConfigButton.IsEnabled = true;

            PlayButton.IsEnabled = File.Exists(_currentmod.Files.DoukutsuFile);

            if (_currentmod.Name == "{ERROR:NULL}")
            {
                ModInfo.Text = "Something has gone wrong while reading the modInfo.json file, is it's syntax correct or the file missing?";
                VersionBox.Text = "";
                AuthorBox.Text = "";
                ModTitle.Text = "CSModLauncher";
                PlayButton.IsEnabled = false;
                UpdateButton.IsEnabled = false;
            }
        }


        private JSONMod GetInfo(string filepath)
        {
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string cmf = File.ReadAllText(filepath);
                return serializer.Deserialize<JSONMod>(cmf);
            }
            catch
            {
                return new JSONMod() { Name = "{ERROR:NULL}" };
            }
        }

        //???
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Thread PlayThread = new Thread(new ThreadStart(Play));
            PlayThread.Start();
        }


        private void Play()
        {
            Process.Start(_currentmod.Files.DoukutsuFile);
        }

        private void Exit()
        {
            Thread ExitThread = new Thread(new ThreadStart(Exit));
            Close();
        }


        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (!string.IsNullOrWhiteSpace(_currentmod.Files.ConfigFile))
            {
                Process.Start(_currentmod.Files.ConfigFile);
            }
            else
            {
                ConfigWindow fd = new ConfigWindow();
                fd.ShowDialog();
            }
            //string configlocation = $"{folder}\\doconfig.exe";
            //Process.Start(configlocation);*/
        }


        //Just turn this into an open folder dialogue to add existing folders
        private void CreateNewCmf_Click(object sender, RoutedEventArgs e)
        {
            AddCMFWindow ncmfw = new AddCMFWindow();
            ncmfw.ShowDialog();
        }


        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //alternatiive to autoupdate, could just turn into a doukutsu club link opener if I scrap the update feature.
            //if (!string.IsNullOrWhiteSpace(doukutsuClubID))
            //{
                //Process.Start($"https://doukutsuclub.knack.com/database#my-mods/mod-details/{doukutsuClubID}");
            //}
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
