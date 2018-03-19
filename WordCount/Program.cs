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
        NextStep = 3,
        StopListOption = 4,
        StopListFile = 5,
        OutputOption = 6,
        OutputFile = 7,
        Finish = 8
    }

    public class Program {

        private static HashSet<char> displayedCharDic = new HashSet<char> { ' ', '\n', '\0', '\r' ,'/','*'};
        private static HashSet<char> splitCharDic = new HashSet<char> { ' ', '\n', '\0', '\r', ','};

        public static void Main(string[] args) {
            int code = Entrance(args);
            //int code = Entrance(new string[] { "-x" });
            //int code = Entrance(new string[] { "-a", "-w", "-l", "-c", "a.txt", "-e", "stopList.txt" });
            if (code != 0) {
                Console.WriteLine("参数格式不正确" + " Code:" + code);
            }
            //Console.ReadKey();
        }
        
        /// <summary>入口测试函数</summary>
        public static int Entrance(string[] args) {
            if (args.Length == 0||args==null)
                return -1;
            List<string> options = new List<string>();//-c -w -l选项
            string sourceFile = null; //源文件名或通配符名
            string outputFile = "result.txt"; //输出文件名
            Dictionary<string,string> sources = new Dictionary<string, string>(); //源文件名字-源文件内容
            bool isOutput = true;//是否输出到文件
            bool isSelectFile = false;//-x选项选择文件
            bool useStopList = false;//是否使用停用词表
            string stopListFile = null;//停用词表文件名
            ArgsStatus status = ArgsStatus.Uninitialized;
            //遍历参数
            foreach(var s in args) {
                if (status== ArgsStatus.Uninitialized||status==ArgsStatus.NomalOption) {
                    if(status== ArgsStatus.Uninitialized && s == "-x") {
                        status = ArgsStatus.Finish;
                        options.Add("-c");
                        options.Add("-w");
                        options.Add("-l");
                        options.Add("-a");
                        isOutput = false;
                        isSelectFile = true;
                        status = ArgsStatus.Finish;
                    }
                    else if (s == "-c" || s == "-w" || s == "-l"||s=="-a"||s=="-s") {
                        if (options.Contains(s)) {
                            return 1;
                        }
                        options.Add(s);
                        status = ArgsStatus.NomalOption;
                    }
                    else if (status == ArgsStatus.NomalOption) {
                        sourceFile = s;
                        status = ArgsStatus.NextStep;
                    }
                    else {
                        return 2;
                    }
                }
                
                else if(status == ArgsStatus.NextStep) {
                    if (s == "-e") {
                        status = ArgsStatus.StopListFile;
                    }
                    else if (s == "-o") {
                        status = ArgsStatus.OutputFile;
                    }
                    else {
                        return 7;
                    }
                }
                else if(status== ArgsStatus.StopListFile) {
                    stopListFile = s;
                    useStopList = true;
                    status = ArgsStatus.OutputOption;
                }

                else if (status == ArgsStatus.OutputOption) {
                    if (s == "-o") {
                        status = ArgsStatus.OutputFile;
                    }
                    else {
                        return 3;
                    }
                }
                else if (status == ArgsStatus.OutputFile) {
                    outputFile = s;
                    isOutput = true;
                    status = ArgsStatus.Finish;
                }

                else if(status == ArgsStatus.Finish) {
                    return 4;
                }
            }

            //最终必须停留在这三个状态
            if (status != ArgsStatus.OutputOption && status != ArgsStatus.Finish && status != ArgsStatus.NextStep) {
                return 5;
            }


            try {
                if (!isSelectFile) {
                    if (options.Contains("-s")) {
                        List<string> files = new List<string>(Directory.GetFiles(Environment.CurrentDirectory, sourceFile));
                        foreach (string file in files) {
                            string fileName = file.Split('\\').Last();
                            sources.Add(fileName, Read(fileName));
                        }
                    }
                    else sources.Add(sourceFile, Read(sourceFile));
                }
                else {
                    string stringFromSelectFile = Read(out sourceFile);
                    sources.Add(sourceFile, stringFromSelectFile);
                }
            }
            catch (IOException) {
                Console.WriteLine(sourceFile+"不存在");
                return 6;
            }
            catch (Exception e) {
                return 0;
            }

            HashSet<string> stopListHashTable = new HashSet<string>();//停用词表
            if (useStopList) {
                string stopListString = Read(stopListFile);
                stopListHashTable = new HashSet<string>(stopListString.Split(' '));
            }


            foreach (KeyValuePair<string,string> kvp in sources) {

                int c_result = -1;//字符数
                int w_result = -1;//单词数
                int l_result = -1;//行数

                int cl_result = -1;//代码行
                int el_result = -1;//空行
                int cml_result = -1;//注释行

                foreach (var option in options) {
                    if (option == "-c")
                        c_result = CountChar(kvp.Value);
                    else if (option == "-w") {
                        if (useStopList) {
                            w_result = CountWord(kvp.Value, stopListHashTable);
                        }
                        else w_result = CountWord(kvp.Value);
                    }
                    else if (option == "-l")
                        l_result = CountLine(kvp.Value);
                    else if (option == "-a")
                        CountMoreAboutLine(kvp.Value, out cl_result, out el_result, out cml_result);
                }

                if (c_result != -1)
                    Console.WriteLine(kvp.Key + ", " + "字符数: " + c_result);
                if (w_result != -1)
                    Console.WriteLine(kvp.Key + ", " + "单词数: " + w_result);
                if (l_result != -1)
                    Console.WriteLine(kvp.Key + ", " + "行数: " + l_result);
                if (cl_result != -1)
                    Console.WriteLine(kvp.Key + ", " + "代码行/空行/注释行: " + cl_result + "/" + el_result + "/" + cml_result);

                if (isOutput) {
                    string content = "";
                    if (c_result != -1)
                        content += (kvp.Key + ", " + "字符数: " + c_result + "\r\n");
                    if (w_result != -1)
                        content += (kvp.Key + ", " + "单词数: " + w_result + "\r\n");
                    if (l_result != -1)
                        content += (kvp.Key + ", " + "行数: " + l_result + "\r\n");
                    if (cl_result != -1)
                        content += (kvp.Key + ", " + "代码行/空行/注释行: " + cl_result + "/" + el_result + "/" + cml_result + "\r\n");
                    Write(outputFile, content);
                    //Console.WriteLine("输出到文件" + outputFile + "成功");
                }

            }

            return 0;

        }

        /// <summary>通过图形界面读取文件 </summary>
        public static string Read(out string fileName) {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
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
            throw new System.Exception("用户没有选择文件");
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


        /// <summary>统计字符数并打印</summary>
        private static int CountChar(string s) {
            return s.Length;
        }

        /// <summary>统计单词数并打印</summary>
        private static int CountWord(string s) {
            return CountWord(s, new HashSet<string>());
        }
        /// <summary>根据停用词表统计单词数并打印</summary>
        private static int CountWord(string s,HashSet<string> stopList) {
            if (s.Length == 0)
                return 0;
            int count = 1;
            int i = -1;
            StringBuilder sb = new StringBuilder();
            foreach (char c in s) {
                i++;
                if (splitCharDic.Contains(c)) {
                    if (i < s.Length - 1 && !splitCharDic.Contains(s[i + 1])) {
                        if (!stopList.Contains(sb.ToString())) {
                            count++;
                            //Console.WriteLine(count + ":" + sb.ToString());
                            sb.Clear();
                        }
                        else {
                            sb.Clear();
                        }
                    }
                }
                else {
                    sb.Append(c);
                }
            }
            return count;
        }

        /// <summary>统计行数并打印</summary>
        private static int CountLine(string s) {
            int count = 0;
            int i = -1;
            foreach (char c in s) {
                i++;
                if (count == 0) {
                    count++;
                    continue;
                }
                if (c == '\n'&&i<s.Length-1) {
                    count++;
                }
            }
            return count;
        }

        private enum CommentLineType {
            NonCommentLine = 0,
            SingleCommentLine = 1,
            SeveralCommentLine = 2,
        }

        /// <summary>统计行的详细信息</summary>
        private static void CountMoreAboutLine(string s, out int codeLineCount, out int emptyLineCount, out int commentLineCount) {
            codeLineCount = 0;//代码行
            emptyLineCount = 0;//空行
            commentLineCount = 0;//注释行
            int charInALine = 0; //当前行可见字符数
            int i = -1;
            CommentLineType CommentLine = CommentLineType.NonCommentLine;
            bool cancelCommentLine = false;
            foreach (char c in s) {
                i++;
                if (c == '\n'||i==s.Length-1) {
                    if (CommentLine==CommentLineType.SingleCommentLine) {
                        commentLineCount++;
                        CommentLine = CommentLineType.NonCommentLine;
                    }
                    else if ((CommentLine == CommentLineType.SeveralCommentLine|| cancelCommentLine )&& charInALine<=1) {
                        commentLineCount++;
                    }
                    else if (charInALine > 1) {
                        codeLineCount++;
                    }
                    else emptyLineCount++;
                    if (cancelCommentLine) {
                        cancelCommentLine = false;
                    }
                    charInALine = 0;
                }
                if (charInALine <= 1 && c == '/' && i > 0 && s[i - 1] == '/') {
                    CommentLine = CommentLineType.SingleCommentLine;
                }
                else if (c == '*' && i > 0 && s[i - 1] == '/') {
                    CommentLine = CommentLineType.SeveralCommentLine;
                }
                else if (c == '/' && i > 0 && s[i - 1] == '*') {
                    cancelCommentLine = true;
                    CommentLine = CommentLineType.NonCommentLine;
                }
                else if (CommentLine == CommentLineType.NonCommentLine && !displayedCharDic.Contains(c)) {
                    charInALine++;
                }
            }
        }


    }
   
}
