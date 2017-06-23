using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace BuildVision.Common
{
    public static class ClipboardHelper
    {
        /// <summary>
        /// Copy files to the Clipboard.
        /// </summary>
        /// <exception cref="Win32Exception">WinAPI exception.</exception>
        public static void SetFileDropList(string[] filePaths)
        {
            if (filePaths == null)
                throw new ArgumentNullException("filePaths");

            if (filePaths.Length == 0)
                return;

            RecodingFilePaths(filePaths);

            bool success = CopyFilesToClipboard(filePaths, new POINT(), out int error);
            if (!success)
                throw new Win32Exception(error);
        }

        private static void RecodingFilePaths(string[] filePaths)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                // Get UTF8 bytes by reading each byte with ANSI encoding
                byte[] utf8Bytes = Encoding.Default.GetBytes(filePaths[i]);

                // Convert UTF7 bytes to UTF16 bytes
                byte[] utf16Bytes = Encoding.Convert(Encoding.UTF7, Encoding.Unicode, utf8Bytes);

                // Return UTF16 bytes as UTF16 string
                filePaths[i] = Encoding.Unicode.GetString(utf16Bytes);
            }
        }

        #region Clipboard WinAPI

        #pragma warning disable 169
        #pragma warning disable 414
        // ReSharper disable InconsistentNaming, UnusedMember.Local, NotAccessedField.Local

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        private enum CLIPFORMAT
        {
            CF_TEXT = 1,
            CF_BITMAP = 2,
            CF_METAFILEPICT = 3,
            CF_SYLK = 4,
            CF_DIF = 5,
            CF_TIFF = 6,
            CF_OEMTEXT = 7,
            CF_DIB = 8,
            CF_PALETTE = 9,
            CF_PENDATA = 10,
            CF_RIFF = 11,
            CF_WAVE = 12,
            CF_UNICODETEXT = 13,
            CF_ENHMETAFILE = 14,
            CF_HDROP = 15,
            CF_LOCALE = 16,
            CF_MAX = 17,
            CF_OWNERDISPLAY = 0x80,
            CF_DSPTEXT = 0x81,
            CF_DSPBITMAP = 0x82,
            CF_DSPMETAFILEPICT = 0x83,
            CF_DSPENHMETAFILE = 0x8E,
        }

        private struct POINT
        {
            public int x;
            public int y;
        }

        private struct DROPFILES
        {
            public int pFiles;
            public POINT pt;
            public bool fNC;
            public int fWide;
        }

        private static bool CopyFilesToClipboard(string[] strFiles, POINT pt, out int error)
        {
            var dropfiles = new DROPFILES();
            int intFile;

            // Calculate total data length
            int intDataLen = 0;
            for (intFile = 0; intFile <= strFiles.GetUpperBound(0); intFile++)
                intDataLen += strFiles[intFile].Length + 1;
            intDataLen++; // Terminating double zero

            var bData = new byte[intDataLen];
            int intPos = 0;

            // Build null terminated list of files
            for (intFile = 0; intFile <= strFiles.GetUpperBound(0); intFile++)
            {
                int intChar;
                for (intChar = 0; intChar < strFiles[intFile].Length; intChar++)
                    bData[intPos++] = (byte) strFiles[intFile][intChar];
                bData[intPos++] = 0;
            }
            bData[intPos] = 0; // Terminating double zero

            // Allocate and get pointer to global memory
            int intTotalLen = Marshal.SizeOf(dropfiles) + intDataLen;
            IntPtr ipGlobal = Marshal.AllocHGlobal(intTotalLen);
            if (ipGlobal == IntPtr.Zero)
            {
                error = Marshal.GetLastWin32Error();
                return false;
            }

            // Build DROPFILES structure in global memory.
            dropfiles.pFiles = Marshal.SizeOf(dropfiles);
            dropfiles.pt = pt;
            dropfiles.fNC = false;
            dropfiles.fWide = 0;
            Marshal.StructureToPtr(dropfiles, ipGlobal, true);
            var ipNew = new IntPtr(ipGlobal.ToInt32() + Marshal.SizeOf(dropfiles));
            Marshal.Copy(bData, 0, ipNew, intDataLen);

            // Open and empty clipboard
            if (!OpenClipboard(IntPtr.Zero))
            {
                error = Marshal.GetLastWin32Error();
                return false;
            }

            EmptyClipboard();

            // Copy data to clipboard
            var result = SetClipboardData((int)CLIPFORMAT.CF_HDROP, ipGlobal);
            bool success = (result != IntPtr.Zero);
            error = Marshal.GetLastWin32Error();

            if (!success)
                Marshal.FreeHGlobal(ipGlobal);

            // Clean up
            CloseClipboard();

            return success;
        }

        #endregion
    }
}