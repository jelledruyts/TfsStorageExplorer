using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: AssemblyVersion("1.0.*")] // This can be updated anytime as long as .NET user settings are not used (otherwise the settings directory depends on this version).
[assembly: AssemblyInformationalVersion("1.0.0.0")] // Keep this constant as long as possible to avoid the user's settings getting lost (it is used for the LocalUserAppDataPath where the configuration file is stored).

[assembly: AssemblyProduct("TFS Storage Explorer")]
[assembly: AssemblyTitle("TFS Storage Explorer")]
[assembly: AssemblyDescription("Allows browsing the Team Foundation Server (non-version control) storage.")]
[assembly: AssemblyCompany("Jelle Druyts")]
[assembly: AssemblyCopyright("Copyright © Jelle Druyts")]

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]