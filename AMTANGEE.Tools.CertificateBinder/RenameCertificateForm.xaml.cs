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

namespace AMTANGEE.Tools.CertificateBinder
{
    /// <summary>
    /// Interaction logic for RenameCertificateForm.xaml
    /// </summary>
    public partial class RenameCertificateForm : Window
    {
        public X509Certificate2 Certificate { get; set; }


        public RenameCertificateForm(X509Certificate2 certificate)
        {
            InitializeComponent();

            Certificate = certificate;
            TbFriendlyName.Text = Certificate.FriendlyName;
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (Certificate.FriendlyName.Trim() == TbFriendlyName.Text.Trim())
            {
                BtnOk.IsEnabled = false;
                return;
            }

            Certificate.FriendlyName = TbFriendlyName.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void TbFriendlyName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            BtnOk.IsEnabled = TbFriendlyName.Text.Trim() != Certificate.FriendlyName.Trim();
        }
    }
}
