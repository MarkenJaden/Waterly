using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die einer Assembly zugeordnet sind.
[assembly: AssemblyTitle("Waterly")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("MarkenJaden")]
[assembly: AssemblyProduct("Waterly")]
[assembly: AssemblyCopyright("Copyright ©  2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion 
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder Standardwerte für die Build- und Revisionsnummern verwenden, 
// indem Sie "*" wie unten gezeigt eingeben:
// [Assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: ComVisible(false)]

public static class AssemblyInfo
{
    /// <summary>
    /// Utility method for retrieving assembly attributes
    /// </summary>
    /// <typeparam name="T">The type of the attribute that has to be returned.</typeparam>
    /// <param name="assembly">The assembly from which the attribute has to be retrieved.</param>
    /// <returns>The requested assembly attribute value (or null)</returns>
    public static T GetAttribute<T>(Assembly assembly)
        where T : Attribute
    {
        // Get attributes of the required type
        var attributes = assembly.GetCustomAttributes(typeof(T), true);

        // If we didn't get anything, return null
        if (attributes.Length == 0)
            return null;

        // Convert the first attribute value
        // into the desired type and return it
        return (T)attributes[0];
    }
}