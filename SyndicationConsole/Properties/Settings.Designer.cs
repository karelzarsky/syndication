﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SyndicationConsole.Properties {
    
    
    [CompilerGenerated()]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("karel.zarsky@gmail.com")]
        public string fromAddress {
            get {
                return ((string)(this["fromAddress"]));
            }
            set {
                this["fromAddress"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("karel.zarsky@gmail.com")]
        public string toAddress {
            get {
                return ((string)(this["toAddress"]));
            }
            set {
                this["toAddress"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("smtp.gmail.com")]
        public string smtpHost {
            get {
                return ((string)(this["smtpHost"]));
            }
            set {
                this["smtpHost"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("false")]
        public string UseDefaultSMTPCredentials {
            get {
                return ((string)(this["UseDefaultSMTPCredentials"]));
            }
            set {
                this["UseDefaultSMTPCredentials"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("karel.zarsky@gmail.com")]
        public string smtpLogin {
            get {
                return ((string)(this["smtpLogin"]));
            }
            set {
                this["smtpLogin"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("ELGSilic")]
        public string smtpPassword {
            get {
                return ((string)(this["smtpPassword"]));
            }
            set {
                this["smtpPassword"] = value;
            }
        }
    }
}
