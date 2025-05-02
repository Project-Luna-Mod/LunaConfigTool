using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

namespace PLunaConfigTool
{
    class Program
    {
        const int HWND_BROADCAST = 0xFFFF;
        const int WM_SETTINGCHANGE = 0x001A;
        const int WM_USER = 0x0400;
        const int SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);

        [STAThread]
        static void Main()
        {
            Console.Title = "Configure Project Luna";
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Welcome to Project Luna's configurator!\nYou can configure these options in this configurator:\n");

            Console.WriteLine("Account settings:");
            Console.WriteLine("[1] Change your username");
            Console.WriteLine("[2] Change your account picture");
            Console.WriteLine("[3] Add or change a password\n");
            Console.WriteLine("Computer settings:");
            Console.WriteLine("[4] Change the computer's name\n");
            Console.WriteLine("Miscellaneous settings:");
            Console.WriteLine("[5] Enable/Disable Snap functionality\n");
            Console.ResetColor();

            Console.WriteLine("[0] to exit this utility");
            Console.Write("Enter choice (0-5): ");
            var choice = Console.ReadLine();

            Console.Clear();

            switch (choice)
            {
                case "0": Environment.Exit(0); break;
                case "1": ChangeUsername(); break;
                case "2": ChangeAccPic(); break;
                case "3": ChangePassword(); break;
                case "4": ChangePCName(); break;
                case "5": EnableSnap(); break;
                default: Console.WriteLine("Invalid choice. Please restart the app."); break;
            }

            Thread.Sleep(5000);
        }

        static void ChangeUsername()
        {
            Console.Write("Enter a new username for your account: ");
            var username = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("A username cannot be empty. Please restart the app and enter a username.");
                return;
            }

            RunCommand($"net user Administrator /fullname:\"{username}\"");
            Console.WriteLine($"Your username has been changed to {username}. Log out to see the changes");
        }

        static void ChangePassword()
        {
            Console.Write("Enter a new password for your account (blank to reset): ");
            var password = Console.ReadLine();

            RunCommand($"net user Administrator \"{password}\"");
            Console.WriteLine(password == ""
                ? "Password reset to blank."
                : "Your password has been changed. Log out to see the changes.");
        }

        static void ChangePCName()
        {
            Console.Write("Enter a new name for your computer: ");
            var newName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("Computer name cannot be empty.");
                return;
            }

            try
            {
                var scope = new System.Management.ManagementScope(@"\\.\root\cimv2");
                scope.Connect();

                var query = new System.Management.SelectQuery("SELECT * FROM Win32_ComputerSystem");
                using var searcher = new System.Management.ManagementObjectSearcher(scope, query);

                foreach (System.Management.ManagementObject obj in searcher.Get())
                {
                    var result = obj.InvokeMethod("Rename", new object[] { newName });
                    if (result != null && (uint)result == 0)
                    {
                        Console.WriteLine($"Computer name changed to {newName}.");
                        Console.WriteLine("You must restart your computer for the change to take effect.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to change computer name. Error code: {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ChangeAccPic()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select your profile picture",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string selectedFile = ofd.FileName;
            string outputDir = @"C:\ProgramData\Microsoft\User Account Pictures";

            Directory.CreateDirectory(outputDir);

            int[] sizes = { 32, 40, 48, 192, 448 };

            using Image original = Image.FromFile(selectedFile);
            foreach (int size in sizes)
            {
                string savePath;

                if (size == 448)
                {
                    savePath = Path.Combine(outputDir, "user.png");
                    using (Bitmap png = new Bitmap(original, new Size(size, size)))
                    {
                        png.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    savePath = Path.Combine(outputDir, "user.bmp");
                    using (Bitmap bmp = new Bitmap(original, new Size(size, size)))
                    {
                        bmp.Save(savePath, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                }
                else
                {
                    savePath = Path.Combine(outputDir, $"user-{size}.png");
                    using (Bitmap png = new Bitmap(original, new Size(size, size)))
                    {
                        png.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                SendMessageTimeout(
                    (IntPtr)HWND_BROADCAST,
                    WM_SETTINGCHANGE,
                    UIntPtr.Zero,
                    "Environment",
                    SMTO_ABORTIFHUNG,
                    1000,
                    out _);
                SendMessageTimeout(
                    (IntPtr)HWND_BROADCAST,
                    WM_USER,
                    UIntPtr.Zero,
                    null,
                    SMTO_ABORTIFHUNG,
                    1000,
                    out _);
            }
        }

        static void EnableSnap()
        {
            const string keyPath = @"HKEY_CURRENT_USER\Control Panel\Desktop";
            const string valueName = "WindowArrangementActive";
            string currentValue = Microsoft.Win32.Registry.GetValue(keyPath, valueName, "1")?.ToString() ?? "1";
            string newValue = currentValue == "1" ? "0" : "1";

            Microsoft.Win32.Registry.SetValue(keyPath, valueName, newValue, Microsoft.Win32.RegistryValueKind.String);

            Console.WriteLine(newValue == "1" ? "Snap Assist has been enabled." : "Snap Assist has been disabled.");
            Console.WriteLine("You need to log off for the changes to take effect.");
        }

        static void RunCommand(string command)
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();
        }
    }
}