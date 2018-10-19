using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using AMTANGEE.Tools.CertificateBinder.Annotations;

namespace AMTANGEE.Tools.CertificateBinder
{
    /// <summary>
    /// Interaction logic for CertificateForm.xaml
    /// </summary>
    public partial class CertificateForm : Window, INotifyPropertyChanged
    {
        public ObservableCollection<CertDetails> CertDetails { get; set; }
        public CertDetails SelectedCert { get; private set; }
        private bool _hasChanged = false;

        public CertificateForm(List<CertDetails> certDetails)
        {
            InitializeComponent();

            CertDetails = new ObservableCollection<CertDetails>();
            certDetails.ForEach(x => CertDetails.Add(x));
            DGCerts.ItemsSource = CertDetails;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnAddCertificate_Click(object sender, RoutedEventArgs e)
        {
            var acf = new AddCertificateForm();
            
            if((acf.ShowDialog()??false) == false)
                return;
            
            Global.CertStore.Add(acf.Certificate);
            CertDetails.Add(new CertDetails(acf.Certificate));
            SelectedCert = null;
            OnPropertyChanged(nameof(CertDetails));
            _hasChanged = true;
        }

        private void BtnRenameCertificate_Click(object sender, RoutedEventArgs e)
        {
            var rcf = new RenameCertificateForm(SelectedCert.Certificate);
            if ((rcf.ShowDialog() ?? false) == false)
                return;
            var index = DGCerts.SelectedIndex;
            DGCerts.ItemsSource = null;
            DGCerts.ItemsSource = CertDetails;
            DGCerts.SelectedIndex = index;
            _hasChanged = true;
        }

        private void BtnDeleteCertificate_Click(object sender, RoutedEventArgs e)
        {
            Global.CertStore.Remove(SelectedCert.Certificate);
            CertDetails.Remove(SelectedCert);
            SelectedCert = null;
            OnPropertyChanged(nameof(CertDetails));
            _hasChanged = true;
        }

        private void DGCerts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnDeleteCertificate.IsEnabled = BtnRenameCertificate.IsEnabled = DGCerts.SelectedIndex >= 0;
            SelectedCert = (CertDetails)DGCerts.SelectedItem;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(CertDetails));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            DialogResult = _hasChanged;
        }
    }
}
