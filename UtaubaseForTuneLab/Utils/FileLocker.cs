using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UtaubaseForTuneLab.Utils
{
    internal static class FileLocker
    {
        public static bool IsLockFile(string TargetFile)//锁定检测
        {
            string LockFile = string.Format("{0}.{1}.filelock", TargetFile, Process.GetCurrentProcess().Id);
            bool locked = File.Exists(LockFile);
            return locked;
        }
        public static void LockFile(string TargetFile)//上锁
        {
            string LockFile = string.Format("{0}.{1}.filelock", TargetFile, Process.GetCurrentProcess().Id);
            while (true)
            {
                try
                {
                    File.WriteAllText(LockFile, Process.GetCurrentProcess().Id.ToString());
                    break;
                    Task.Delay(100).Wait();
                }
                catch {; }
            }
        }

        public static void UnlockFile(string TargetFile)//删除锁
        {
            string LockFile = string.Format("{0}.{1}.filelock", TargetFile, Process.GetCurrentProcess().Id);
            if(File.Exists(LockFile))File.Delete(LockFile);
        }
    }

    internal class ProcessQueueLocker : IDisposable
    {
        static List<string> WorkingFile = new List<string>();

        List<string> fileBeLock = new List<string>();
        bool lockMultiProcess = false;
        public ProcessQueueLocker(string[] LockFile, bool lockMultiProcess=false)
        {
            fileBeLock.AddRange(LockFile);
            this.lockMultiProcess = lockMultiProcess;
            LockFiles();
        }
        public ProcessQueueLocker(string LockFile, bool lockMultiProcess = false)
        {
            fileBeLock.Add(LockFile);
            this.lockMultiProcess = lockMultiProcess;
            LockFiles();
        }
        public void Dispose()
        {
            UnlockFiles();
        }

        bool checkIsProcessInnerLockedWithoutQueueLock()
        {
            bool locing = false;
            if (fileBeLock.Count == 1)
            {
                locing = WorkingFile.Contains(fileBeLock[0]);
            }
            else
            {
                Parallel.ForEach(fileBeLock, (f) =>
                {
                    locing = locing || WorkingFile.Contains(f);
                });
            }
            return locing;
        }
        bool checkIsProcessInnerLocked()
        {
            lock (WorkingFile)
            {
                return checkIsProcessInnerLockedWithoutQueueLock();
            }
        }
        bool checkIsProcessOutterLocked()
        {
            bool locing = false;
            if (fileBeLock.Count == 1)
            {
                locing = FileLocker.IsLockFile(fileBeLock[0]);
            }
            else
            {
                Parallel.ForEach(fileBeLock, (f) =>
                {
                    locing = locing || FileLocker.IsLockFile(f);
                });
            }
            return locing;
        }
        void Delay()
        {
            Random rand = new Random();
            Task.Delay(rand.Next(10,201)).Wait();
        }
        void LockFiles()
        {
            //进程内锁定
            while (checkIsProcessInnerLocked()) Delay();
            {
                lock (WorkingFile)//上锁
                {
                    while (checkIsProcessInnerLockedWithoutQueueLock()) Delay();//上锁后防止互斥，再查一次锁
                    WorkingFile.AddRange(fileBeLock);//上锁
                }
            }
            //进程间锁定
            if (lockMultiProcess)
            {
                while (checkIsProcessOutterLocked()) Delay();
                {
                    foreach(var lFile in fileBeLock)
                    {
                        while(FileLocker.IsLockFile(lFile)) Delay();
                        FileLocker.LockFile(lFile);
                    }
                }
            }
        }
        void UnlockFiles()
        {
            //进程间解锁定
            {
                if (lockMultiProcess)
                {
                    foreach (var lFile in fileBeLock)
                    {
                        FileLocker.UnlockFile(lFile);
                    }
                }
            }
            //进程内解锁定
            {
               // lock (WorkingFile)
                {
                    foreach(var lFile in fileBeLock)
                    {
                        if (WorkingFile.Contains(lFile)) WorkingFile.Remove(lFile);
                    }
                }
            }
        }


    }
}
