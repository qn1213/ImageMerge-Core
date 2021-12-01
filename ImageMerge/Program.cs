using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Util;

namespace ImageMerge
{
    public class Program
    {
        private string[] suppoort_extension = { ".png", ".jpg", ".jpeg" };

        private const int MAX_WIDTH = 10000;
        private const int MAX_HEIGHT = 10000;

        private bool rotate = false;
        private bool isFail = false;

        private string imgFolder;
        private string outPutPath;
        private readonly string outPutFolder = "output";

        // 여러 확장자로 할건지 단일 확장자로 할건지
        private List<string> filter;


        private int folderCnt = 0;
        private string log = null;


        public bool IsFail { get => isFail; protected set => isFail = value; }
        public int FolderCnt { get => folderCnt; protected set => FolderCnt = value; }
        public string Log { get => log; protected set => log = value; }

        public Program(string[] args)
        {
            if (args[0] == "1" || args[0] == "0")
                rotate = args[0] == "1";
            else { Print(); isFail = true; return; }

            this.imgFolder = args[1];
            this.outPutPath = this.imgFolder + "\\" + outPutFolder;

            if (!Directory.Exists(imgFolder))
                Console.WriteLine("[ERROR] Check Image Folder");

            if (!Directory.Exists(this.outPutPath))
                Directory.CreateDirectory(outPutPath);

            if (args.Length == 3)
                filter = new List<string> { args[2] };
            else if (args.Length == 2)
                filter = suppoort_extension.ToList();
            else
                isFail = true;
        }

        public Program(bool rotate, string path, string ext = null)
        {
            this.rotate = rotate;
            this.imgFolder = path;
            this.outPutPath = this.imgFolder + "\\" + outPutFolder;

            if (!Directory.Exists(imgFolder))
            {
                Console.WriteLine("[ERROR] Check Image Folder");
                log += "[ERROR] Check Image Folder\n";
                IsFail = true;
                return;
            }

            if (!Directory.Exists(this.outPutPath))
                Directory.CreateDirectory(outPutPath);

            if (ext != null) this.filter = new List<string> { ext };
            else this.filter = suppoort_extension.ToList();
        }

        static void Main(string[] args)
        {
            if (args.Length > 3 || args.Length <= 1)
            {
                Print();
                return;
            }

            Program combineImage = new Program(args);
            if (combineImage.isFail)
                return;

            combineImage.Run();
        }

        
        public static void Print()
        {
            Console.WriteLine("===================[CLI Usage]===================");
            Console.WriteLine("| (0~1) (ImageFolder) (Image_EXT or Put Empty)");
            Console.WriteLine("| Argument 1 : 0(Width) 1(Length)"); // 가로 0, 세로 1 (좌우, 위아래)
            Console.WriteLine("| Argument 2 : Image Folder");       // 분활되어있는 이미지 폴더
            Console.WriteLine("| Argument 3 : Image Extension(optional)");    // 특정 확장자만 모으고 싶다면 적으면 됨
            Console.WriteLine("| ex1) 0 C:\\IMG_FOLDER png");
            Console.WriteLine("| ex2) 1 C:\\IMG_FOLDER");
            Console.WriteLine("|");
            Console.WriteLine("|====================[NOTE]=======================");
            Console.WriteLine("| [Suppoort File] : png, jpg, jpeg");
            Console.WriteLine("| [File] : You must input at least [2 files]");
            Console.WriteLine("| [OutPut Folder] : ImageFolder\\output");
            Console.WriteLine("| [OutPut File] : final_N.png");
            Console.WriteLine("| [OutPut File] : Cut image from 10,000 pixels.");
            Console.WriteLine("=================================================");
        }

        public void Run()
        {
            DirectoryInfo di = new DirectoryInfo(this.imgFolder);

            int fileCnt = di.GetFiles().Count();
            if (fileCnt <= 1)
            {
                Console.WriteLine("[ERROR] File Count : {0}\n\n", fileCnt);
                Print();

                log += "[ERROR] File Count : " + fileCnt + "\n";
                isFail = true;
                return;
            }
            Console.WriteLine("File Count : {0}", fileCnt);
            log += "File Count : " + fileCnt + "\n";

            string[] fileInfoList = GetFileByExtensionName_String(imgFolder, filter);
            if (fileInfoList == null)
            {
                Console.WriteLine("[ERROR] File Info List is Null");
                log += "[ERROR] File Info List is Null\n";
                isFail = true;
                return;
            }

            // string 배열 정렬
            Array.Sort(fileInfoList, new StringAsNumericComparer());

            // 이미지 처리
            List<Bitmap> resultImgs = CombineBitmap(fileInfoList);

            try
            { // 여기서 저장
                int i = 0;
                foreach (Bitmap bmp in resultImgs)
                    bmp.Save((outPutPath + "\\final_" + i++ + ".png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                log += ex.Message + "\n";
                isFail = true;
                return;
            }

            foreach (Bitmap img in resultImgs)
                img.Dispose();

            Console.WriteLine("Done!");
            log += "Done!\n";
        }

        public List<string[]> GetFileByExtensionName_List(string path, List<string> filterList)
        {
            List<string[]> list = new List<string[]>();
            var ext = filterList;

            //foreach (string file in Directory.GetFiles(path, "*.*").Where(s => ext.Any(e => s.ToLower().EndsWith(e))).OrderByDescending(f => new FileInfo(f).LastWriteTime))
            foreach (string file in Directory.GetFiles(path, "*.*").Where(s => ext.Any(e => s.ToLower().EndsWith(e))))
            {
                FileInfo f = new FileInfo(file);
                list.Add(new string[] { file });
            }
            return list;
        }

        public string[] GetFileByExtensionName_String(string path, List<string> filterList)
        {
            List<string> list = new List<string>();
            var ext = filterList;

            foreach (string file in Directory.GetFiles(path, "*.*").Where(s => ext.Any(e => s.ToLower().EndsWith(e))))
            {// 특정 확장자만 가져오는거. => 실행 시 특별한 입력 없으면 기본 확장자 다 긁어다옴
                FileInfo f = new FileInfo(file);
                list.Add(file);
            }
            return list.ToArray();
        }

        public List<Bitmap> CombineBitmap(string[] files)
        {
            List<Bitmap> imgs = new List<Bitmap>();
            Bitmap procImg = null;

            List<Bitmap> finalImgs = new List<Bitmap>();

            try
            {
                int width = 0;
                int height = 0;
                int fileCnt = 0;

                bool oneFile = false;
                bool fewFile = false;

                foreach (string img in files)
                {
                    Bitmap bitmap = new Bitmap(img);
                    fileCnt++;

                    oneFile = bitmap.Width >= MAX_WIDTH || bitmap.Height >= MAX_WIDTH; // 이미지 파일 1개의 크기가 맥스값이 넘을 때
                    fewFile = width >= MAX_WIDTH || height >= MAX_HEIGHT;              // 여러 이미지 파일을 합쳤는데 크기가 맥스값이 넘을 때
                    if (oneFile || fewFile)
                    {
                        if (oneFile)
                        {
                            width = bitmap.Width;
                            height = bitmap.Height;
                        }
                        else if (fewFile)
                        {
                            if (this.rotate)
                                height += bitmap.Height;
                            else
                                width += bitmap.Width;
                        }

                        imgs.Add(bitmap);
                        procImg = new Bitmap(width, height);

                        finalImgs.Add(DrawImage(procImg, imgs));
                        if (fileCnt >= files.Count()) break;

                        width = 0;
                        height = 0;
                        imgs.Clear();
                        continue;
                    }

                    if (this.rotate)
                    {
                        width = width > bitmap.Width ? width : bitmap.Width;
                        height += bitmap.Height;
                    }
                    else
                    {
                        width += bitmap.Width;
                        height = bitmap.Height > height ? bitmap.Height : height;
                    }

                    imgs.Add(bitmap);
                }

                procImg = new Bitmap(width, height);
                finalImgs.Add(DrawImage(procImg, imgs));

                return finalImgs;
            }
            catch (Exception ex)
            {
                if (procImg != null)
                    procImg.Dispose();

                Console.WriteLine(ex.Message);
                log += ex.Message + "\n";
                isFail = true;
                throw;
            }
            finally
            {
                foreach (Bitmap img in imgs)
                    img.Dispose();
            }
        }

        public Bitmap DrawImage(Bitmap finalImg, List<Bitmap> imgs)
        {
            Bitmap bitmap = finalImg;
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);

                int offset = 0;
                foreach (Bitmap img in imgs)
                {
                    if (this.rotate)
                    {
                        g.DrawImage(img, new Rectangle(0, offset, img.Width, img.Height));
                        offset += img.Height;
                    }
                    else
                    {
                        g.DrawImage(img, new Rectangle(offset, 0, img.Width, img.Height));
                        offset += img.Width;
                    }
                }
            }
            return bitmap;
        }
    }
}