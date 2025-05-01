using System;
using System.Diagnostics;
using System.Threading;

namespace PLunaConfigTool
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Configure Project Luna";
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Welcome to Project Luna's configurator!\nYou can configure this in this configurator:\n");

            Console.WriteLine("Account settings:");
            Console.WriteLine("[1] Change your username");
            Console.WriteLine("[2] Change your account picture");
            Console.WriteLine("[3] Add or change a password\n");
            Console.WriteLine("Computer settings:");
            Console.WriteLine("[4] Change the computer's name\n");
            Console.ResetColor();

            Console.Write("Enter choice (1-3): ");
            var choice = Console.ReadLine();

            Console.Clear();

            switch (choice)
            {
                case "1": ChangeUsername(); break;
                case "2": ChangeAccPic(); break;
                case "3": ChangePassword(); break;
                case "4": ChangePCName(); break;
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

        }

        static void ChangeAccPic()
        {

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