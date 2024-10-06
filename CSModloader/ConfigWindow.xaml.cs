using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;

namespace CSModLauncher
{
    //I'm gonna be so sorry for anybody who sees this.
    //TODO: Actually finish the thingn and doublecheck that i didn't miss anything in the separate release, this is based on the standalone 


    public partial class ConfigWindow : Window
    {
        public static string config = ""; //The config file
        public string path; //The path to the file
        public static string font; //The currently selected font, easier to save as an variable than to get it from the sample box's font



        public ConfigWindow()
        {
            //path = $"{MainWindow._currentmod.Filepath}\\config.dat"; //The default path; a config file at the same location as the exe
            /*if (args.Length > 0)
            {
                path = args[0];
            }*/
            //string[] config = new string[0]; //old code for drag and drop support
            bool fail = false;
            InitializeComponent();
            try
            {
                if (File.Exists(path))
                {
                    config = File.ReadAllLines(path)[0];
                }
                else { SetDefault(); }
            }
            catch (FileNotFoundException)
            {
                SetDefault(); //should we have failed loading a nearby config, set all the elements to a default and generate a new config
                /*fail = true;    
                Console.WriteLine("Config not found.");
                OpenFileDialog fd = new OpenFileDialog
                {
                    Filter = "Cave Story Config files (Config.Dat)|Config.Dat|All files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var f = fd.OpenFile();
                    path = fd.FileName;
                    config = File.ReadAllLines(path);
                    fail = false;
                }*/
            }




            if (!fail) // should there be a file...
            {
                LoadAndSet(); //...load the file and set all the elements
            }
        }

        private void FontSetting_Click(object sender, RoutedEventArgs e)
        {
            //FontDialog fd = new FontDialog(); //used default fontdialog before this, had formatting and size settings which is obviously not needed here.
            FontDialog fd = new FontDialog(); //At least i can change what's on this one and make it more userfriendly.
            if (fd.ShowDialog() == true)
            {
                font = fd.FontBox.Text; //We take the fontname from the textbox, that's why changing the combobox also sets the textbox.
                FontSample.FontFamily = new FontFamily(fd.FontBox.Text);
            }
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e) //I don't need to explain this do i?
        {
            SetDefault();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) //Save all changes and close the window.
        {
            config = config.Remove(32, 68).Insert(32, "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0");
            config = config.Remove(32, font.Length).Insert(32, font); //remove the font name's length in characters starting from the config's font offset and replace it with the selected font name.
            //for that matter everything replaces things like this.
            {
                char c = '\u0000';
                if (UsingGamepad.IsChecked == true)
                {
                    c = '\u0001';
                }
                config = config.Remove(112, 1).Insert(112, Convert.ToString(c));
            }//check if the gamepad is enabled

            char one = '\u0001';
            char two = '\u0002';
            char three = '\u0003';
            char four = '\u0004';
            char five = '\u0005';
            char six = '\u0006';
            {
                {
                    if (Jump1.IsChecked == true)
                    {
                        config = config.Remove(116, 1).Insert(116, Convert.ToString(one));
                    }
                    else if (Attack1.IsChecked == true)
                    {
                        config = config.Remove(116, 1).Insert(116, Convert.ToString(two));
                    }
                    else if (WepPlus1.IsChecked == true)
                    {
                        config = config.Remove(116, 1).Insert(116, Convert.ToString(three));
                    }
                    else if (Item1.IsChecked == true)
                    {
                        config = config.Remove(116, 1).Insert(116, Convert.ToString(four));
                    }
                    else if (Map1.IsChecked == true)
                    {
                        config = config.Remove(116, 1).Insert(116, Convert.ToString(five));
                    }
                    else if (WepMin1.IsChecked == true)
                    {
                        config = config.Remove(116, 1).Insert(116, Convert.ToString(six));
                    }
                }
                {
                    if (Jump2.IsChecked == true)
                    {
                        config = config.Remove(120, 1).Insert(120, Convert.ToString(one));
                    }
                    else if (Attack2.IsChecked == true)
                    {
                        config = config.Remove(120, 1).Insert(120, Convert.ToString(two));
                    }
                    else if (WepPlus2.IsChecked == true)
                    {
                        config = config.Remove(120, 1).Insert(120, Convert.ToString(three));
                    }
                    else if (Item2.IsChecked == true)
                    {
                        config = config.Remove(120, 1).Insert(120, Convert.ToString(four));
                    }
                    else if (Map2.IsChecked == true)
                    {
                        config = config.Remove(120, 1).Insert(120, Convert.ToString(five));
                    }
                    else if (WepMin2.IsChecked == true)
                    {
                        config = config.Remove(120, 1).Insert(120, Convert.ToString(six));
                    }
                }
                {
                    if (Jump3.IsChecked == true)
                    {
                        config = config.Remove(124, 1).Insert(124, Convert.ToString(one));
                    }
                    else if (Attack3.IsChecked == true)
                    {
                        config = config.Remove(124, 1).Insert(124, Convert.ToString(two));
                    }
                    else if (WepPlus3.IsChecked == true)
                    {
                        config = config.Remove(124, 1).Insert(124, Convert.ToString(three));
                    }
                    else if (Item3.IsChecked == true)
                    {
                        config = config.Remove(124, 1).Insert(124, Convert.ToString(four));
                    }
                    else if (Map3.IsChecked == true)
                    {
                        config = config.Remove(124, 1).Insert(124, Convert.ToString(five));
                    }
                    else if (WepMin3.IsChecked == true)
                    {
                        config = config.Remove(124, 1).Insert(124, Convert.ToString(six));
                    }
                }
                {
                    if (Jump4.IsChecked == true)
                    {
                        config = config.Remove(128, 1).Insert(128, Convert.ToString(one));
                    }
                    else if (Attack4.IsChecked == true)
                    {
                        config = config.Remove(128, 1).Insert(128, Convert.ToString(two));
                    }
                    else if (WepPlus4.IsChecked == true)
                    {
                        config = config.Remove(128, 1).Insert(128, Convert.ToString(three));
                    }
                    else if (Item4.IsChecked == true)
                    {
                        config = config.Remove(128, 1).Insert(128, Convert.ToString(four));
                    }
                    else if (Map4.IsChecked == true)
                    {
                        config = config.Remove(128, 1).Insert(128, Convert.ToString(five));
                    }
                    else if (WepMin4.IsChecked == true)
                    {
                        config = config.Remove(128, 1).Insert(128, Convert.ToString(six));
                    }
                }
                {
                    if (Jump5.IsChecked == true)
                    {
                        config = config.Remove(132, 1).Insert(132, Convert.ToString(one));
                    }
                    else if (Attack5.IsChecked == true)
                    {
                        config = config.Remove(132, 1).Insert(132, Convert.ToString(two));
                    }
                    else if (WepPlus5.IsChecked == true)
                    {
                        config = config.Remove(132, 1).Insert(132, Convert.ToString(three));
                    }
                    else if (Item5.IsChecked == true)
                    {
                        config = config.Remove(132, 1).Insert(132, Convert.ToString(four));
                    }
                    else if (Map5.IsChecked == true)
                    {
                        config = config.Remove(132, 1).Insert(132, Convert.ToString(five));
                    }
                    else if (WepMin5.IsChecked == true)
                    {
                        config = config.Remove(132, 1).Insert(132, Convert.ToString(six));
                    }
                }
                {
                    if (Jump6.IsChecked == true)
                    {
                        config = config.Remove(136, 1).Insert(136, Convert.ToString(one));
                    }
                    else if (Attack6.IsChecked == true)
                    {
                        config = config.Remove(136, 1).Insert(136, Convert.ToString(two));
                    }
                    else if (WepPlus6.IsChecked == true)
                    {
                        config = config.Remove(136, 1).Insert(136, Convert.ToString(three));
                    }
                    else if (Item6.IsChecked == true)
                    {
                        config = config.Remove(136, 1).Insert(136, Convert.ToString(four));
                    }
                    else if (Map6.IsChecked == true)
                    {
                        config = config.Remove(136, 1).Insert(136, Convert.ToString(five));
                    }
                    else if (WepMin6.IsChecked == true)
                    {
                        config = config.Remove(136, 1).Insert(136, Convert.ToString(six));
                    }
                }
                {
                    if (Jump7.IsChecked == true)
                    {
                        config = config.Remove(140, 1).Insert(140, Convert.ToString(one));
                    }
                    else if (Attack7.IsChecked == true)
                    {
                        config = config.Remove(140, 1).Insert(140, Convert.ToString(two));
                    }
                    else if (WepPlus7.IsChecked == true)
                    {
                        config = config.Remove(140, 1).Insert(140, Convert.ToString(three));
                    }
                    else if (Item7.IsChecked == true)
                    {
                        config = config.Remove(140, 1).Insert(140, Convert.ToString(four));
                    }
                    else if (Map7.IsChecked == true)
                    {
                        config = config.Remove(140, 1).Insert(140, Convert.ToString(five));
                    }
                    else if (WepMin7.IsChecked == true)
                    {
                        config = config.Remove(140, 1).Insert(140, Convert.ToString(six));
                    }
                }
                {
                    if (Jump8.IsChecked == true)
                    {
                        config = config.Remove(144, 1).Insert(144, Convert.ToString(one));
                    }
                    else if (Attack8.IsChecked == true)
                    {
                        config = config.Remove(144, 1).Insert(144, Convert.ToString(two));
                    }
                    else if (WepPlus8.IsChecked == true)
                    {
                        config = config.Remove(144, 1).Insert(144, Convert.ToString(three));
                    }
                    else if (Item8.IsChecked == true)
                    {
                        config = config.Remove(144, 1).Insert(144, Convert.ToString(four));
                    }
                    else if (Map8.IsChecked == true)
                    {
                        config = config.Remove(144, 1).Insert(144, Convert.ToString(five));
                    }
                    else if (WepMin8.IsChecked == true)
                    {
                        config = config.Remove(144, 1).Insert(144, Convert.ToString(six));
                    }
                }
            } //big ouchy stuff about checking what radiobutton was checked.

            {
                char c = '\u0000';
                switch (WindowSetting.SelectedIndex)
                {
                    case 1:
                        c = '\u0001';
                        break;
                    case 2:
                        c = '\u0002';
                        break;
                    case 3:
                        c = '\u0003';
                        break;
                    case 4:
                        c = '\u0004';
                        break;
                    default:
                        break;
                }
                config = config.Remove(108, 1).Insert(108, Convert.ToString(c));
            } //less ouchy stuff about the window setting


            File.WriteAllLines(path, new[] { config });//save this mess
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }






        private void SetDefault()
        {
            Attack1.IsChecked = true;
            Jump2.IsChecked = true;
            Map3.IsChecked = true;
            WepMin4.IsChecked = true;
            WepPlus5.IsChecked = true;
            Item6.IsChecked = true;
            WepMin7.IsChecked = true;
            WepPlus8.IsChecked = true;
            UsingGamepad.IsChecked = false;
            font = "Courier New";
            FontSample.FontFamily = new FontFamily("Courier New");
            WindowSetting.SelectedIndex = 0;

            config = $"DOUKUTSU20041206"; //interestingly, this is needed in order for cave story to work. Otherwise it'll just load as if no config file was present.
            char nothing = '\u0000';
            //147
            for (int i = 0; i < 132; i++)
            {
                config += nothing;
            } //messy, but fills the to-be-saved config file with a bunch of zeros
        }

        private void UsingGamepad_Checked(object sender, RoutedEventArgs e)
        {
            FindRadios(true);
        }

        private void UsingGamepad_Unchecked(object sender, RoutedEventArgs e)
        {
            FindRadios(false);
        }

        public void FindRadios(bool onoff)
        {
            foreach (UIElement radio in RadioGrid1.Children)
            {
                radio.IsEnabled = onoff;
            }
            foreach (UIElement radio in RadioGrid2.Children)
            {
                radio.IsEnabled = onoff;
            }
            foreach (UIElement radio in RadioGrid3.Children)
            {
                radio.IsEnabled = onoff;
            }
            foreach (UIElement radio in RadioGrid4.Children)
            {
                radio.IsEnabled = onoff;
            }
            foreach (UIElement radio in RadioGrid5.Children)
            {
                radio.IsEnabled = onoff;
            }
            foreach (UIElement radio in RadioGrid6.Children)
            {
                radio.IsEnabled = onoff;
            }
            foreach (UIElement radio in RadioGrid7.Children)
            {
                radio.IsEnabled = onoff;
            }
            foreach (UIElement radio in RadioGrid8.Children)
            {
                radio.IsEnabled = onoff;
            }
        }

        public void LoadAndSet()
        {
            Console.WriteLine("Config found, updating window.");
            //Update all elements.
            if (Convert.ToByte(Convert.ToChar(config.Substring(112, 1))) == 1)
            {
                UsingGamepad.IsChecked = true;
            }//Uses Gamepad
            {
                switch (Convert.ToByte(Convert.ToChar(config.Substring(116, 1))))
                {
                    case 1:
                        Jump1.IsChecked = true;
                        break;
                    case 2:
                        Attack1.IsChecked = true;
                        break;
                    case 3:
                        WepPlus1.IsChecked = true;
                        break;
                    case 4:
                        Item1.IsChecked = true;
                        break;
                    case 5:
                        Map1.IsChecked = true;
                        break;
                    case 6:
                        WepMin1.IsChecked = true;
                        break;
                    default:
                        Jump1.IsChecked = true;
                        break;
                }
                switch (Convert.ToByte(Convert.ToChar(config.Substring(120, 1))))
                {
                    case 1:
                        Jump2.IsChecked = true;
                        break;
                    case 2:
                        Attack2.IsChecked = true;
                        break;
                    case 3:
                        WepPlus2.IsChecked = true;
                        break;
                    case 4:
                        Item2.IsChecked = true;
                        break;
                    case 5:
                        Map2.IsChecked = true;
                        break;
                    case 6:
                        WepMin2.IsChecked = true;
                        break;
                    default:
                        Jump2.IsChecked = true;
                        break;
                }
                switch (Convert.ToByte(Convert.ToChar(config.Substring(124, 1))))
                {
                    case 1:
                        Jump3.IsChecked = true;
                        break;
                    case 2:
                        Attack3.IsChecked = true;
                        break;
                    case 3:
                        WepPlus3.IsChecked = true;
                        break;
                    case 4:
                        Item3.IsChecked = true;
                        break;
                    case 5:
                        Map3.IsChecked = true;
                        break;
                    case 6:
                        WepMin3.IsChecked = true;
                        break;
                    default:
                        Jump3.IsChecked = true;
                        break;
                }
                switch (Convert.ToByte(Convert.ToChar(config.Substring(128, 1))))
                {
                    case 1:
                        Jump4.IsChecked = true;
                        break;
                    case 2:
                        Attack4.IsChecked = true;
                        break;
                    case 3:
                        WepPlus4.IsChecked = true;
                        break;
                    case 4:
                        Item4.IsChecked = true;
                        break;
                    case 5:
                        Map4.IsChecked = true;
                        break;
                    case 6:
                        WepMin4.IsChecked = true;
                        break;
                    default:
                        Jump4.IsChecked = true;
                        break;
                }
                switch (Convert.ToByte(Convert.ToChar(config.Substring(132, 1))))
                {
                    case 1:
                        Jump5.IsChecked = true;
                        break;
                    case 2:
                        Attack5.IsChecked = true;
                        break;
                    case 3:
                        WepPlus5.IsChecked = true;
                        break;
                    case 4:
                        Item5.IsChecked = true;
                        break;
                    case 5:
                        Map5.IsChecked = true;
                        break;
                    case 6:
                        WepMin5.IsChecked = true;
                        break;
                    default:
                        Jump5.IsChecked = true;
                        break;
                }
                switch (Convert.ToByte(Convert.ToChar(config.Substring(136, 1))))
                {
                    case 1:
                        Jump6.IsChecked = true;
                        break;
                    case 2:
                        Attack6.IsChecked = true;
                        break;
                    case 3:
                        WepPlus6.IsChecked = true;
                        break;
                    case 4:
                        Item6.IsChecked = true;
                        break;
                    case 5:
                        Map6.IsChecked = true;
                        break;
                    case 6:
                        WepMin6.IsChecked = true;
                        break;
                    default:
                        Jump6.IsChecked = true;
                        break;
                }
                switch (Convert.ToByte(Convert.ToChar(config.Substring(140, 1))))
                {
                    case 1:
                        Jump7.IsChecked = true;
                        break;
                    case 2:
                        Attack7.IsChecked = true;
                        break;
                    case 3:
                        WepPlus7.IsChecked = true;
                        break;
                    case 4:
                        Item7.IsChecked = true;
                        break;
                    case 5:
                        Map7.IsChecked = true;
                        break;
                    case 6:
                        WepMin7.IsChecked = true;
                        break;
                    default:
                        Jump7.IsChecked = true;
                        break;
                }
                switch (Convert.ToByte(Convert.ToChar(config.Substring(144, 1))))
                {
                    case 1:
                        Jump8.IsChecked = true;
                        break;
                    case 2:
                        Attack8.IsChecked = true;
                        break;
                    case 3:
                        WepPlus8.IsChecked = true;
                        break;
                    case 4:
                        Item8.IsChecked = true;
                        break;
                    case 5:
                        Map8.IsChecked = true;
                        break;
                    case 6:
                        WepMin8.IsChecked = true;
                        break;
                    default:
                        Jump8.IsChecked = true;
                        break;
                }
            }//radiobuttons
             //WindowSetting.SelectedIndex = Convert.ToInt16(config.Substring(108, 1));
            switch (Convert.ToChar(config.Substring(108, 1)))
            {
                case '\u0000':
                    WindowSetting.SelectedIndex = 0;
                    break;

                case '\u0001':
                    WindowSetting.SelectedIndex = 1;
                    break;

                case '\u0002':
                    WindowSetting.SelectedIndex = 2;
                    break;

                case '\u0003':
                    WindowSetting.SelectedIndex = 3;
                    break;

                case '\u0004':
                    WindowSetting.SelectedIndex = 4;
                    break;
                default:
                    WindowSetting.SelectedIndex = 0;
                    break;
            }
            font = config.Substring(32, 68);
            FontSample.FontFamily = new FontFamily(font);
        }
    }
}