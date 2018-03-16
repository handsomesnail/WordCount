using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WordCount {

    /// <summary>参数状态</summary>
    public enum ArgsStatus {
        Uninitialized = 0,
        NomalOption = 1,
        SourceFile = 2,
        OutputOption = 3,
        OutputFile = 4,
        Finish = 5
    }

    public class Program {
        public static void Main(string[] args) {
            Entrance(args);
            //Entrance(new string[] { "-x" });
            //Console.ReadKey();
        }
        
        /// <summary>入口测试函数</summary>
        private static void Entrance(string[] args) {
            if (args.Length == 0)
                return;
            List<string> options = new List<string>();//-c -w -l选项
            string sourceFile = null; //源文件名
            string outputFile = "result.txt"; //输出文件名
            string source = null; //源文件字符串
            bool isOutput = true;//是否输出到文件
            bool isSelectFile = false;//-x选项选择文件
            ArgsStatus status = ArgsStatus.Uninitialized;
            //遍历参数
            foreach(var s in args) {
                if (status== ArgsStatus.Uninitialized||status==ArgsStatus.NomalOption) {
                    if(status== ArgsStatus.Uninitialized && s == "-x") {
                        status = ArgsStatus.Finish;
                        options.Add("-c");
                        options.Add("-w");
                        options.Add("-l");
                        isOutput = false;
                        isSelectFile = true;
                    }
                    else if (s == "-c" || s == "-w" || s == "-l") {
                        if (options.Contains(s)) {
                            LogArgumentError(0x0001);
                            return;
                        }
                        options.Add(s);
                        status = ArgsStatus.NomalOption;
                    }
                    else if (status == ArgsStatus.NomalOption) {
                        sourceFile = s;
                        status = ArgsStatus.OutputOption;
                    }
                    else {
                        LogArgumentError(0x0002);
                        return;
                    }
                }
                else if(status == ArgsStatus.OutputOption) {
                    if (s == "-o") {
                        status = ArgsStatus.OutputFile;
                    }
                    else {
                        LogArgumentError(0x0003);
                        return;
                    }
                }
                else if (status == ArgsStatus.OutputFile) {
                    outputFile = s;
                    status = ArgsStatus.Finish;
                }
                else if(status == ArgsStatus.Finish) {
                    LogArgumentError(0x0004);
                    return;
                }
            }

            //最终必须停留在这两个状态
            if (status != ArgsStatus.OutputOption && status != ArgsStatus.Finish) {
                LogArgumentError(0x0005);
                return;
            }

            try {
                if (!isSelectFile) {
                    source = Read(sourceFile);
                }
                else source = Read(out sourceFile);
            }
            catch (IOException) {
                Console.WriteLine(sourceFile+"不存在");
                return;
            }

            int c_result = -1;
            int w_result = -1;
            int l_result = -1;

            foreach(var option in options) {
                if (option == "-c")
                    c_result = CountChar(source);
                else if (option == "-w")
                    w_result = CountWord(source);
                else if (option == "-l")
                    l_result = CountLine(source);
                else throw new Exception();
            }

            if (c_result != -1)
                Console.WriteLine(sourceFile + ", " + "字符数: " + c_result);
            if (w_result != -1)
                Console.WriteLine(sourceFile + ", " + "单词数: " + w_result);
            if (w_result != -1)
                Console.WriteLine(sourceFile + ", " + "行数: " + l_result);

            if (isOutput) {
                string content = "";
                if (c_result != -1)
                    content += (sourceFile + ", " + "字符数: " + c_result + "\r\n");
                if (w_result != -1)
                    content += (sourceFile + ", " + "单词数: " + w_result + "\r\n");
                if (w_result != -1)
                    content += (sourceFile + ", " + "行数: " + l_result + "\r\n");
                Write(outputFile, content);
                //Console.WriteLine("输出到文件" + outputFile + "成功");
            }

        }

        /// <summary>通过图形界面读取文件 </summary>
        public static string Read(out string fileName) {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            //ofn.filter = "jpg文件(*.jpg)\0*.jpg\0png文件(*.png)\0*.png\0";
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.title = "选择文件";
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            if (LocalDialog.GetOpenFileName(ofn)) {
                fileName = ofn.fileTitle;
                using (FileStream fs = new FileStream(ofn.file, FileMode.Open, FileAccess.Read)) {
                    using (StreamReader sw = new StreamReader(fs, Encoding.UTF8)) {
                        return sw.ReadToEnd();
                    }
                }
            }
            throw new System.Exception("用户没有选择图片");
        }

        /// <summary>在相对路径下根据指定文件名读取 </summary>
        private static string Read(string fileName) {
            string path = System.Environment.CurrentDirectory + @"\" + fileName;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (StreamReader sw = new StreamReader(fs, Encoding.UTF8)) {
                    return sw.ReadToEnd();
                }
            }
        }

        /// <summary>在相对路径下写入文件</summary>
        private static void Write(string fileName, string content) {
            string path = System.Environment.CurrentDirectory + @"\" + fileName;
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8)) {//append形式
                sw.Write(content);
            }
        }

        /// <summary>参数错误Log</summary>
        private static void LogArgumentError(int code) {
            Console.WriteLine("参数格式不正确"+" Code:"+code);
        }


        /// <summary>统计字符数并打印</summary>
        private static int CountChar(string s) {
            return s.Length;
        }

        /// <summary>统计单词数并打印</summary>
        private static int CountWord(string s) {
            int count = 0;
            char lastChar = ' ';
            foreach (char c in s) {
                if ((c != ',' || c != ' ' || c != '\0' || c != '\r' || c != '\n') && (lastChar == ','|| lastChar == ' '|| lastChar == '\n')) {
                    count++;
                }
                lastChar = c;
            }
            return count;
        }

        /// <summary>统计行数并打印</summary>
        private static int CountLine(string s) {
            int count = 0;
            foreach (char c in s) {
                if (count == 0) {
                    count++;
                    continue;
                }
                if (c == '\n') {
                    count++;
                }
            }
            return count;
        }
    }

   
}
