using BuildVision.UI.Models;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BuildVision.UI.Helpers
{
    public static class StyleConverting
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToMediaBitmap(this System.Drawing.Image source)
        {
            BitmapSource bitSrc;
            using (var bitmap = new System.Drawing.Bitmap(source))
            {
                bitSrc = bitmap.ToMediaBitmap();
            }

            return bitSrc;
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToMediaBitmap(this System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc;
            IntPtr hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        public static SortOrder ToMedia(this ListSortDirection? listSortDirection)
        {
            switch (listSortDirection)
            {
                case null:
                    return SortOrder.None;
                case ListSortDirection.Ascending:
                    return SortOrder.Ascending;
                case ListSortDirection.Descending:
                    return SortOrder.Descending;
                default:
                    throw new ArgumentOutOfRangeException("listSortDirection");
            }
        }

        public static ListSortDirection? ToSystem(this SortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case SortOrder.None:
                    return null;
                case SortOrder.Ascending:
                    return ListSortDirection.Ascending;
                case SortOrder.Descending:
                    return ListSortDirection.Descending;
                default:
                    throw new ArgumentOutOfRangeException("sortOrder");
            }
        }
    }
}