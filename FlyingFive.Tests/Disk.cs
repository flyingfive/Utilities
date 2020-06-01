using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FlyingFive.Tests
{
    [Serializable]
    public struct HardDiskInfo
    {
        ///<summary>
        /// 型号
        ///</summary>
        public string ModuleNumber { get; internal set; }

        ///<summary>
        /// 固件版本
        ///</summary>
        public string Firmware { get; internal set; }

        ///<summary>
        /// 序列号
        ///</summary>
        public string SerialNumber { get; internal set; }

        ///<summary>
        /// 可寻址扇区数
        ///</summary>
        internal uint AddressableSectors { get; set; }
    }

    #region Internal Structs


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct GetVersionOutParams
    {
        public byte bVersion;
        public byte bRevision;
        public byte bReserved;
        public byte bIDEDeviceMap;
        public uint fCapabilities;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] dwReserved; // For future use.

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct IdeRegs
    {
        public byte bFeaturesReg;           // 特征寄存器(用于SMART命令)
        public byte bSectorCountReg;        // 扇区数目寄存器
        public byte bSectorNumberReg;       // 开始扇区寄存器
        public byte bCylLowReg;             // 开始柱面低字节寄存器
        public byte bCylHighReg;            // 开始柱面高字节寄存器
        public byte bDriveHeadReg;          // 驱动器/磁头寄存器
        public byte bCommandReg;            // 指令寄存器
        public byte bReserved;              // 保留
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SendCmdInParams
    {
        public uint cBufferSize;
        public IdeRegs irDriveRegs;
        public byte bDriveNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] bReserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] dwReserved;
        public byte bBuffer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    internal struct DriverStatus
    {
        public byte bDriverError;           // 错误码
        public byte bIDEStatus;             // IDE状态寄存器
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] bReserved;            // 保留
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] dwReserved;           // 保留
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SendCmdOutParams
    {
        public uint cBufferSize;                // 缓冲区字节数
        public DriverStatus DriverStatus;       // IDE寄存器组
        public IdSector bBuffer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 512)]
    internal struct IdSector
    {
        public ushort wGenConfig;               // WORD 0: 基本信息字
        public ushort wNumCyls;                 // WORD 1: 柱面数
        public ushort wReserved;
        public ushort wNumHeads;                // WORD 3: 磁头数
        public ushort wBytesPerTrack;
        //[MarshalAs(UnmanagedType.U2)]
        public ushort wBytesPerSector;
        public ushort wSectorsPerTrack;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] wVendorUnique;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] sSerialNumber;            // WORD 10-19:序列号
        public ushort wBufferType;              // WORD 20: 缓冲类型
        public ushort wBufferSize;              // WORD 21: 缓冲大小
        public ushort wECCSize;                 // WORD 22: ECC校验大小
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] sFirmwareRev;             // WORD 23-26: 固件版本
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] sModelNumber;             // WORD 27-46: 内部型号
        public ushort wMoreVendorUnique;        // WORD 7-9: 厂家设定值
        public ushort wDoubleWordIO;
        public ushort wCapabilities;
        public ushort wReserved1;               // WORD 50: 保留
        public ushort wPIOTiming;               // WORD 51: PIO时序
        public ushort wDMATiming;                // WORD 52: DMA时序
        public ushort wBS;
        public ushort wNumCurrentCyls;              // WORD 54: CHS可寻址的柱面数
        public ushort wNumCurrentHeads;             // WORD 55: CHS可寻址的磁头数
        public ushort wNumCurrentSectorsPerTrack;   // WORD 56: CHS可寻址每磁道扇区数
        public uint ulCurrentSectorCapacity;
        public ushort wMultSectorStuff;         // WORD 59: 多 扇区读写设定
        public uint ulTotalAddressableSectors;
        public ushort wSingleWordDMA;           // WORD 62: 单字节DMA支持能力
        public ushort wMultiWordDMA;            // WORD 63: 多字节DMA支持能力
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] bReserved;
    }

    #endregion

    ///<summary>
    /// ATAPI驱动器相关
    ///</summary>
    public class AtapiDevice
    {
        #region DllImport 

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        private static extern int DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, ref GetVersionOutParams lpOutBuffer, uint nOutBufferSize, ref uint lpBytesReturned, [Out] IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        private static extern int DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, ref SendCmdInParams lpInBuffer, uint nInBufferSize, ref SendCmdOutParams lpOutBuffer, uint nOutBufferSize, ref uint lpBytesReturned, [Out] IntPtr lpOverlapped);

        private const uint DFP_GET_VERSION = 0x00074080;
        private const uint DFP_SEND_DRIVE_COMMAND = 0x0007c084;
        private const uint DFP_RECEIVE_DRIVE_DATA = 0x0007c088;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint CREATE_NEW = 1;
        private const uint OPEN_EXISTING = 3;

        #endregion

        #region GetHddInfo

        ///<summary>
        /// 获得硬盘信息
        /// 在Windows 2000/2003下，需要Administrators组的权限
        ///</summary>
        ///<param name="driveIndex">硬盘序号</param>
        ///<returns>硬盘信息</returns>
        public static HardDiskInfo GetHddInfo(byte driveIndex)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                    return GetHddInfoFowWin9x(driveIndex);
                case PlatformID.Win32NT:
                    return GetHddInfoForWinNT(driveIndex);
                case PlatformID.Win32S:
                    throw new NotSupportedException("Win32S is not supported.");
                case PlatformID.WinCE:
                    throw new NotSupportedException("WinCE is not supported.");
                default:
                    throw new NotSupportedException("Unknown Platform.");
            }
        }

        private static HardDiskInfo GetHddInfoFowWin9x(byte driveIndex)
        {
            var vers = new GetVersionOutParams();
            var inParam = new SendCmdInParams();
            var outParam = new SendCmdOutParams();
            uint bytesReturned = 0;
            var hDevice = CreateFile(@"\\.\Smartvsd", 0, 0, IntPtr.Zero, CREATE_NEW, 0, IntPtr.Zero);
            if (hDevice == IntPtr.Zero)
            {
                throw new InvalidOperationException(string.Format("错误：位置{0}的磁盘打开失败.", driveIndex));
            }
            var rn = DeviceIoControl(hDevice, DFP_GET_VERSION, IntPtr.Zero, 0, ref vers, (uint)Marshal.SizeOf(vers), ref bytesReturned, IntPtr.Zero);
            if (0 == rn)
            {
                CloseHandle(hDevice);
                throw new InvalidOperationException(string.Format("DeviceIoControl失败：DFP_GET_VERSION."));
            }
            //If IDE identify command not supported, fails
            if (0 == (vers.fCapabilities & 1))
            {
                CloseHandle(hDevice);
                throw new Exception("错误：不支持的磁盘识别指令.");
            }
            if (0 != (driveIndex & 1))
            {
                inParam.irDriveRegs.bDriveHeadReg = 0xb0;
            }
            else
            {
                inParam.irDriveRegs.bDriveHeadReg = 0xa0;
            }
            if (0 != (vers.fCapabilities & (16 >> driveIndex)))
            {
                // We don't detect a ATAPI device.
                CloseHandle(hDevice);
                throw new InvalidOperationException(string.Format("不支持ATAPI类型设备信息获取.", driveIndex));
            }
            else
            {
                inParam.irDriveRegs.bCommandReg = 0xec;     // ATA的ID指令(IDENTIFY DEVICE)
            }
            inParam.bDriveNumber = driveIndex;
            inParam.irDriveRegs.bSectorCountReg = 1;
            inParam.irDriveRegs.bSectorNumberReg = 1;
            inParam.cBufferSize = 512;
            rn = DeviceIoControl(hDevice, DFP_RECEIVE_DRIVE_DATA, ref inParam, (uint)Marshal.SizeOf(inParam), ref outParam, (uint)Marshal.SizeOf(outParam), ref bytesReturned, IntPtr.Zero);
            if (0 == rn)
            {
                CloseHandle(hDevice);
                throw new InvalidOperationException(string.Format("DeviceIoControl失败：DFP_RECEIVE_DRIVE_DATA."));
            }
            CloseHandle(hDevice);
            return GetHardDiskInfo(outParam.bBuffer);
        }

        private static HardDiskInfo GetHddInfoForWinNT(byte driveIndex)
        {
            var vers = new GetVersionOutParams();
            var inParam = new SendCmdInParams();
            var outParam = new SendCmdOutParams();
            uint bytesReturned = 0;
            var hDevice = CreateFile(string.Format(@"\\.\PhysicalDrive{0}", driveIndex), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (hDevice == IntPtr.Zero)
            {
                throw new Exception(string.Format("错误：位置{0}的磁盘打开失败.", driveIndex));
            }
            var rn = DeviceIoControl(hDevice, DFP_GET_VERSION, IntPtr.Zero, 0, ref vers, (uint)Marshal.SizeOf(vers), ref bytesReturned, IntPtr.Zero);
            if (0 == rn)
            {
                CloseHandle(hDevice);
                throw new Exception(string.Format("错误：位置{0}的磁盘可能不存在.", driveIndex));
            }
            //If IDE identify command not supported, fails
            if (0 == (vers.fCapabilities & 1))
            {
                CloseHandle(hDevice);
                throw new Exception("错误：不支持的磁盘识别指令.");
            }
            //Identify the IDE drives
            if (0 != (driveIndex & 1))
            {
                inParam.irDriveRegs.bDriveHeadReg = 0xb0;
            }
            else
            {
                inParam.irDriveRegs.bDriveHeadReg = 0xa0;
            }
            if (0 != (vers.fCapabilities & (16 >> driveIndex)))
            {
                // We don't detect a ATAPI device.
                CloseHandle(hDevice);
                throw new InvalidOperationException(string.Format("不支持ATAPI类型设备信息获取.", driveIndex));
            }
            else
            {
                inParam.irDriveRegs.bCommandReg = 0xec;
            }
            inParam.bDriveNumber = driveIndex;
            inParam.irDriveRegs.bSectorCountReg = 1;
            inParam.irDriveRegs.bSectorNumberReg = 1;
            inParam.cBufferSize = 512;
            rn = DeviceIoControl(hDevice, DFP_RECEIVE_DRIVE_DATA, ref inParam, (uint)Marshal.SizeOf(inParam), ref outParam, (uint)Marshal.SizeOf(outParam), ref bytesReturned, IntPtr.Zero);
            if (0 == rn)
            {
                CloseHandle(hDevice);
                throw new InvalidOperationException(string.Format("DeviceIoControl失败：DFP_RECEIVE_DRIVE_DATA."));
            }
            CloseHandle(hDevice);
            return GetHardDiskInfo(outParam.bBuffer);
        }

        private static HardDiskInfo GetHardDiskInfo(IdSector phdinfo)
        {
            var hdd = new HardDiskInfo();
            ChangeByteOrder(phdinfo.sModelNumber);
            hdd.ModuleNumber = Encoding.ASCII.GetString(phdinfo.sModelNumber).Trim();
            ChangeByteOrder(phdinfo.sFirmwareRev);
            hdd.Firmware = Encoding.ASCII.GetString(phdinfo.sFirmwareRev).Trim();
            ChangeByteOrder(phdinfo.sSerialNumber);
            hdd.SerialNumber = Encoding.ASCII.GetString(phdinfo.sSerialNumber).Trim();
            hdd.AddressableSectors = phdinfo.ulTotalAddressableSectors;// / 2 / 1024;
            return hdd;
        }

        /// <summary>
        /// 将串中的字符两两颠倒
        /// 原因是ATA/ATAPI中的WORD，与Windows采用的字节顺序相反
        /// 驱动程序中已经将收到的数据全部反过来，我们来个负负得正
        /// </summary>
        /// <param name="charArray"></param>
        private static void ChangeByteOrder(byte[] charArray)
        {
            // 两两颠倒
            byte temp;
            for (int i = 0; i < charArray.Length; i += 2)
            {
                temp = charArray[i];
                charArray[i] = charArray[i + 1];
                charArray[i + 1] = temp;
            }
        }

        #endregion

    }
}
