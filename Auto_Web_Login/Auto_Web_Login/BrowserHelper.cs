using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace Auto_Web_Login
{
    internal class BrowserHelper
    {

        public static void SetBrowserEmulation(string programName, IE browserVersion)

        {
            //  if (string.IsNullOrEmpty(programName))
            //  {
            programName = AppDomain.CurrentDomain.FriendlyName;
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);


            if (regKey != null)
            {
                try
                {
                    regKey.SetValue(programName, browserVersion, RegistryValueKind.DWord);

                  
                    regKey.Close();
                    MessageBox.Show("Success");

                }
                catch (Exception ex)
                {
                    // throw new Exception("Error writing to the registry", ex);
                    MessageBox.Show("Error writing to the registry", ex.Message);
                }
            }

            else
            {
                try
                {
                    regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl", true);



                    regKey.CreateSubKey("FEATURE_BROWSER_EMULATION");
                    regKey.SetValue(programName, browserVersion, RegistryValueKind.DWord);

                    regKey.Close();
                    MessageBox.Show("Success");
                }
                catch (Exception ex)
                {
                    // throw new Exception("Error accessing the registry", ex);
                    MessageBox.Show("Error writing to the registry", ex.Message);
                }
            }
        }
        }

        internal enum IE
        {
            IE7 = 7000,
            IE8 = 8000,
            IE8StandardsMode = 8888,
            IE9 = 9000,
            IE9StandardsMode = 9999,
            IE10 = 10000,
            IE10StandardsMode = 10001,
            IE11 = 11000,
            IE11StandardsMode = 11000,
           
    }
    }
