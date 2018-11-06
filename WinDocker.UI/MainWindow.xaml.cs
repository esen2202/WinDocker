using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing.IconLib;
using Microsoft.Win32;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WinDocker.UI
{


    #region Acrylic Efect
    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }
    #endregion



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Acrylic Efect
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy { AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        #endregion

        private MultiIcon mMultiIcon = new MultiIcon();


        private ObservableCollection<DockItem> _DockItems = new ObservableCollection<DockItem>();
        public ObservableCollection<DockItem> DockItems
        {
            get
            {
                return _DockItems;
            }
            set
            {
                _DockItems = value;

            }
        }

        List<DockItem1> imageList = new List<DockItem1>();

        public MainWindow()
        {
            InitializeComponent();

            this.Topmost = true;

            this.Top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height;

            this.Loaded += MainWindow_Loaded;

            DockFill.Initialize();
            DockFill.GetItems().ForEach(x =>
            {

                DockItems.Add(x);
            });

            this.DataContext = this;
            // Automatically resize width relative to content
            this.SizeToContent = SizeToContent.Width;
            //this.Width = (DockItems.Count + 1) * 55;
            this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - (Convert.ToInt32( this.SizeToContent) / 2);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();


            mMultiIcon.Load(@"C:\Users\erkan\AppData\Local\GitHubDesktop\GitHubDesktop.exe");
            mMultiIcon.Load(@"D:\Portable\Adobe\InDesign CC 2018\InDesign13\InDesignPortable.exe");
            mMultiIcon.Load(@"C:\Windows\System32\notepad.exe");


            List<IconImage> iconImageList = new List<IconImage>();


            foreach (var item in mMultiIcon[0])
            {
                iconImageList.Add(item); // Dosyay ya ait bütün boyutlardaki iconlar listeye eklenir
            }

            IconImage iconImage = iconImageList.OrderByDescending(x => x.Size.Height).First();

            //DP_Container.Children.Add(new System.Windows.Controls.Image()
            //{
            //    Source = ToImageSource(iconImage.Icon),
            //    Width = 50,
            //    Stretch = Stretch.Uniform
            //});


            Icon largeIcon = IconTools.GetIconForExtension(".html", ShellIconSize.LargeIcon);

            //DockPanel.SetDock(DP_Container, Dock.Left);
            //GetIcon(@"C:\Users\erkan\OneDrive\Masaüstü").ForEach(x =>
            //{
            //    DP_Container.Children.Add(new System.Windows.Controls.Image()
            //    {
            //        Source = ToImageSource(largeIcon),

            //        Height = 60,
            //        Stretch = Stretch.Uniform
            //    });
            //});
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ToImageSource(Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }


        public List<DockItem1> GetIcon(string directory)
        {
            string[] dir = Directory.GetFiles(directory);

            var data = new List<DockItem1>();

            foreach (var filePath in dir)
            {
                var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            sysicon.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();

                var itm = new DockItem1() { Icon = bmpSrc, Label = filePath };

                data.Add(itm);
            }

            return data;
        }
    }

    public class DockItem1
    {
        public ICommand Command { get; set; }
        public String Label { get; set; }
        public BitmapSource Icon { get; set; }
    }




    public class DockItem
    {
        public ICommand Command { get; set; }
        public String Label { get; set; }
        public ImageSource Icon { get; set; }
    }

    public static class DockFill
    {

        static List<DockItem> dockItems = new List<DockItem>();


        public static void Initialize()
        {
            dockItems.Add(new DockItem()
            {
                Icon = ToImageSourceWithMemoryStream(GetIconFromFile(@"C:\Users\erkan\AppData\Local\GitHubDesktop\GitHubDesktop.exe").Icon),
                Label = "GitHub"
            });

            dockItems.Add(new DockItem()
            {
                Icon = ToImageSourceWithMemoryStream(GetIconFromFile(@"D:\Portable\Adobe\After Effect CC 2018\After Effects 15\AfterFXPortable.exe").Icon),
                Label = "After Effects"
            });

            dockItems.Add(new DockItem()
            {
                Icon = ToImageSourceWithMemoryStream(GetIconFromFile(@"D:\Portable\Adobe\InDesign CC 2018\InDesign13\InDesignPortable.exe").Icon),
                Label = "InDesign"
            });

            //dockItems.Add(new DockItem()
            //{
            //    Icon = ToImageSourceWithMemoryStream(GetIconFromFile(@"C:\Users\erkan\Downloads\imusic-win_setup_full2400.exe").Icon),
            //    Label = "IMusic"
            //});

            dockItems.Add(new DockItem()
            {
                Icon = ToImageSourceWithMemoryStream(GetIconFromFile(@"D:\Program Files\Esen Software\ScopeFolder\ScopeFolder.exe").Icon),
                Label = "Scope Folder"
            });

            dockItems.Add(new DockItem()
            {
                Icon = ToImageSourceWithMemoryStream(GetIconFromFile(@"D:\Program Files\Esen Software\WordNote\WordNote.exe").Icon),
                Label = "Word Note"
            });



        }

        public static IconImage GetIconFromFile(string path)
        {
            MultiIcon multiIcon = new MultiIcon();

            multiIcon.Load(path);

            SingleIcon singleIcon = multiIcon[0];

            List<IconImage> iconImages = new List<IconImage>();

            foreach (var item in singleIcon)
            {
                iconImages.Add(item);
            }

            IconImage iconImage = iconImages.OrderByDescending(x => x.Size.Height).First();


            return iconImage;
        }

        public static List<DockItem> GetItems()
        {
            return dockItems;
        }


        public static ImageSource ToImageSourceWithMemoryStream(Icon icon)
        {
            using (System.IO.MemoryStream iconstream = new System.IO.MemoryStream())
            {
                icon.Save(iconstream);
                iconstream.Seek(0, System.IO.SeekOrigin.Begin);
                return System.Windows.Media.Imaging.BitmapFrame.Create(iconstream);
            }

        }


    }

}
