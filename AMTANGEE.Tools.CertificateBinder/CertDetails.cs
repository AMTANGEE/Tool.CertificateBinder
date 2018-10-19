using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Documents;

namespace AMTANGEE.Tools.CertificateBinder
{
    public class CertDetails
    {
        public X509Certificate2 Certificate { get; private set; }

        public string FriendlyName => string.IsNullOrWhiteSpace(Certificate.FriendlyName) ? Certificate.Subject : Certificate.FriendlyName;
        public string Subject => Certificate == null ? "" : Certificate.Subject;

        private string _intendedPurpose;
        public string IntendedPurposes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_intendedPurpose))
                {
                    var purposes = new List<string>();
                    foreach (var ext in Certificate.Extensions)
                        if (ext is X509EnhancedKeyUsageExtension eku)
                            foreach (var oid in eku.EnhancedKeyUsages)
                                purposes.Add(oid.FriendlyName);

                    _intendedPurpose = string.Join(", ", purposes);
                }

                return _intendedPurpose;
            }
        }

        public string ValidUntilString => Certificate == null ? "" : Certificate.NotAfter.ToString("dd.MM.yyyy");
        public string ValidFromString => Certificate == null ? "" : Certificate.NotBefore.ToString("dd.MM.yyyy");

        public string IssuerName => Certificate == null? "" : (Certificate.Issuer.StartsWith("CN=")? Certificate.Issuer.Substring(3) : Certificate.Issuer);
        public string IssuedFor => Certificate == null ? "" : Certificate.GetNameInfo(X509NameType.SimpleName, false);

        public string Thumbprint => Certificate == null ? "" : (Certificate.Thumbprint ?? "no thumbprint").ToLower();

        public CertDetails(X509Certificate2 cert)
        {
            Certificate = cert;
        }

        public string ToEcsString() => Certificate == null ? "" : (FriendlyName.Length > 30 ? FriendlyName.Substring(0, 27) + "..." : FriendlyName) + " ("+ Subject +")";
        public override string ToString() => (FriendlyName.Length > 30 ? FriendlyName.Substring(0, 27) + "..." : FriendlyName) + " (" + Thumbprint + ")";
    }
}