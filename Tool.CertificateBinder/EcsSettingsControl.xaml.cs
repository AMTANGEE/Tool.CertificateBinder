using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CertBindToolWpf
{
    /// <summary>
    /// Interaction logic for EcsSettingsControl.xaml
    /// </summary>
    public partial class EcsSettingsControl : UserControl, ISettings
    {
        public ServiceDetails Service { get; private set; }
        public List<CertDetails> Certs { get; private set; }
        public CertDetails SelectedCert
        {
            get
            {
                var ecsString = (string)ComboBoxCertificate.SelectedItem;
                if (ecsString != null)
                    return Certs.FirstOrDefault(x => x.ToEcsString() == ecsString);
                else
                    return null;
            }
        }

        public bool SelectedCertIsDifferent => SelectedCert == null ? 
            false : _settings["SslCertificateName"] == null ? 
            true : SelectedCert.Subject != (string)_settings["SslCertificateName"];
            
        public bool HasChanged
        {
            get
            {
                return !(
                    TbImapSsl.Text == (string)_settings["ImapSslPort"] &&
                    TbSmtpSsl.Text == (string)_settings["SmtpSslPort"] &&
                    CheckBoxSslOnly.IsChecked == ((string)_settings["UseSslOnly"] == "1") &&
                    TbImap.Text == (string)_settings["ImapPort"] &&
                    TbSmtp.Text == (string)_settings["SmtpPort"]
                );
            }
        }


        private System.Collections.Hashtable _settings;

        public EcsSettingsControl()
        {
            InitializeComponent();
        }

        public EcsSettingsControl(ServiceDetails serviceDetails)
        {
            InitializeComponent();

            SetService(serviceDetails);
        }

        public void SetService(ServiceDetails serviceDetails)
        {
            Service = serviceDetails;
            _settings = Service.GetSettings();

            /*
                <add key="ImapPort" value="140" />
                <add key="ImapSslPort" value="1993" />
                <add key="SmtpPort" value="20" />
                <add key="SmtpSslPort" value="1465" />
                <add key="UseSslOnly" value="1" />
                <add key="SslCertificateName" value="E=test@test.de, CN=172.16.3.24, OU=test, O=test, L=Berlin, S=Berlin, C=DE" />
            */

            TbImapSsl.Text = (string)_settings["ImapSslPort"];
            TbSmtpSsl.Text = (string)_settings["SmtpSslPort"];

            CheckBoxSslOnly.IsChecked = (string)_settings["UseSslOnly"] == "1";
            TbImap.Text = (string)_settings["ImapPort"];
            TbSmtp.Text = (string)_settings["SmtpPort"];
        }

        public bool SaveToSettings()
        {
            _settings["ImapSslPort"] = TbImapSsl.Text;
            _settings["SmtpSslPort"] = TbSmtpSsl.Text;
            _settings["UseSslOnly"] = (CheckBoxSslOnly.IsChecked ?? false) ? "1" : "0";
            _settings["ImapPort"] = TbImap.Text;
            _settings["SmtpPort"] = TbSmtp.Text;

            return Service.SetSettings(_settings);
        }

        public void SetComboBoxSource(List<CertDetails> certDetails)
        {
            if (certDetails == null)
                return;

            Certs = certDetails;
            ComboBoxCertificate.ItemsSource = Certs.Select(x => x.ToEcsString());

            var cert = Certs.FirstOrDefault(x => x.Subject == (string)_settings["SslCertificateName"]);
            if (cert != null)
            {
                var i = ComboBoxCertificate.Items.IndexOf(cert.ToEcsString());

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

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            TbImap.IsEnabled = TbSmtp.IsEnabled = !(CheckBoxSslOnly.IsChecked ?? false);
        }

        public void SetCertificate(CertDetails cert)
        {
            if (!SelectedCertIsDifferent)
                return;

            _settings["SslCertificateName"] = cert.Subject;
            SaveToSettings();
        }
    }
}
