namespace SimpleSSHDSever
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            // InitializeIkvm();

            if (args.Any(arg => string.Equals(arg, "--integration-tests", StringComparison.OrdinalIgnoreCase)))
            {
                return IntegrationTestRunner.RunAsync(args).GetAwaiter().GetResult();
            }

            RunWinForms();
            return 0;
        }

        private static void InitializeIkvm()
        {
            // Load the DLLs by their assembly name (usually the filename without .dll)
            var apiAssembly = System.Reflection.Assembly.Load("org.slf4j");
            var simpleAssembly = System.Reflection.Assembly.Load("org.slf4j.simple");

            //var logbackAssembly = System.Reflection.Assembly.Load("ch.qos.logback.classic");
            //var logbackCoreAssembly = System.Reflection.Assembly.Load("ch.qos.logback.core");
            // Force IKVM to search these assemblies for Java classes
            ikvm.runtime.Startup.addBootClassPathAssembly(apiAssembly);
            ikvm.runtime.Startup.addBootClassPathAssembly(simpleAssembly);
            
            //ikvm.runtime.Startup.addBootClassPathAssembly(logbackAssembly);
            //ikvm.runtime.Startup.addBootClassPathAssembly(logbackCoreAssembly);
        }

        private static void RunWinForms()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
