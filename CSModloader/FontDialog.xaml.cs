using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSModLauncher
{
    /// <summary>
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window
    {
        public FontDialog()
        {
            InitializeComponent();
        }

        public bool first = true; //used to avoid a crash when the dialog is made

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FontBox.Focus();
        }

        private void FontList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!first)
            {
                FontBox.Text = FontList.SelectedItem.ToString(); //get the selection and set it to the textbox
            }
            else
            {
                first = false;
            }
        }
    }
}
