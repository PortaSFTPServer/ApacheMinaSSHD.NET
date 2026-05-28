using ApacheMinaSSHD.NET.Helpers;
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Logging;
using System.Text.Json;

namespace SimpleSSHDSever
{
    public partial class frmMain : Form
    {
        private AMNetSshServer? sshd = null;

        // Fix: Configure JSON to ignore case (JS = lowercase, C# = PascalCase)

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static string appPath => AppContext.BaseDirectory;

        /// <summary>
        /// the customize logger class
        /// this class implements the methods that is only needed
        /// </summary>
        IAMNetLogger logger = new AMNetLogger(typeof(frmMain), AMNetLogger.LogLevel.Debug);
        public frmMain()
        {
            InitializeComponent();

        }


        private void InitializeRichTextBoxLogging()
        {
            try
            {

                richTextBox1.WordWrap = false;

                _ = new SshdLoggerStream(this.richTextBox1);

                logger.Info("Java logging redirected to RichTextBox successfully.");
            }
            catch (Exception ex)
            {
                this.richTextBox1.AppendText($"Logging Init Error: {ex.Message}\n");
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            _ = this.richTextBox1.Handle;

            InitializeRichTextBoxLogging();

            logger.Info("This should now appear in the RichTextBox!");

        }

     

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                AMNSecurityUtils.SetFipsMode(checkBox1.Checked);
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {

            try
            {
                btnStart.Enabled = false;


                // let just wrap the whole block
                await Task.Run(() =>
                {

                    // coalesce  / or use compound assignment
                    sshd = sshd ?? AMNetSshServer.setUpDefaultServer();

                    bool isRunning = sshd.isStarted();

                    if (!isRunning)
                    {

                        // this is if you want to use FIPS mode / - disabled by default
                        AMNSecurityUtils.SetFipsMode(checkBox1.Checked);

                        sshd.setPort(2222);

                        var keyPath = Path.Combine(appPath, "hostkey.ser");

                        var kekProvider = new AMNetSimpleGeneratorHostKeyProvider(keyPath);

                        kekProvider.setAlgorithm("RSA");

                        kekProvider.setKeySize(2048);

                        sshd.setKeyPairProvider(kekProvider);

                        string usersHome = Path.Combine(appPath, "DATA");

                        if (!Directory.Exists(usersHome)) { Directory.CreateDirectory(usersHome); }

                        var fileSystemFactory = new AMNetVirtualFileSystemFactory(usersHome);

                        sshd.setFileSystemFactory(fileSystemFactory);

                        string? samplePassword = "Test1234"; // Environment.GetEnvironmentVariable("AMNET_SAMPLE_PASSWORD");
                        if (!string.IsNullOrWhiteSpace(samplePassword))
                        {
                            string sampleUsername = Environment.GetEnvironmentVariable("AMNET_SAMPLE_USERNAME") ?? "demo";
                            sshd.SetFixedPasswordAuthenticator(sampleUsername, samplePassword);
                        }

                        // The sample uses paths relative to the application output folder.
                        // AMNetPublickeyAuthenticator will look for ./Authorized_Keys under this base path.
                        // Create that folder and add user public keys when testing public-key authentication.
                        string authorizedKeysBasePath = appPath;

                        sshd.setPublickeyAuthenticator(new AMNetPublickeyAuthenticator(authorizedKeysBasePath));

                        // this will be called right after the proxy and before the session is created
                        sshd.setIoServiceEventListener(new AMNetIoServiceEventListener());


                        // to monitor the file operations.
                        var sftpFactory = new AMNetSftpSubsystemFactory();


                        sshd.setSubsystemFactories(sftpFactory);

                        // you can use this for the logging / override 
                        // from your c# application - note that you may need to deal with some java code, depending on your implementation
                        sftpFactory.addSftpEventListener(new AMNetSftpEventListener());


                        // Set your custom DirectoryStream.Filter e.g hide or show files
                        sftpFactory.setFileSystemAccessor(new AMNetSftpFileSystemAccessor());



                        // this needed to be implemented in the bridge / internals
                        // sshd.addSessionListener(new MyDynamicAuthListener());

                        // attatch the intance of the server
                        // Keep the sample closed by default. Set AMNET_SAMPLE_PASSWORD to enable password auth.
                        if (string.IsNullOrWhiteSpace(samplePassword))
                        {
                            sshd.SetAuthenticationMethods(AMNetSshAuthenticationMethods.PublicKey);
                        }
                        else
                        {
                            sshd.SetAuthenticationMethods(
                                AMNetSshAuthenticationMethods.PublicKey,
                                AMNetSshAuthenticationMethods.Password);
                        }
                        sshd.Config.MAX_AUTH_REQUESTS = 10;
                        sshd.Config.AUTH_TIMEOUT = TimeSpan.FromSeconds(60); 
                        sshd.Config.IDLE_TIMEOUT = TimeSpan.FromSeconds(120);
                        sshd.Config.HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(45);
                        sshd.Config.MAX_CONCURRENT_CHANNELS = 10;
                        sshd.Config.MAX_CONCURRENT_SESSIONS = 5;

                        // serverProxyAcceptor
                        // this will act as the gatekeeper for balancer or proxy server to communicate with SFTP Server,
                        // this can also helps to prevent attackers from reaching the server
                        // this should be conditional based on the admin requirements
                        // sshd.setServerProxyAcceptor(new AMNetServerProxyAcceptor());

                        sshd.start();

                        btnStart.BeginInvoke(new Action(() =>
                        {
                            btnStart.Text = "Stop";
                            btnStart.Enabled = true;
                        }));


                        logger.Info("IKVM-hosted SSH Server started on port 2222...");


                    }
                    else
                    {
                        try
                        {

              

                            sshd.stop();
                            int timeout = 0;
                            while (!sshd.isClosed() && timeout < 20)
                            {
                                Thread.Sleep(100);
                                timeout++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Shutdown error: " + ex.Message);
                        }

                        btnStart.BeginInvoke(new Action(() =>
                        {
                            btnStart.Text = "Start";
                            btnStart.Enabled = true;
                        }));


                        if (sshd.isClosed())
                        {
                            sshd = AMNetSshServer.setUpDefaultServer();
                            logger.Info("IKVM-hosted SSH Server has been stopped on port 2222...");
                        }

                    }

                });

            }
            catch (Exception)
            {
                MessageBox.Show("Operation can not be completed this time. Please try again later", "NetSSHServer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }


       

    }
}
