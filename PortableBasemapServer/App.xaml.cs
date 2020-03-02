using System.Windows;
using PBS.APP.ViewModels;
using System.Diagnostics;
using System.ServiceProcess;
using PBS.Util;
using System;

namespace PBS.APP
{
    /// <summary>
    /// 初始化
    /// </summary>
    public partial class App : Application
    {
        //程序启动
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CheckAdministrator();
            //log4net初始化
            try
            {
                Utility.InitLog4net();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Application.Current.FindResource("ReadLog4netConfigError")+"\r\n"+ex.Message);
                Application.Current.Shutdown(1);
                return;
            }


            //避免启动多个进程
            string processName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            int processCount = 0;
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == processName)
                {
                    processCount++;
                }
            }
            if (processCount>1)
            {
                Application.Current.Shutdown();
                if (e.Args.Length == 1 && e.Args[0] == "/runasservice")//try to start windows service after exe started
                {
                    //when come to here, it's another process to write to the same log file.
                    //ref:Can Log4net have multiple appenders write to the same file?
                    //http://stackoverflow.com/questions/3010407/can-log4net-have-multiple-appenders-write-to-the-same-file
                    Utility.Log(LogLevel.Error,null,Application.Current.FindResource("WindowsServiceStartError").ToString() + Application.Current.FindResource("ProcessStarted").ToString());
                }
                else if (PBS.Util.Utility.IsWindowsServiceStarted(processName))//try to start exe after windows service started
                {
                    MessageBox.Show(Application.Current.FindResource("WindowsServiceStarted").ToString());
                }
                else//try to start another exe after exe started
                {
                    MessageBox.Show(Application.Current.FindResource("ProcessStarted").ToString());
                    //throw new Exception(Application.Current.FindResource("ProcessStarted").ToString());
                }                
                return;
            }
            if (e.Args.Length == 1)
            {
                switch (e.Args[0])
                {
                    case "/runasservice":
                        ServiceBase[] servicesToRun = new ServiceBase[]
                                { 
                                    new PBSWindowsService()
                                };
                        ServiceBase.Run(servicesToRun);
                        break;
                    //case "/getServices":
                    //    Console.WriteLine("Runing Services Count:");
                    //    break;
                    default:
                        break;
                }
            }
            else
            {
                MainWindow mw = new MainWindow(new VMMainView());
            }
        }

        /// <summary>
        /// 检测是否以管理员身份运行
        /// </summary>
        private void CheckAdministrator()
        {
            var wi = System.Security.Principal.WindowsIdentity.GetCurrent();
            var wp = new System.Security.Principal.WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                var processInfo = new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("程序自动以管理员身份运行出错，请手动设置以管理员身份运行程序" + ex);
                    throw;
                }
                Environment.Exit(0);
            }
        }

        //程序退出
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            
        }


        //未处理的异常
        private void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //#if RELEASE
            //Unhandled Exception Handler For WPF Applications
            //http://www.codeproject.com/Articles/90866/Unhandled-Exception-Handler-For-WPF-Applications
            MessageBox.Show(e.Exception.Message + "\r\n" + e.Exception.StackTrace, "Exception Caught",MessageBoxButton.OK, MessageBoxImage.Error);
            Utility.Log(LogLevel.Fatal, e.Exception);
            Application.Current.Shutdown(1);
            e.Handled = true;
        }
    }
}
