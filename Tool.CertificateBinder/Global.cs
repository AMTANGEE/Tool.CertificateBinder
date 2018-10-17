using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;

namespace CertBindToolWpf
{
    public class Global
    {
        private static X509Store certStore;
        public static X509Store CertStore
        {
            get
            {
                if(certStore == null)
                {
                    certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    try
                    {
                        certStore.Open(OpenFlags.ReadWrite);
                    }
                    catch (System.Security.Cryptography.CryptographicException)
                    {
                        MessageBox.Show("Das Programm muss als Administrator gestartet werden!",
                            "Fehlende Zugriffsrechte", MessageBoxButton.OK, MessageBoxImage.Error);
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }
                }

                return certStore;
            }
        }

    }
}
