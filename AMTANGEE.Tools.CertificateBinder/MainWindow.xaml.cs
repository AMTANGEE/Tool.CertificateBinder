using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace AMTANGEE.Tools.CertificateBinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<CertDetails> _certificates;
        private readonly ServiceDetails _calCardDavService = new ServiceDetails("AMTANGEE.CalCardDAV.Server.Service");
        private readonly ServiceDetails _mobileServerService = new ServiceDetails("AMTANGEE.Mobile.Server.Service");
        private readonly ServiceDetails _emailConnectivityService = new ServiceDetails("AMTANGEE.EmailConnectivity.Service");

        private readonly List<ISettings> _settingsControls;

        public MainWindow()
        {
            InitializeComponent();

            _settingsControls = new List<ISettings>() { SslCtrlCCD, SslCtrlMobile, EcsCtrl };

            GroupBoxCCD.Visibility = _calCardDavService.Installed? Visibility.Visible : Visibility.Collapsed;
            GroupBoxMobile.Visibility = _mobileServerService.Installed? Visibility.Visible : Visibility.Collapsed;
            GroupBoxECS.Visibility = _emailConnectivityService.Installed? Visibility.Visible : Visibility.Collapsed;

            if (!_calCardDavService.Installed && !_mobileServerService.Installed && !_emailConnectivityService.Installed)
                BtnApply.Visibility = Visibility.Collapsed;

            SslCtrlCCD.SetService(_calCardDavService);
            SslCtrlMobile.SetService(_mobileServerService);
            EcsCtrl.SetService(_emailConnectivityService);
            
            RefreshCerts();
        }

        public void RefreshCerts()
        {
            _certificates = new List<CertDetails>();
            var ecsCertList = new List<string>();

            foreach (var cert in Global.CertStore.Certificates)
            {
                if (!cert.HasPrivateKey)
                    continue;

                //var cur = new CertDetails(cert.Subject, cert.Thumbprint, cert.NotAfter);
                var cur = new CertDetails(cert);
                _certificates.Add(cur);
                ecsCertList.Add(cur.ToEcsString());
            }

            _settingsControls.ForEach(x => x.SetComboBoxSource(_certificates));
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            var changed = 0;

            foreach (var x in _settingsControls.Where(x => x.Service.Installed))
            {
                var hasChanged = false;
                if (x.HasChanged)
                {
                    x.SaveToSettings();
                    hasChanged = true;
                }
                if (x.SelectedCertIsDifferent)
                {
                    x.SetCertificate(x.SelectedCert);
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    changed++;
                    x.SetService(x.Service);
                    x.Service.RestartService();
                }
            }
            if(changed > 0)
                MessageBox.Show("Änderungen an " + changed + " Diensten gespeichert.", "Dienste angepasst", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Keine Änderungen festgestellt.", "Keine Änderung", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MaxHeight = MinHeight = Height;
        }

        private void BtnManageCertificates_Click(object sender, RoutedEventArgs e)
        {
            var window = new CertificateForm(_certificates);

            if(window.ShowDialog()??false)
                RefreshCerts();
        }
    }
}
