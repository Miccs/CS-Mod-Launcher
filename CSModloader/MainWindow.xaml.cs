using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

using DataFormats = System.Windows.DataFormats;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

//TODO: Clean up all the comments, move some code to their own functions and files, cry at 6 year old code
//Also update maybe the .net version or see about options that might be friendlier to linux
//Also maybe do more crash prevention/handling
//Config option to automatically delete zip archives after installing the mod.
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

    public class ModListInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int ID { get; set; }
    }

    public partial class MainWindow : Window
    {
        //TODO: just store this in a current mod object or something
        public static JSONMod _currentmod;
        public static int _currentmodID;
        public static List<ModListInfo> modlist = new List<ModListInfo>();

        public static Config _config = new Config();

        public static uint waffles = 0;
        public static bool waffleshown = false;

        public MainWindow()
        {
            InitializeComponent();
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
                    Select_folder();
                }

                Update_Config();
            }
            Modlist.Focus();

            Update_ModList();
        }

        private void Select_folder()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a default folder...";
                folderDialog.ShowNewFolderButton = true;
                folderDialog.SelectedPath = Environment.CurrentDirectory;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _config.ModsFolder = folderDialog.SelectedPath;

                    DetectMods(folderDialog.SelectedPath);
                }
            }
        }

        private void Update_Config()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            File.WriteAllText("config.json", serializer.Serialize(_config));
        }

        private int Get_New_ID()
        {
            return (_config.Mods.Count > 0 ? _config.Mods.OrderByDescending(m => m.ID).FirstOrDefault().ID + 1 : 0);
        }

        private void Update_ModList()
        {
            Modlist.Items.Clear();
            foreach (var mod in _config.Mods)
            {
                Modlist.Items.Add(new ListBoxItem() { Content = mod.Name, ToolTip = mod.Path });
            }
        }

        private void Add_Mod(ModListInfo mod)
        {
            if (!_config.Mods.Contains(mod))
            {
                _config.Mods.Add(mod);

                Update_Config();
            }
        }

        private void Add_Mod(JSONMod mod, string path)
        {
            if (!_config.Mods.Any(m => m.Path == path))
            {
                _config.Mods.Add(new ModListInfo() { Name = mod.Name, Path = path, ID = Get_New_ID() });

                Update_Config();
            }
        }

        private void Write_ModInfo(JSONMod mod, string path)
        {
            var serializer = new JavaScriptSerializer();
            var modJSON = serializer.Serialize(mod);
            File.WriteAllText(path + "\\ModInfo.json", modJSON);
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            //Add folder support, see if the extracting can be done elsewhere as well so an open zip option can be added
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (string.IsNullOrWhiteSpace(_config.ModsFolder) || !Directory.Exists(_config.ModsFolder))
                {
                    MessageBox.Show("Default mod folder not found, you can select one in File>Set mod folder...",
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

                    if (Directory.EnumerateFiles(folderPath).Count() < 1)
                    {
                        var oldSource = Directory.GetDirectories(folderPath).First();
                        var oldSourceName = Path.Combine(_config.ModsFolder, Path.GetFileName(oldSource));
                        var oldDirectory = folderPath;
                        var oldDirectoryName = Path.GetFileName(oldDirectory);
                        folderPath = oldSource.Replace(oldDirectoryName + "\\", "");

                        Directory.Move(oldSource, oldSourceName);
                        Directory.Delete(oldDirectory);
                    }

                    Create_Mod(Path.GetFileName(folderPath), folderPath);
                }
            }
        }

        private void Create_Mod(string file, string path)
        {
            var gameFileDialog = new OpenFileDialog()
            {
                Filter = "Game executable (.exe)|*.exe",
                InitialDirectory = path,
                Title = "Select the game executable",
            };

            var result = gameFileDialog.ShowDialog();
            var doukutsuPath = "";
            if (result == true) 
            {
                doukutsuPath = gameFileDialog.FileName;
                if (doukutsuPath.Contains(path)) { doukutsuPath = doukutsuPath.Replace(path + "\\", ""); }
                else { doukutsuPath = ""; }
            }

            var mod = new JSONMod()
            {
                Name = file,
                Files = new JSONModFiles() { DoukutsuFile = result == true ? doukutsuPath : "" },

            };

            if (!File.Exists(path + "\\ModInfo.json"))
            {
                Write_ModInfo(mod, path);
            }
            else
            {
                mod = GetInfo(path + "\\ModInfo.json");

            }

            Add_Mod(new ModListInfo() { Name = mod.Name, Path = path, ID = Get_New_ID() });
            Update_ModList();
        }

        private void Modlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Modlist.SelectedItem is null) return;
            ListBoxItem item = (ListBoxItem)Modlist.SelectedItem;
            var path = Path.Combine((string)item.ToolTip, "ModInfo.json");

            _currentmod = GetInfo(path);
            _currentmodID = _config.Mods.First(m => m.Path == (string)item.ToolTip).ID;


            if (_currentmod.Name == "{ERROR:NULL}")
            {
                ModInfo.Text = "Something has gone wrong while reading the ModInfo.json file, is it's syntax correct or the file missing?";
                VersionBox.Text = "";
                AuthorBox.Text = "";
                ModTitle.Text = "CSModLauncher";
                PlayButton.IsEnabled = false;
                ConfigButton.IsEnabled = false;
                EditButton.IsEnabled = false;
            }

            else
            {
                ModTitle.Text = _currentmod.Name;
                ModInfo.Text = _currentmod.Description;
                AuthorBox.Text = $"Made by: {_currentmod.Author}";
                PlayButton.IsEnabled = !string.IsNullOrWhiteSpace(_currentmod.Files.DoukutsuFile);
                ConfigButton.IsEnabled = true;
                EditButton.IsEnabled = true;

                PlayButton.IsEnabled = File.Exists(Path.Combine((string)item.ToolTip, _currentmod.Files.DoukutsuFile));
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Play()
        {
            var path = _config.Get_Mod_Path_By_ID(_currentmodID, _currentmod.Files.DoukutsuFile);
            if (!File.Exists(path)) return;
            Process.Start(path);
        }


        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(_currentmod.Files.ConfigFile))
            {
                var path = _config.Get_Mod_Path_By_ID(_currentmodID, _currentmod.Files.ConfigFile);
                if (File.Exists(path))
                {
                    Process.Start(path);
                    return;
                }
            }
            var configPath = Path.Combine(_config.Get_Mod_Path_By_ID(_currentmodID), "config.dat");
            ConfigWindow fd = new ConfigWindow(configPath);
            fd.ShowDialog();
            //string configlocation = $"{folder}\\doconfig.exe";
            //Process.Start(configlocation);*/
        }


        private void AddMod_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select mod's root folder...";
                folderDialog.SelectedPath = Environment.CurrentDirectory;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var modInfo = Directory.GetFiles(folderDialog.SelectedPath, "*ModInfo.json", SearchOption.TopDirectoryOnly);
                    if (modInfo.Length > 0)
                    {
                        Add_Mod(GetInfo(modInfo[0]), folderDialog.SelectedPath);
                    }

                    else
                    {
                        var folderName = new DirectoryInfo(folderDialog.SelectedPath).Name;
                        Create_Mod(folderName, folderDialog.SelectedPath);
                    }

                    Update_ModList();
                }
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditModDialog emd = new EditModDialog();
            emd.Field_ModName.Text = _currentmod.Name;
            emd.Field_Author.Text = _currentmod.Author;
            emd.Field_Description.Text = _currentmod.Description;
            emd.Field_DoukutsuFile.Text = _currentmod.Files.DoukutsuFile;
            emd.Field_ConfigFile.Text = _currentmod.Files.ConfigFile;

            if (emd.ShowDialog() == true)
            {
                _currentmod.Name = emd.Field_ModName.Text;
                _currentmod.Author = emd.Field_Author.Text;
                _currentmod.Description = emd.Field_Description.Text;
                _currentmod.Files.DoukutsuFile = emd.Field_DoukutsuFile.Text;
                _currentmod.Files.ConfigFile = emd.Field_ConfigFile.Text;

                var modInfo = _config.Mods.First(m => m.ID == _currentmodID);
                modInfo.Name = _currentmod.Name;
                Write_ModInfo(_currentmod, modInfo.Path);
                Update_Config();
                Update_ModList();
                Modlist_SelectionChanged(null, null);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Select zip file, extract to mod folder. Helpful when downloads have a duplicate name suffix (ie 'name (2)') or add the version number to the file.
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            Select_folder();
            Update_Config();
        }

        private void Detect_Click(object sender, RoutedEventArgs e)
        {
            DetectMods(_config.ModsFolder);
        }

        private void DetectModSelect_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select folder to search in...";
                folderDialog.SelectedPath = Environment.CurrentDirectory;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DetectMods(folderDialog.SelectedPath);
                }
            }
        }

        private void DetectMods(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*ModInfo.json", SearchOption.AllDirectories))
            {
                if (!_config.Mods.Any(m => m.Path == file))
                {
                    Add_Mod(GetInfo(file), file.ToLower().Replace("modinfo.json", ""));
                }
            }

            Update_ModList();
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

        private void GitHubLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Miccs/CS-Mod-Launcher/");
        }

        private void CSTSFLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://forum.cavestory.org/threads/14368/");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cave Story Mod Launcher.\nMade with love by Mint/MintiFreshFox\n2018-2024",
                            "CSModLoader",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
