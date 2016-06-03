using System.Configuration;

namespace CLRH100Demo1
{
    [System.Diagnostics.DebuggerNonUserCodeAttribute]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    public static class AppSettings
    {
        public static string AuthorizedKeys
        {
            get { return ConfigurationManager.AppSettings["authorizedKeys"]; }
        }

        public static class Clrh100demo1
        {
            public static string Url
            {
                get { return ConfigurationManager.AppSettings["clrh100demo1.url"]; }
            }
        }

        public static string Salt
        {
            get { return ConfigurationManager.AppSettings["salt"]; }
        }
    }
}

