using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private string _Appname = string.Empty;
        private string _Runtimename = string.Empty;
        private string _Jrename = string.Empty;

        public override void Run()
        {
            // これはワーカーの実装例です。実際のロジックに置き換えてください。
            Trace.WriteLine("PlayFramework is starting", "Information");

            //色々情報
            //ローカルストレージに文字列を出力する
            LocalResource disk = RoleEnvironment.GetLocalResource("LocalStorage1");
            DirectoryInfo localStorageRoot = new DirectoryInfo(disk.RootPath);
            using (var stream = new StreamWriter(disk.RootPath + "test.txt"))
            {
                string exe = string.Empty;
                stream.WriteLine("ローカルストレージに出力");
                try
                {
                    exe = disk.RootPath + @"\" + Path.GetFileNameWithoutExtension(_Appname) + @"\start.bat";
                    var proc = new Process();
                    var procStartInfo = new ProcessStartInfo();
                    procStartInfo.FileName = exe;
                    procStartInfo.UseShellExecute = false;
                    proc.StartInfo = procStartInfo;
                    proc.Start();
                }
                catch (Exception ex)
                {
                    //デバッグ情報出力
                    stream.WriteLine(ex);
                }
                finally
                {
                    //デバッグ情報出力
                    stream.WriteLine(exe);
                    stream.WriteLine(Environment.GetEnvironmentVariable("PATH"));
                    stream.WriteLine(Environment.GetEnvironmentVariable("JAVA_HOME"));
                    stream.WriteLine(Path.GetFileNameWithoutExtension(_Appname));
                }
            }


            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Play Framework is working", "Information");
            }
        }

        public override bool OnStart()
        {
            // 同時接続の最大数を設定します
            ServicePointManager.DefaultConnectionLimit = 12;

            // 構成の変更を処理する方法については、
            // MSDN トピック (http://go.microsoft.com/fwlink/?LinkId=166357) を参照してください。

            var appzip = RoleEnvironment.GetConfigurationSettingValue("PLAY_APP_ZIP_URI");
            var runtimezip = RoleEnvironment.GetConfigurationSettingValue("PLAY_RUNTIME_ZIP_URI");
            var jrezip = RoleEnvironment.GetConfigurationSettingValue("JRE_ZIP_URI");
            _Appname = Path.GetFileName(appzip);
            _Runtimename = Path.GetFileName(runtimezip);
            _Jrename = Path.GetFileName(jrezip);

            //ローカルストレージに文字列を出力する
            LocalResource disk = RoleEnvironment.GetLocalResource("LocalStorage1");
            DirectoryInfo localStorageRoot = new DirectoryInfo(disk.RootPath);

            //ZipFile のダウンロード
            new WebClient().DownloadFile(appzip, _Appname);
            new WebClient().DownloadFile(runtimezip, _Runtimename);
            new WebClient().DownloadFile(jrezip, _Jrename);

            //ZipFile の展開
            ZipFile.ExtractToDirectory(_Appname, disk.RootPath);
            ZipFile.ExtractToDirectory(_Runtimename, disk.RootPath);
            ZipFile.ExtractToDirectory(_Jrename, disk.RootPath);

            //アプリの展開
            File.Copy(_Runtimename, disk.RootPath + _Runtimename);

            //環境変数を設定する
            Environment.SetEnvironmentVariable("JAVA_HOME", disk.RootPath
                + Path.GetFileNameWithoutExtension(_Jrename));
            string pathval = Environment.GetEnvironmentVariable("PATH");
            pathval += ";" + disk.RootPath
                + Path.GetFileNameWithoutExtension(_Jrename) + @"\bin;" + disk.RootPath
                + Path.GetFileNameWithoutExtension(_Runtimename);
            Environment.SetEnvironmentVariable("PATH", pathval);

            return base.OnStart();
        }
    }
}
