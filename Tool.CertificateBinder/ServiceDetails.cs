using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace CertBindToolWpf
{
    public class ServiceDetails
    {
        public string ServiceName { get; set; }
        public string InstallPath { get; set; }
        static Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services", false);
        public bool Installed { get => System.IO.Directory.Exists(InstallPath); }

        public ServiceDetails(string ServiceName)
        {
            this.ServiceName = ServiceName;
            InstallPath = GetInstallPath();
        }

        private string GetInstallPath()
        {
            string value = "";
            try
            {
                using (var reg = registryKey.OpenSubKey(ServiceName, false))
                {
                    if (reg.GetValue("ImagePath") != null)
                    {
                        string path = (string)reg.GetValue("ImagePath");
                        path = path.Substring(1, path.LastIndexOf('\\'));

                        value = path;
                    }
                }
            }
            catch
            {
                value = null;
            }

            return value;
        }

        public System.Collections.Hashtable GetSettings()
        {
            System.Collections.Hashtable _ret = new System.Collections.Hashtable();
            if (Installed)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader
                (
                    new System.IO.FileStream(
                        System.IO.Path.Combine(InstallPath, ServiceName + ".exe.config"),
                        System.IO.FileMode.Open,
                        System.IO.FileAccess.Read,
                        System.IO.FileShare.Read)
                );
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                string xmlIn = reader.ReadToEnd();
                reader.Close();
                doc.LoadXml(xmlIn);
                foreach (System.Xml.XmlNode child in doc.ChildNodes)
                    if (child.Name.Equals("configuration"))
                        foreach (System.Xml.XmlNode node in child.ChildNodes)
                            if (node.Name.Equals("appSettings"))
                                foreach (System.Xml.XmlNode node2 in node.ChildNodes)
                                    if (node2.Name.Equals("add"))
                                        _ret.Add
                                        (
                                            node2.Attributes["key"].Value,
                                            node2.Attributes["value"].Value
                                        );
            }
            return _ret;
        }

        public bool SetSettings(System.Collections.Hashtable data)
        {
            var configPath = System.IO.Path.Combine(InstallPath, ServiceName + ".exe.config");
            var result = false;
            if (Installed)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader
                (
                    new System.IO.FileStream(
                        configPath,
                        System.IO.FileMode.Open,
                        System.IO.FileAccess.Read,
                        System.IO.FileShare.Read)
                );
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                string xmlIn = reader.ReadToEnd();
                reader.Close();
                doc.LoadXml(xmlIn);
                foreach (System.Xml.XmlNode child in doc.ChildNodes)
                    if (child.Name.Equals("configuration"))
                        foreach (System.Xml.XmlNode node in child.ChildNodes)
                            if (node.Name.Equals("appSettings"))
                                foreach (System.Xml.XmlNode node2 in node.ChildNodes)
                                    if (node2.Name.Equals("add"))
                                        foreach (string key in data.Keys)
                                            if (key == node2.Attributes["key"].Value)
                                            {
                                                if (node2.Attributes["value"].Value != data[key].ToString())
                                                    result = true;

                                                node2.Attributes["value"].Value = data[key].ToString();
                                            }

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(configPath, false, System.Text.Encoding.UTF8))
                    sw.Write(doc.OuterXml.Replace(">", ">\r\n"));
            }
            return result;
        }

        public void RestartService()
        {
            var sc = new ServiceController(ServiceName);
            if(sc.DisplayName == null)
            {
                System.Windows.MessageBox.Show("Der Dienst '" + ServiceName + "' konnte nicht gefunden werden.\r\n\r\nBitte starten Sie den Dienst manuell neu.", "Fehler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (sc.Status != ServiceControllerStatus.Stopped)
                sc.Stop();
            
            sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 15));

            if(sc.Status != ServiceControllerStatus.Stopped)
            {
                System.Windows.MessageBox.Show("Der Dienst '" + ServiceName + "' konnte nicht gestopt werden.\r\n\r\nBitte starten Sie den Dienst manuell neu.", "Fehler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 15));
            if (sc.Status != ServiceControllerStatus.Running)
            {
                System.Windows.MessageBox.Show("Der Dienst '" + ServiceName + "' konnte nicht gestartet werden.\r\n\r\nBitte starten Sie den Dienst manuell neu.", "Fehler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
        }
    }
}
