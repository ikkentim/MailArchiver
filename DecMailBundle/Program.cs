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
            if (Licences.SyncFusion != null)
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Licences.SyncFusion);
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}