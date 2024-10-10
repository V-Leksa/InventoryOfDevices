using System.Diagnostics;
using System.Windows;
using static System.Diagnostics.Process;

namespace InventoryOfDevices.Services
{
     internal static class CustomDiagnostics
     {
        static Stopwatch timer = new Stopwatch();
        static long bytesPhysicalBefore = 0;
        static long bytesVirtualBefore = 0;

        /// <summary>
        /// Начинает запись.
        /// </summary>
        public static void Start()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            // Отображает в байтах количество виртуальной памяти, выделенной для процесса.
            bytesPhysicalBefore = GetCurrentProcess().WorkingSet64;
            // Отображает в байтах количество физической памяти, выделенной для процесса.
            bytesVirtualBefore = GetCurrentProcess().VirtualMemorySize64;
            timer.Restart();
        }

        /// <summary>
        /// Останавливает запись.
        /// </summary>
        public static void Stop()
        {
            timer.Stop();
            long bytesPhysicalAfter = GetCurrentProcess().WorkingSet64;
            long bytesVirtualAfter = GetCurrentProcess().VirtualMemorySize64;

            string answer = $"Запись остановлена.\n" +
                $"{bytesPhysicalAfter - bytesPhysicalBefore:N0} использовано физической памяти (в байтах).\n" +
                $"{bytesVirtualAfter - bytesVirtualBefore:N0} использовано виртуальной памяти (в байтах).\n" +
                $"{timer.Elapsed} время выполнения.\n" +
                $"{timer.ElapsedMilliseconds:N0} время выполнения в миллисекундах.";

            MessageBox.Show(answer);
        }
     }
}
