using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMTANGEE.Tools.CertificateBinder
{
    /// <summary>
    /// Interaction logic for SslSettingsControl.xaml
    /// </summary>
    public partial class SslSettingsControl : UserControl, ISettings
    {
        public ServiceDetails Service { get; private set; }
        public List<CertDetails> Certs { get; private set; }
        public CertDetails SelectedCert { get => (CertDetails)ComboBoxCertificate.SelectedItem; }
        public bool SelectedCertIsDifferent
        {
            get
            {
                if (SelectedCert != null)
                    return SelectedCert.Thumbprint != oldHash;
                else
                    return false;
            }
        }
        public bool HasChanged
        {
            get
            {
                return !(
                    CheckBox1.IsChecked == ((string)_settings["UseHttp"] == "1") &&
                    TbPort.Text == (string)_settings["HttpPort"] &&
                    CheckBox2.IsChecked == ((string)_settings["UseHttps"] == "1") &&
                    TbPortSsl.Text == (string)_settings["HttpsPort"]
                );
            }
        }

        private System.Collections.Hashtable _settings;
        private string oldHash;

        public SslSettingsControl()
        {
            InitializeComponent();
        }

        public SslSettingsControl(ServiceDetails serviceDetails)
        {
            InitializeComponent();

            SetService(serviceDetails);
        }

        public string GetCurrentCertificateThumbprint()
        {
            var output = new List<string>();
            var psi = new System.Diagnostics.ProcessStartInfo() { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true };
            psi.FileName = "netsh";

            psi.Arguments = "http show sslcert ipport=0.0.0.0:" + _settings["HttpsPort"];
            var procShow = System.Diagnostics.Process.Start(psi);
            while (procShow != null && !procShow.StandardOutput.EndOfStream)
            {
                var line = procShow.StandardOutput.ReadLine().Replace(" ","");
                if (line.StartsWith("Zertifikathash"))
                    return line.Split(':').Last();
            }

            return null;
        }

        public void SetService(ServiceDetails serviceDetails)
        {
            Service = serviceDetails;
            _settings = Service.GetSettings();

            /*
                <add key="UseHttp" value="1" />
                <add key="HttpPort" value="4006" />
                <add key="UseHttps" value="1" />
                <add key="HttpsPort" value="4007" />
            */

            CheckBox1.IsChecked = (string)_settings["UseHttp"] == "1";
            TbPort.Text = (string)_settings["HttpPort"];
            CheckBox2.IsChecked = (string)_settings["UseHttps"] == "1";
            TbPortSsl.Text = (string)_settings["HttpsPort"];

            oldHash = GetCurrentCertificateThumbprint();
        }

        public bool SaveToSettings()
        {
            _settings["UseHttp"] = (CheckBox1.IsChecked ?? false) ? "1" : "0";
            _settings["HttpPort"] = TbPort.Text;
            _settings["UseHttps"] = (CheckBox2.IsChecked ?? false) ? "1" : "0";
            _settings["HttpsPort"] = TbPortSsl.Text;

            return Service.SetSettings(_settings);
        }

        public void SetComboBoxSource(List<CertDetails> certDetails)
        {
            Certs = certDetails;
            ComboBoxCertificate.ItemsSource = certDetails;

            if (string.IsNullOrEmpty(oldHash))
                return;
            var cert = certDetails.FirstOrDefault(x => x.Thumbprint == oldHash);
            if(cert != null)
            { 
                var i = certDetails.IndexOf(cert);

                if (i >= 0)
                   ComboBoxCertificate.SelectedIndex = i;
            }
        }

        readonly static string _intRegex = "[^0-9]";
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(e.Text, _intRegex))
                e.Handled = true;
        }

        private void CheckBox1_CheckedChanged(object sender, RoutedEventArgs e)
        {
            TbPort.IsEnabled = CheckBox1.IsChecked ?? false;
        }

        private void CheckBox2_CheckedChanged(object sender, RoutedEventArgs e)
        {
            TbPortSsl.IsEnabled = CheckBox2.IsChecked ?? false;
        }

        public void SetCertificate(CertDetails cert)
        {
            if (!string.IsNullOrEmpty(oldHash))
                RemoveBinding().WaitForExit();

            var id = Name == "AMTANGEE.CalCardDAV.Server.Service" ?
                "13A1739F-0EB2-4334-48D3-84B62B92FB04" : "6CBD1C2F-1110-9F24-9878-AF667CF76D4C";

            var cmd = "netsh http add sslcert ipport=0.0.0.0:" + TbPortSsl.Text + " certhash=" + cert.Thumbprint.ToLower() + " appid={" + id + "} clientcertnegotiation=enable";
                
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + cmd;
            process.StartInfo = startInfo;
            process.Start();
        }

        private System.Diagnostics.Process RemoveBinding()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.FileName = "cmd.exe";
            psi.Arguments = "/C netsh http delete sslcert ipport=0.0.0.0:" + TbPortSsl.Text;
            p.StartInfo = psi;
            p.Start();

            return p;
        }
    }
}
