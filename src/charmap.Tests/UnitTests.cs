using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using charmap;

namespace charmap.Tests
{
    [TestClass]
    public class UnicodeLogicTests
    {
        [TestMethod]
        public void CharacterItem_HandlesBmpCharacters()
        {
            var item = new CharacterItem { Character = "A", CodePoint = 65 };
            Assert.AreEqual("A", item.Character);
            Assert.AreEqual(65, item.CodePoint);
        }

        [TestMethod]
        public void CharacterItem_HandlesSurrogatePairs()
        {
            var item = new CharacterItem { Character = "\U0001F600", CodePoint = 0x1F600 };
            Assert.AreEqual("\U0001F600", item.Character);
            Assert.AreEqual(0x1F600, item.CodePoint);
        }

        [TestMethod]
        public void CharacterItem_HandlesPolishCharacters()
        {
            var chars = new[] { "\u0105", "\u0107", "\u0119", "\u0142", "\u0144", "\u00F3", "\u015B", "\u017A", "\u017C" };
            foreach (var c in chars)
            {
                var item = new CharacterItem { Character = c, CodePoint = char.ConvertToUtf32(c, 0) };
                Assert.AreEqual(c, item.Character);
            }
        }

        [TestMethod]
        public void CharacterItem_DefaultValues()
        {
            var item = new CharacterItem();
            Assert.AreEqual(string.Empty, item.Character);
            Assert.AreEqual(0, item.CodePoint);
            Assert.AreEqual(string.Empty, item.FontFamily);
        }

        [TestMethod]
        public void CharacterItem_FontProperty()
        {
            var item = new CharacterItem { Character = "X", CodePoint = 88, FontFamily = "Segoe UI" };
            Assert.AreEqual("Segoe UI", item.FontFamily);
        }

        [TestMethod]
        public void CharacterItem_Equals_SameCharacterAndCodePoint()
        {
            var a = new CharacterItem { Character = "A", CodePoint = 65 };
            var b = new CharacterItem { Character = "A", CodePoint = 65 };
            Assert.AreEqual(a.Character, b.Character);
            Assert.AreEqual(a.CodePoint, b.CodePoint);
        }

        [TestMethod]
        public void UnicodeRanges_AreValid()
        {
            var ranges = new (int start, int end)[]
            {
                (0x21, 0x7E), (0xA1, 0xFF), (0x100, 0x17F), (0x180, 0x24F),
                (0x250, 0x2AF), (0x2B0, 0x2FF), (0x300, 0x36F), (0x370, 0x3FF),
                (0x400, 0x4FF), (0x1E00, 0x1EFF), (0x2000, 0x206F), (0x2070, 0x209F),
                (0x20A0, 0x20CF), (0x2100, 0x214F), (0x2150, 0x218F), (0x2190, 0x21FF),
                (0x2200, 0x22FF), (0x2300, 0x23FF), (0x2500, 0x257F), (0x2580, 0x259F),
                (0x25A0, 0x25FF), (0x2600, 0x26FF), (0x2700, 0x27BF), (0x3000, 0x303F),
            };

            foreach (var (start, end) in ranges)
            {
                Assert.IsTrue(start <= end, $"Range start {start:X} should be <= end {end:X}");
                Assert.IsTrue(start >= 0 && start <= 0x10FFFF, $"Start {start:X} out of Unicode range");
                Assert.IsTrue(end >= 0 && end <= 0x10FFFF, $"End {end:X} out of Unicode range");
            }
        }

        [TestMethod]
        public void UnicodeRanges_HaveNoOverlaps()
        {
            var ranges = new (int start, int end)[]
            {
                (0x21, 0x7E), (0xA1, 0xFF), (0x100, 0x17F), (0x180, 0x24F),
                (0x250, 0x2AF), (0x2B0, 0x2FF), (0x300, 0x36F), (0x370, 0x3FF),
                (0x400, 0x4FF), (0x1E00, 0x1EFF), (0x2000, 0x206F), (0x2070, 0x209F),
                (0x20A0, 0x20CF), (0x2100, 0x214F), (0x2150, 0x218F), (0x2190, 0x21FF),
                (0x2200, 0x22FF), (0x2300, 0x23FF), (0x2500, 0x257F), (0x2580, 0x259F),
                (0x25A0, 0x25FF), (0x2600, 0x26FF), (0x2700, 0x27BF), (0x3000, 0x303F),
            };

            var sorted = ranges.OrderBy(r => r.start).ToList();
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                Assert.IsTrue(sorted[i].end < sorted[i + 1].start,
                    $"Overlap between {sorted[i].start:X}-{sorted[i].end:X} and {sorted[i + 1].start:X}-{sorted[i + 1].end:X}");
            }
        }

        [TestMethod]
        public void UnicodeRanges_ExpectedBlocksPresent()
        {
            var starts = new[] { 0x21, 0xA1, 0x100, 0x400, 0x20A0, 0x2200, 0x2600 };
            var ranges = new (int start, int end)[]
            {
                (0x21, 0x7E), (0xA1, 0xFF), (0x100, 0x17F), (0x180, 0x24F),
                (0x250, 0x2AF), (0x2B0, 0x2FF), (0x300, 0x36F), (0x370, 0x3FF),
                (0x400, 0x4FF), (0x1E00, 0x1EFF), (0x2000, 0x206F), (0x2070, 0x209F),
                (0x20A0, 0x20CF), (0x2100, 0x214F), (0x2150, 0x218F), (0x2190, 0x21FF),
                (0x2200, 0x22FF), (0x2300, 0x23FF), (0x2500, 0x257F), (0x2580, 0x259F),
                (0x25A0, 0x25FF), (0x2600, 0x26FF), (0x2700, 0x27BF), (0x3000, 0x303F),
            };

            foreach (var s in starts)
            {
                Assert.IsTrue(ranges.Any(r => r.start == s), $"Expected block starting at {s:X4} not found");
            }
        }
    }

    [TestClass]
    public class UnicodeHelperTests
    {
        [TestMethod]
        public void GetUnicodeBlockName_BasicLatin_ReturnsBasicLatin()
        {
            Assert.AreEqual("Basic Latin", UnicodeHelper.GetUnicodeBlockName(0x41));
        }

        [TestMethod]
        public void GetUnicodeBlockName_Greek_ReturnsGreekAndCoptic()
        {
            Assert.AreEqual("Greek and Coptic", UnicodeHelper.GetUnicodeBlockName(0x3A0));
        }

        [TestMethod]
        public void GetUnicodeBlockName_Cyrillic_ReturnsCyrillic()
        {
            Assert.AreEqual("Cyrillic", UnicodeHelper.GetUnicodeBlockName(0x410));
        }

        [TestMethod]
        public void GetUnicodeBlockName_Armenian_ReturnsArmenian()
        {
            Assert.AreEqual("Armenian", UnicodeHelper.GetUnicodeBlockName(0x531));
        }

        [TestMethod]
        public void GetUnicodeBlockName_Hebrew_ReturnsHebrew()
        {
            Assert.AreEqual("Hebrew", UnicodeHelper.GetUnicodeBlockName(0x5D0));
        }

        [TestMethod]
        public void GetUnicodeBlockName_Arabic_ReturnsArabic()
        {
            Assert.AreEqual("Arabic", UnicodeHelper.GetUnicodeBlockName(0x625));
        }

        [TestMethod]
        public void GetUnicodeBlockName_HangulSyllables_ReturnsHangulSyllables()
        {
            Assert.AreEqual("Hangul Syllables", UnicodeHelper.GetUnicodeBlockName(0xAC00));
        }

        [TestMethod]
        public void GetUnicodeBlockName_Unknown_ReturnsUnknown()
        {
            Assert.AreEqual("Unknown Block", UnicodeHelper.GetUnicodeBlockName(0x110000));
        }

        [TestMethod]
        public void GetUnicodeBlockName_CJKUnifiedIdeographs_ReturnsCJKUnifiedIdeographs()
        {
            Assert.AreEqual("CJK Unified Ideographs", UnicodeHelper.GetUnicodeBlockName(0x4E00));
        }
    }

    [TestClass]
    public class AppLoggerTests
    {
        [TestMethod]
        public void AppLogger_GetLogPath_ReturnsValidPath()
        {
            var path = AppLogger.GetLogPath();
            Assert.IsFalse(string.IsNullOrEmpty(path));
            Assert.IsTrue(path.Contains("charmap"), "Log path should contain 'charmap'");
            Assert.IsTrue(path.EndsWith(".log"), "Log path should end with .log");
            Assert.IsTrue(Path.IsPathRooted(path), "Log path should be absolute");
        }

        [TestMethod]
        public void AppLogger_WritesInfoLog()
        {
            var testMessage = $"INFO_TEST_{Guid.NewGuid()}";
            AppLogger.Info(testMessage);
            var path = AppLogger.GetLogPath();
            Assert.IsTrue(File.Exists(path), "Log file should exist after writing");
            var content = File.ReadAllText(path);
            Assert.IsTrue(content.Contains(testMessage), "Log should contain the test message");
            Assert.IsTrue(content.Contains("[INFO]"), "Log should contain INFO level");
        }

        [TestMethod]
        public void AppLogger_WritesErrorLog_WithException()
        {
            var testMessage = $"ERROR_TEST_{Guid.NewGuid()}";
            var ex = new InvalidOperationException("Test exception");
            AppLogger.Error(testMessage, ex);
            var content = File.ReadAllText(AppLogger.GetLogPath());
            Assert.IsTrue(content.Contains(testMessage));
            Assert.IsTrue(content.Contains("[ERROR]"));
            Assert.IsTrue(content.Contains("Exception:"));
            Assert.IsTrue(content.Contains("InvalidOperationException"));
        }

        [TestMethod]
        public void AppLogger_WritesFatalLog()
        {
            var testMessage = $"FATAL_TEST_{Guid.NewGuid()}";
            AppLogger.Fatal(testMessage);
            var content = File.ReadAllText(AppLogger.GetLogPath());
            Assert.IsTrue(content.Contains(testMessage));
            Assert.IsTrue(content.Contains("[FATAL]"));
        }

        [TestMethod]
        public void AppLogger_LogFormat_ContainsTimestamp()
        {
            var testMessage = $"TIME_TEST_{Guid.NewGuid()}";
            AppLogger.Info(testMessage);
            var lines = File.ReadAllLines(AppLogger.GetLogPath())
                .Where(l => l.Contains(testMessage))
                .ToList();
            Assert.AreEqual(1, lines.Count, "Should find exactly one line with test message");
            var line = lines[0];
            Assert.IsTrue(line.StartsWith("["), "Log line should start with timestamp bracket");
            Assert.IsTrue(line.Contains("] [INFO] "), "Log line should contain level marker");
        }

        [TestMethod]
        public void AppLogger_ThreadSafety_MultipleConcurrentWrites()
        {
            var testId = Guid.NewGuid().ToString();
            int threadCount = 10;
            int logsPerThread = 100;
            var tasks = new Task[threadCount];

            for (int t = 0; t < threadCount; t++)
            {
                int threadNum = t;
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < logsPerThread; i++)
                    {
                        AppLogger.Info($"THREAD_{testId}_{threadNum}_{i}");
                    }
                });
            }

            Task.WaitAll(tasks);

            var content = File.ReadAllText(AppLogger.GetLogPath());
            for (int t = 0; t < threadCount; t++)
            {
                for (int i = 0; i < logsPerThread; i++)
                {
                    Assert.IsTrue(content.Contains($"THREAD_{testId}_{t}_{i}"),
                        $"Missing log from thread {t} iteration {i}");
                }
            }
        }
    }
}
