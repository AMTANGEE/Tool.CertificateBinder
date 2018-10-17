using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace AMTANGEE.Tools.CertificateBinder
{
    /// <summary>
    /// Interaction logic for AddCertificateForm.xaml
    /// </summary>
    public partial class AddCertificateForm : Window
    {
        public X509Certificate2 Certificate = null;

        public AddCertificateForm()
        {
            InitializeComponent();
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyUiChanges();
            if(!BtnAdd.IsEnabled)
                return;

            try
            {
                Certificate = new X509Certificate2(TbPfxPath.Text, TbPassword.SecurePassword, X509KeyStorageFlags.PersistKeySet);
                DialogResult = true;
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                DialogResult = BtnAdd.IsEnabled = false;
            }
        }

        private void BtnChooseFile_OnClick(object sender, RoutedEventArgs e)
        {
            var fbd = new OpenFileDialog()
            {
                Filter = "PFX-Datei (*.pfx) | *.pfx",
                Multiselect = false
            };

            if((fbd.ShowDialog()??false) == false)
                return;

            TbPfxPath.Text = fbd.FileName;
        }

        private void TbPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ApplyUiChanges();
        }

        public void ApplyUiChanges()
        {
            BtnAdd.IsEnabled = (!string.IsNullOrWhiteSpace(TbPfxPath.Text) && System.IO.File.Exists(TbPfxPath.Text) &&
                                !string.IsNullOrWhiteSpace(TbPassword.Password));
        }
    }
}
