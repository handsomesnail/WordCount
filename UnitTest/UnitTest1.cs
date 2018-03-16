using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordCount;

namespace UnitTest {
    [TestClass]
    public class UnitTest1 {

        [TestMethod]
        public void TestMethod1() {
            string[] input = new string[] { };
            Assert.AreEqual(Program.Entrance(input), -1);
        }

        [TestMethod]
        public void TestMethod2() {
            string[] input = new string[] {"-q", "input.txt" };
            Assert.AreEqual(Program.Entrance(input), 2);
        }

        [TestMethod]
        public void TestMethod3() {
            string[] input = new string[] {"-c", "-c" ,"input.txt"};
            Assert.AreEqual(Program.Entrance(input), 1);
        }

        [TestMethod]
        public void TestMethod4() {
            string[] input = new string[] { "-c", "input.txt", "-a" };
            Assert.AreEqual(Program.Entrance(input), 3);
        }

        [TestMethod]
        public void TestMethod5() {
            string[] input = new string[] { "-c", "input.txt", "-o", "output.txt", "-c" };
            Assert.AreEqual(Program.Entrance(input), 4);
        }

        [TestMethod]
        public void TestMethod6() {
            string[] input = new string[] { "-c", "input.txt","-o" };
            Assert.AreEqual(Program.Entrance(input), 5);
        }

        [TestMethod]
        public void TestMethod7() {
            string[] input = new string[] { "-c", "-w", "-l" ,"example.txt" };
            Assert.AreEqual(Program.Entrance(input), 6);
        }

        [TestMethod]
        public void TestMethod8() {
            string[] input = new string[] { "-c", "-w", "-l", "input.txt" };
            Assert.AreEqual(Program.Entrance(input), 0);
        }

        [TestMethod]
        public void TestMethod9() {
            string[] input = new string[] { "-l", "-w", "-c", "input.txt", "-o" ,"output.txt"};
            Assert.AreEqual(Program.Entrance(input), 0);
        }

        [TestMethod]
        public void TestMethod10() {
            string[] input = new string[] {  "-x" };
            Assert.AreEqual(Program.Entrance(input), 0);
        }


    }
}
