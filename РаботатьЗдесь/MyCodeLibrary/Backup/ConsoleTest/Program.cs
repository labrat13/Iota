using System.Threading;
using MyCodeLibrary;
using MyCodeLibrary.FileOperations;
using System;
using System.IO;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ////test for windows
            //CWindowProcessor.СвернутьВсеОкна();
            //Thread.Sleep(5000);
            //CWindowProcessor.РазвернутьВсеОкна();
            //Thread.Sleep(5000);
            ////make screenshot
            //System.Drawing.Bitmap b = CImageProcessor.GetScreenShot(System.Windows.Forms.Screen.PrimaryScreen);
            //b.Save(@"c:\my_screenshot.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            //test for recycle bin
            //ShellFileOperations.EmptyRecycleBin(IntPtr.Zero, null, ShellFileOperations.EmptyRecycleBinFlags.None); 

            //test for function
            //String rootFolder = "c:\\Temp";
            //for (int i = 0; i < 5; i++)
            //{
            //    String newFolderTitle = CFileOperations.getTitleOfNewSubfolder(rootFolder, "folder-");
            //    Directory.CreateDirectory(Path.Combine(rootFolder, newFolderTitle));
            //    Console.WriteLine(newFolderTitle);
            //}
            /*  выводит в консоль:
                folder-1
                folder-2
                folder-3
                folder-4
                folder-5
                Press Enter to finish...
             * */

            TestAssemblyInfo();

            Console.WriteLine("Press Enter to finish...");
            Console.ReadLine();
            return;
        }

        private static void TestAssemblyInfo()
        {
            AssemblyInfo ai = new AssemblyInfo(AssemblyInfo.getAssembly());
            AssemblyInfoEx aix = new AssemblyInfoEx(AssemblyInfoEx.getAssembly());

            return;
        }


    }
}
