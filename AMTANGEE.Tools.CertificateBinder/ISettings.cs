using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMTANGEE.Tools.CertificateBinder
{
    interface ISettings
    {
        ServiceDetails Service { get; }
        List<CertDetails> Certs { get; }
        CertDetails SelectedCert { get; }
        bool SelectedCertIsDifferent { get; }
        bool HasChanged { get; }

        void SetService(ServiceDetails serviceDetails);
        void SetComboBoxSource(List<CertDetails> certDetails);
        bool SaveToSettings();

        void SetCertificate(CertDetails cert);
    }
}
