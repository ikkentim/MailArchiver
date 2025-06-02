namespace DecMailBundle
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // You need to create a licence class (ignored in .gitignore) with a SyncFusion license key.
            //
            // namespace DecMailBundle;
            // internal static class Licences
            // {
            //     public static string SyncFusion => "licence-key-here";
            // }

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Licences.SyncFusion);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}