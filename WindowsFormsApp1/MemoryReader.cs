using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    class MemoryReader
    {
        const int PROCESS_VM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32")]
        public static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBase, ref long lpBuffer, int nSize, int lpNumberOfBytesRead);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out IntPtr processId);

        // offsets for: current turn number
        const int OFFSET_CTN_1 = 0x031AA488;
        const int OFFSET_CTN_2 = 0x0;
        const int OFFSET_CTN_3 = 0xFC0;

        // offsets for: ingame menu/window active (not sure what exactly, but it works)
        const int OFFSET_IMA_1 = 0x3486FDC;

        // wh2 process variables
        private Process wh2Process;
        private IntPtr wh2ProcessBaseAddress = IntPtr.Zero;
        private IntPtr wh2ProcessHandle = IntPtr.Zero;

        // addresses to scan
        private IntPtr addressCurrentTurnNumber = IntPtr.Zero;
        private IntPtr addressIngameMenuActive = IntPtr.Zero;

        public MemoryReader()
        {
            initProcess();

            addressCurrentTurnNumber = GetCurrentTurnNumberAddress();
            addressIngameMenuActive = wh2ProcessBaseAddress + OFFSET_IMA_1;
        }

        private void initProcess()
        {
            wh2Process = Process.GetProcessesByName("Warhammer2")[0];

            foreach (ProcessModule m in wh2Process.Modules)
            {
                if (m.ModuleName.Contains("Warhammer2"))
                {
                    wh2ProcessBaseAddress = m.BaseAddress;
                    break;
                }
            }

            if (wh2ProcessBaseAddress == IntPtr.Zero)
            {
                throw new Exception("Warhammer2 process not found!");
            }

            wh2ProcessHandle = OpenProcess(PROCESS_VM_READ, false, wh2Process.Id);
        }

        private IntPtr GetCurrentTurnNumberAddress()
        {
            long readValue = 0;
            ReadProcessMemory(wh2ProcessHandle, (wh2ProcessBaseAddress + OFFSET_CTN_1), ref readValue, sizeof(long), 0);
            ReadProcessMemory(wh2ProcessHandle, ((IntPtr) readValue + OFFSET_CTN_2), ref readValue, sizeof(long), 0);
            return (IntPtr) readValue + OFFSET_CTN_3;
        }

        public void resetAddressCurrentTurnNumber()
        {
            addressCurrentTurnNumber = GetCurrentTurnNumberAddress();
        }

        public int getCurrentTurnNumber()
        {
            long readValue = 0;
            ReadProcessMemory(wh2ProcessHandle, addressCurrentTurnNumber, ref readValue, sizeof(int), 0);
            return (int) readValue;
        }

        public bool isIngameMenuActive()
        {
            long readValue = 0;
            ReadProcessMemory(wh2ProcessHandle, addressIngameMenuActive, ref readValue, sizeof(byte), 0);
            return (readValue == 1);
        }

        public bool isWh2ProcessWindowActive()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;
            }

            IntPtr activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return Process.GetProcessById((int) activeProcId).ProcessName == "Warhammer2";
        }
    }
}
