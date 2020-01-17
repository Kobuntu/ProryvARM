using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;

namespace Proryv.AskueARM2.Client.ServiceReference.Service.OData
{
    public static class ConnectionHelper
    {
        private const string _prefix = "api";
        private static string _ODataBaseUri;
        private static object _syncLock = new object();

        public static string ODataBaseUri
        {
            get
            {
                lock (_syncLock)
                {
                    if (!string.IsNullOrEmpty(_ODataBaseUri)) return _ODataBaseUri;

                    _ODataBaseUri = GetConnectionStringFromRegistry()
                                    //?? ARM_Service.GetClickOnceEndPointAddressString()
                                    ?? GetArmServiceAddress()
                                    ?? "http://localhost.com:" + port + "/" + _prefix + "/";
                }

                return _ODataBaseUri;
            }
        }

        private static string GetConnectionStringFromRegistry()
        {
            var result = string.Empty;

            Func<RegistryView, string, string, string> readKeyFunc = (registry, keyName, valueName) =>
            {
                try
                {
                    var localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registry);
                    var rk = localMachine64.OpenSubKey(@"SOFTWARE");
                    if (rk != null)
                    {
                        rk = rk.OpenSubKey("НПФ ПРОРЫВ");
                        if (rk != null) rk = rk.OpenSubKey(keyName);
                        //if (rk != null) rk = rk.OpenSubKey("4.0");
                    }

                    if (rk != null)
                    {
                        return rk.GetValue(valueName) as string;
                    }
                }
                catch 
                {

                }

                return string.Empty;
            };

            var rv = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;

            result = readKeyFunc(rv, "ФСК", "ODataBaseUri");

            if (string.IsNullOrEmpty(result))
            {
                result = readKeyFunc(rv, "Телескоп+\\4.0", "ODataBaseUri");
            }

            if (string.IsNullOrEmpty(result)) return null;

            return result;
        }

        private static string GetArmServiceAddress()
        {
            if (ARM_Service.Service == null || ARM_Service.Service.Endpoint == null ||
                ARM_Service.Service.Endpoint.Address == null 
                || ARM_Service.Service.Endpoint.Address.Uri == null
                || string.IsNullOrEmpty(ARM_Service.Service.Endpoint.Address.Uri.Host)) return null;

            return "http://" + ARM_Service.Service.Endpoint.Address.Uri.Host + ":" + port + "/" + _prefix + "/";
        }


        private const string _defaultPort = "8018";

        private static string _port;
        private static string port
        {
            get
            {
                if (!string.IsNullOrEmpty(_port)) return _port;

                ArmRegistrySetting setting = null;

                try
                {
                    var dict = EnumClientServiceDictionary.GlobalSettingsFromServerRegistry;

                    List<ArmRegistrySetting> proryvODataServiceSettings;
                    if (dict == null || !dict.TryGetValue(RegistrySettings.FolderProryvODataService, out proryvODataServiceSettings) || proryvODataServiceSettings == null) return _defaultPort;

                    setting = EnumClientServiceDictionary.GetGlobalSettingsByName(RegistrySettings.FolderProryvODataService, RegistrySettings.SettingProryvOdataPort);
                }
                catch
                {
                    return _defaultPort;
                }

                if (setting == null) return _defaultPort;

                _port = setting.Setting;

                return _port ?? _defaultPort;
            }
        }

        private static bool? _isSupportProryvODataService;
        public static bool IsSupportProryvODataService
        {
            get
            {
                if (_isSupportProryvODataService.HasValue) return _isSupportProryvODataService.Value;

                List<ArmRegistrySetting> proryvODataServiceSettings = null;

                try
                {
                    var dict = EnumClientServiceDictionary.GlobalSettingsFromServerRegistry;
                    if (dict == null || !dict.TryGetValue(RegistrySettings.FolderProryvODataService, out proryvODataServiceSettings) ||
                        proryvODataServiceSettings == null)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

                var setting = proryvODataServiceSettings.FirstOrDefault(r =>
                    string.Equals(r.Key, "port", StringComparison.InvariantCultureIgnoreCase));

                if (setting == null) return false;

                _isSupportProryvODataService = !string.IsNullOrEmpty(setting.Setting);

                return _isSupportProryvODataService.Value;
            }
        }
    }
}
