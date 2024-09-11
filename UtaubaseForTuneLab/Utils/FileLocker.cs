using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtaubaseForTuneLab.Utils
{
    internal static class FileLocker
    {
        private static object innerLock = new object();
        //跨进程异步文件锁-目的：锁定文件只能有一个进程在读写
        public static bool IsLockFile(string TargetFile)
        {
            string LockFile = string.Format("{0}.{1}.filelock", TargetFile, Process.GetCurrentProcess().Id);
            bool locked = false;
            lock(innerLock) { locked = File.Exists(LockFile); }
            return locked;
        }
        public static void LockFile(string TargetFile)
        {
            string LockFile = string.Format("{0}.{1}.filelock", TargetFile, Process.GetCurrentProcess().Id);
            File.WriteAllText(LockFile, Process.GetCurrentProcess().Id.ToString());
        }

        public static void WaitforLock(string TargetFile)
        {
            //while (TryLockFile(TargetFile)) { Task.Delay(100).Wait();  }
            //LockFile(TargetFile);
            while (true)
            {
                if(!IsLockFile(TargetFile)) { 
                    LockFile(TargetFile);
                    break;
                }
                Task.Delay(100).Wait();
            }
        }

        public static void UnlockFile(string TargetFile)
        {
            string LockFile = string.Format("{0}.{1}.filelock", TargetFile, Process.GetCurrentProcess().Id);
            if(File.Exists(LockFile))File.Delete(LockFile);
        }
    }
}
