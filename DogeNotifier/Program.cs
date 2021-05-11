using System.Windows.Forms;
using System.Drawing;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace DogeNotifier
{
    class Program
    {
        static NotifyIcon notifyIcon = new NotifyIcon();
        static bool Visible = true;
        static void Main(string[] args)
        {
            notifyIcon.DoubleClick += (s, e) =>
            {
                Visible = !Visible;
                SetConsoleWindowVisibility(Visible);
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, (s, e) => { Application.Exit(); });
            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Visible = true;
            notifyIcon.Text = Application.ProductName;

            Console.WriteLine("Running!");

            SetConsoleWindowVisibility(false);

            updateDoge();

            string Toast = "";
            Toast = "<toast><visual><binding template=\"ToastImageAndText01\"><text id = \"1\" >";
            Toast += "DogeNotifier is running as a tray icon!";
            Toast += "</text></binding></visual></toast>";
            XmlDocument tileXml = new XmlDocument();
            tileXml.LoadXml(Toast);
            var toast = new ToastNotification(tileXml);
            ToastNotificationManager.CreateToastNotifier("Doge toast").Show(toast);

            Application.Run();

            while (true)
            {
                try
                {
                    Console.ReadLine();
                }
                catch
                {

                }
                Thread.Sleep(1000);
            }
        }


        public static int lastNotification = 0;

        public static void updateDoge()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        WebClient client = new WebClient();
                        string coingeckoRespJson = client.DownloadString("https://api.coingecko.com/api/v3/simple/price?ids=dogecoin&vs_currencies=gbp");
                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(coingeckoRespJson), new System.Xml.XmlDictionaryReaderQuotas());
                        var root = XElement.Load(jsonReader);
                        string currentDogePrice = root.XPathSelectElement("//dogecoin/gbp").Value;
                        string penniesOnly = currentDogePrice.Split('.')[1].Substring(0, 2);

                        var color = Color.White;
                        if (Convert.ToInt32(penniesOnly) >= 40)
                        {
                            color = Color.Green;
                        }
                        else if (Convert.ToInt32(penniesOnly) >= 34)
                        {
                            color = Color.Yellow;
                        }
                        else if (Convert.ToInt32(penniesOnly) <= 33)
                        {
                            color = Color.Red;
                        }

                        Font fontToUse = new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel);
                        Brush brushToUse = new SolidBrush(color);
                        Bitmap bitmapText = new Bitmap(16, 16);
                        Graphics g = System.Drawing.Graphics.FromImage(bitmapText);
                        IntPtr hIcon;
                        g.Clear(Color.Transparent);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                        g.DrawString(penniesOnly, fontToUse, brushToUse, -4, -2);
                        hIcon = (bitmapText.GetHicon());
                        notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
                        notifyIcon.Visible = true;
                        notifyIcon.Text = "Doge price: £" + currentDogePrice;

                        if (Convert.ToInt32(penniesOnly) <= 33)
                        {
                            if (lastNotification >= 6)
                            {
                                lastNotification = 0;
                            }
                            if (lastNotification == 0)
                            {
                                // if you dont want the notifications, delete everything in this block

                                string Toast = "";
                                Toast = "<toast><visual><binding template=\"ToastImageAndText01\"><text id = \"1\" >";
                                Toast += "Doge is currently Low! At £" + currentDogePrice;
                                Toast += "</text></binding></visual></toast>";
                                XmlDocument tileXml = new XmlDocument();
                                tileXml.LoadXml(Toast);
                                var toast = new ToastNotification(tileXml);
                                ToastNotificationManager.CreateToastNotifier("Doge toast").Show(toast);

                                //stop deleting here ^^
                            }
                        }
                    }
                    catch
                    {

                    }
                    Thread.Sleep(5000);
                }
            }).Start();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void SetConsoleWindowVisibility(bool visible)
        {
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                if (visible) ShowWindow(hWnd, 1);
                else ShowWindow(hWnd, 0);
            }
        }
    }
}
