using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog
{
    public class ILogger
    {
        public void Information(string message) {; }
        public void Error(string message) {; }
        public void Warning(string message) {; }
        public void Error(Exception exception, string message) {; }
        public void Information(Exception exception, string message) {; }
        public void Warning(Exception exception, string message) {; }
    }
    public class Log
    {
        public static void Error(Exception exception, string message) {; }
        public static void Information(Exception exception, string message) {; }
        public static void Warning(Exception exception, string message) {; }
        public static void Error(string message) {; }
        public static void Information(string message) {; }
        public static void Warning(string message) {; }
    }
}
