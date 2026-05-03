using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace charmap.Core.Tests
{
    [TestClass]
    public class LocalizationTests
    {
        private static string _solutionDir = null!;
        private static readonly char[] PolishChars = new[] { 'ą', 'ć', 'ę', 'ł', 'ń', 'ó', 'ś', 'ź', 'ż', 'Ą', 'Ć', 'Ę', 'Ł', 'Ń', 'Ó', 'Ś', 'Ź', 'Ż' };

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            _solutionDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
        }

        [TestMethod]
        public void AllPlKeysExistInEn()
        {
            var plKeys = LoadReswKeys(Path.Combine(_solutionDir, "charmap", "Strings", "pl", "Resources.resw"));
            var enKeys = LoadReswKeys(Path.Combine(_solutionDir, "charmap", "Strings", "en", "Resources.resw"));

            var missing = plKeys.Except(enKeys).ToList();
            Assert.AreEqual(0, missing.Count, "Missing EN keys: " + string.Join(", ", missing));
        }

        [TestMethod]
        public void AllEnKeysExistInPl()
        {
            var plKeys = LoadReswKeys(Path.Combine(_solutionDir, "charmap", "Strings", "pl", "Resources.resw"));
            var enKeys = LoadReswKeys(Path.Combine(_solutionDir, "charmap", "Strings", "en", "Resources.resw"));

            var missing = enKeys.Except(plKeys).ToList();
            Assert.AreEqual(0, missing.Count, "Missing PL keys: " + string.Join(", ", missing));
        }

        [TestMethod]
        public void NoEmptyTranslations()
        {
            foreach (var lang in new[] { "pl", "en" })
            {
                var path = Path.Combine(_solutionDir, "charmap", "Strings", lang, "Resources.resw");
                var doc = XDocument.Load(path);
                var empties = doc.Root!.Elements("data")
                    .Where(e => string.IsNullOrWhiteSpace(e.Element("value")?.Value))
                    .Select(e => e.Attribute("name")?.Value)
                    .Where(v => v != null)
                    .ToList();

                Assert.AreEqual(0, empties.Count, $"Empty {lang} translations: " + string.Join(", ", empties!));
            }
        }

        [TestMethod]
        public void PolishResw_ContainsPolishCharacters()
        {
            var plPath = Path.Combine(_solutionDir, "charmap", "Strings", "pl", "Resources.resw");
            var text = File.ReadAllText(plPath);
            bool hasAny = PolishChars.Any(c => text.Contains(c));
            Assert.IsTrue(hasAny, "Polish RESW should contain Polish characters");
        }

        [DataTestMethod]
        [DataRow("pl")]
        [DataRow("en")]
        public void ReswValues_AreWithinReasonableLength(string lang)
        {
            var path = Path.Combine(_solutionDir, "charmap", "Strings", lang, "Resources.resw");
            var doc = XDocument.Load(path);
            var offenders = doc.Root!.Elements("data")
                .Where(e => (e.Element("value")?.Value?.Length ?? 0) > 500)
                .Select(e => e.Attribute("name")?.Value)
                .Where(v => v != null)
                .ToList();

            Assert.AreEqual(0, offenders.Count, $"Values too long in {lang}: " + string.Join(", ", offenders!));
        }

        [TestMethod]
        public void PolishResw_LongValuesContainPolishCharacters()
        {
            var plPath = Path.Combine(_solutionDir, "charmap", "Strings", "pl", "Resources.resw");
            var doc = XDocument.Load(plPath);
            var values = doc.Root!.Elements("data")
                .Select(e => new { Key = e.Attribute("name")?.Value, Value = e.Element("value")?.Value })
                .Where(x => x.Key != null && x.Value != null && x.Value.Length > 10)
                .ToList();

            var offenders = new List<string>();
            foreach (var item in values)
            {
                if (!PolishChars.Any(c => item.Value!.Contains(c)))
                {
                    offenders.Add(item.Key!);
                }
            }

            Assert.IsTrue(offenders.Count < values.Count,
                $"At least one long PL value should contain Polish characters. All {values.Count} failed: " + string.Join(", ", offenders));
        }

        [TestMethod]
        public void EnglishResw_ContainsNoPolishCharacters()
        {
            var enPath = Path.Combine(_solutionDir, "charmap", "Strings", "en", "Resources.resw");
            var text = File.ReadAllText(enPath);
            var offenders = PolishChars.Where(c => text.Contains(c)).ToList();
            Assert.AreEqual(0, offenders.Count, "English RESW contains Polish characters: " + string.Join(", ", offenders));
        }

        [TestMethod]
        public void AllReswKeys_AreCamelCase()
        {
            foreach (var lang in new[] { "pl", "en" })
            {
                var path = Path.Combine(_solutionDir, "charmap", "Strings", lang, "Resources.resw");
                var doc = XDocument.Load(path);
                var keys = doc.Root!.Elements("data")
                    .Select(e => e.Attribute("name")?.Value)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                var invalid = keys.Where(k => !Regex.IsMatch(k!, "^[A-Z][a-zA-Z0-9]*$")).ToList();
                Assert.AreEqual(0, invalid.Count, $"Invalid keys in {lang}: " + string.Join(", ", invalid));
            }
        }

        [TestMethod]
        public void AllFallbackKeys_ExistInResw()
        {
            var csPath = Path.Combine(_solutionDir, "charmap", "MainWindow.xaml.cs");
            var csText = File.ReadAllText(csPath);
            var matches = Regex.Matches(csText, @"GetString\(""([^""]*)""");
            var fallbackKeys = matches.Select(m => m.Groups[1].Value).Distinct().ToList();

            var plKeys = LoadReswKeys(Path.Combine(_solutionDir, "charmap", "Strings", "pl", "Resources.resw"));
            var enKeys = LoadReswKeys(Path.Combine(_solutionDir, "charmap", "Strings", "en", "Resources.resw"));

            var allKeys = new HashSet<string>(plKeys);
            allKeys.UnionWith(enKeys);

            var missing = fallbackKeys.Where(k => !allKeys.Contains(k)).ToList();
            Assert.AreEqual(0, missing.Count, "Fallback keys missing in RESW: " + string.Join(", ", missing));
        }

        [TestMethod]
        public void CsCode_UsesResourceLoader_NotHardcodedPolishStrings()
        {
            var csFiles = Directory.GetFiles(Path.Combine(_solutionDir, "charmap"), "*.cs", SearchOption.AllDirectories);
            var forbidden = new[] { "Czcionka:", "Znaki do skopiowania:", "Widok zaawansowany", "Pomoc", "Wybierz", "Kopiuj" };
            var violations = new List<string>();

            foreach (var file in csFiles)
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.Contains("GetString(")) continue;
                    foreach (var f in forbidden)
                    {
                        if (line.Contains($"\"{f}\""))
                        {
                            violations.Add($"{Path.GetFileName(file)}:{i + 1} => {f}");
                        }
                    }
                }
            }

            Assert.AreEqual(0, violations.Count, "Hardcoded Polish strings found: " + string.Join("; ", violations));
        }

        private static HashSet<string> LoadReswKeys(string path)
        {
            var doc = XDocument.Load(path);
            return new HashSet<string>(
                doc.Root!.Elements("data")
                   .Select(e => e.Attribute("name")?.Value)
                   .Where(v => !string.IsNullOrEmpty(v))!);
        }
    }

    [TestClass]
    public class XamlParsingTests
    {
        private static string _solutionDir = null!;
        private static string _xamlContent = null!;
        private static string _csContent = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            _solutionDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
            _xamlContent = File.ReadAllText(Path.Combine(_solutionDir, "charmap", "MainWindow.xaml"));
            _csContent = File.ReadAllText(Path.Combine(_solutionDir, "charmap", "MainWindow.xaml.cs"));
        }

        [TestMethod]
        public void MainWindowXaml_ContainsAllNamedControls()
        {
            var required = new[]
            {
                "x:Name=\"AppTitleBar\"",
                "x:Name=\"TitleTextBlock\"",
                "x:Name=\"FontLabel\"",
                "x:Name=\"FontComboBox\"",
                "x:Name=\"HelpButton\"",
                "x:Name=\"CharacterGridView\"",
                "x:Name=\"CopyLabel\"",
                "x:Name=\"CopyTextBox\"",
                "x:Name=\"SelectButton\"",
                "x:Name=\"CopyButton\"",
                "x:Name=\"AdvancedViewCheckBox\"",
                "x:Name=\"AdvancedPanel\"",
                "x:Name=\"CharacterSetLabel\"",
                "x:Name=\"CharacterSetComboBox\"",
                "x:Name=\"GoToUnicodeLabel\"",
                "x:Name=\"GoToUnicodeTextBox\"",
                "x:Name=\"GoToUnicodeButton\"",
                "x:Name=\"GroupByLabel\"",
                "x:Name=\"GroupByComboBox\"",
                "x:Name=\"SearchLabel\"",
                "x:Name=\"SearchTextBox\"",
                "x:Name=\"SearchButton\"",
                "x:Name=\"SelectedCharacterTextBlock\""
            };

            foreach (var r in required)
            {
                Assert.IsTrue(_xamlContent.Contains(r), $"Missing control: {r}");
            }
        }

        [TestMethod]
        public void MainWindowXaml_AllButtonsHaveHandlers()
        {
            var buttons = new[] { "HelpButton", "SelectButton", "CopyButton" };
            foreach (var btn in buttons)
            {
                Assert.IsTrue(_xamlContent.Contains($"x:Name=\"{btn}\"") && _xamlContent.Contains($"{btn}\""),
                    $"Button {btn} should exist and likely have a handler in code-behind");
            }
        }

        [TestMethod]
        public void MainWindowXaml_CopyTextBox_IsReadOnly()
        {
            Assert.IsTrue(_xamlContent.Contains("x:Name=\"CopyTextBox\"") && _xamlContent.Contains("IsReadOnly=\"True\""),
                "CopyTextBox should be read-only");
        }

        [TestMethod]
        public void MainWindowXaml_AdvancedView_IsEnabled()
        {
            Assert.IsTrue(_xamlContent.Contains("x:Name=\"AdvancedViewCheckBox\"") && !_xamlContent.Contains("IsEnabled=\"False\""),
                "AdvancedViewCheckBox should be enabled");
        }

        [TestMethod]
        public void CodeBehind_UsesMicaAlt()
        {
            Assert.IsTrue(_csContent.Contains("MicaBackdrop") && _csContent.Contains("MicaKind.BaseAlt"), "C# should set Mica Alt backdrop");
        }

        [TestMethod]
        public void CodeBehind_ExtendsContentIntoTitleBar()
        {
            Assert.IsTrue(_csContent.Contains("ExtendsContentIntoTitleBar = true"), "C# should extend content into title bar");
        }

        [TestMethod]
        public void Xaml_IsWellFormedXml()
        {
            try
            {
                XDocument.Parse(_xamlContent);
            }
            catch (Exception ex)
            {
                Assert.Fail("MainWindow.xaml is not well-formed XML: " + ex.Message);
            }
        }

        [TestMethod]
        public void Xaml_NoDuplicateXNames()
        {
            var matches = Regex.Matches(_xamlContent, @"x:Name=""([^""]+)""");
            var names = matches.Select(m => m.Groups[1].Value).ToList();
            var duplicates = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.AreEqual(0, duplicates.Count, "Duplicate x:Name values found: " + string.Join(", ", duplicates));
        }

        [TestMethod]
        public void Xaml_AllClickHandlers_HaveMethodsInCodeBehind()
        {
            var clickMatches = Regex.Matches(_xamlContent, @"Click=""([^""]+)""");
            var handlers = clickMatches.Select(m => m.Groups[1].Value).Distinct().ToList();

            foreach (var handler in handlers)
            {
                Assert.IsTrue(_csContent.Contains($"void {handler}("),
                    $"Click handler '{handler}' not found in MainWindow.xaml.cs");
            }
        }

        [TestMethod]
        public void Xaml_NoHardcodedTextContent()
        {
            var doc = XDocument.Parse(_xamlContent);
            var hardcoded = new List<string>();
            foreach (var element in doc.Descendants())
            {
                if (element.Name.LocalName == "DataTemplate") continue;
                if (element.Name.LocalName == "ItemsPanelTemplate") continue;
                if (element.Name.LocalName == "Window") continue;

                var content = element.Value?.Trim();
                if (!string.IsNullOrEmpty(content) && content.Length > 1)
                {
                    if (element.Attribute("{http://schemas.microsoft.com/winfx/2006/xaml}Bind") != null) continue;
                    if (element.Attributes().Any(a => a.Name.LocalName == "Tag")) continue;
                    if (element.Name.LocalName == "ToolTipService.ToolTip") continue;

                    if (!content.StartsWith("{") && !content.Contains("{x:Bind"))
                    {
                        hardcoded.Add($"<{element.Name.LocalName}>{content}</{element.Name.LocalName}>");
                    }
                }
            }

            Assert.AreEqual(0, hardcoded.Count, "Hardcoded text content in XAML: " + string.Join("; ", hardcoded));
        }
    }

    [TestClass]
    public class ResponsivenessTests
    {
        private static string _solutionDir = null!;
        private static string _xamlContent = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            _solutionDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
            _xamlContent = File.ReadAllText(Path.Combine(_solutionDir, "charmap", "MainWindow.xaml"));
        }

        [TestMethod]
        public void MainWindowXaml_HasScrollViewerAroundGrid()
        {
            Assert.IsTrue(_xamlContent.Contains("<ScrollViewer"), "Should have ScrollViewer for responsiveness");
        }

        [TestMethod]
        public void MainWindowXaml_UsesStarSizing()
        {
            Assert.IsTrue(_xamlContent.Contains("Height=\"*\""), "Should use star sizing for responsive layout");
        }

        [TestMethod]
        public void MainWindowXaml_AllNamedControlsArePublic()
        {
            var namedControls = new[] { "AppTitleBar", "TitleTextBlock", "FontLabel", "FontComboBox",
                                        "HelpButton", "CharacterGridView", "CopyLabel", "CopyTextBox",
                                        "SelectButton", "CopyButton", "AdvancedViewCheckBox", "AdvancedPanel",
                                        "CharacterSetLabel", "CharacterSetComboBox", "GoToUnicodeLabel", "GoToUnicodeTextBox",
                                        "GoToUnicodeButton", "GroupByLabel", "GroupByComboBox", "SearchLabel", "SearchTextBox",
                                        "SearchButton", "SelectedCharacterTextBlock" };
            foreach (var ctrl in namedControls)
            {
                var nameAttr = $"x:Name=\"{ctrl}\"";
                Assert.IsTrue(_xamlContent.Contains(nameAttr), $"Missing control: {ctrl}");
                var index = _xamlContent.IndexOf(nameAttr);
                var snippet = _xamlContent.Substring(index, Math.Min(120, _xamlContent.Length - index));
                Assert.IsTrue(snippet.Contains("x:FieldModifier=\"public\""),
                    $"Control {ctrl} should be public for UI test accessibility");
            }
        }

        [TestMethod]
        public void MainWindowXaml_UsesGridNotFixedDimensions()
        {
            Assert.IsTrue(_xamlContent.Contains("<Grid"), "Should use Grid for layout");
            var fixedWidthMatches = Regex.Matches(_xamlContent, @"Width=""\d+""");
            var fixedHeightMatches = Regex.Matches(_xamlContent, @"Height=""\d+""");
            Assert.IsTrue(fixedWidthMatches.Count < 5, "Too many fixed widths for responsive design");
            Assert.IsTrue(fixedHeightMatches.Count < 5, "Too many fixed heights for responsive design");
        }
    }

    [TestClass]
    public class CodeQualityTests
    {
        private static string _solutionDir = null!;
        private static List<string> _csFiles = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            _solutionDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
            _csFiles = Directory.GetFiles(Path.Combine(_solutionDir, "charmap"), "*.cs", SearchOption.AllDirectories)
                                .Where(f => !f.Contains("\\obj\\"))
                                .ToList();
        }

        [TestMethod]
        public void NoEmptyCatchBlocks()
        {
            var violations = new List<string>();
            foreach (var file in _csFiles)
            {
                var text = File.ReadAllText(file);
                if (Regex.IsMatch(text, @"catch\s*\{\s*\}"))
                {
                    violations.Add(Path.GetFileName(file));
                }
            }
            Assert.AreEqual(0, violations.Count, "Empty catch blocks found in: " + string.Join(", ", violations));
        }

        [TestMethod]
        public void AllEventHandlers_AreInternalOrPublic()
        {
            var violations = new List<string>();
            foreach (var file in _csFiles)
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (Regex.IsMatch(line, @"\b(private)\s+\w+\s+\w+_Click\b") ||
                        Regex.IsMatch(line, @"\b(private)\s+\w+\s+\w+_SelectionChanged\b"))
                    {
                        violations.Add($"{Path.GetFileName(file)}:{i + 1}");
                    }
                }
            }
            Assert.AreEqual(0, violations.Count, "Event handlers should be internal or public: " + string.Join("; ", violations));
        }

        [TestMethod]
        public void NoHardcodedEnglishStringsOutsideFallbacks()
        {
            var forbidden = new[] { "Character Map", "Font:", "Help", "Characters to copy:", "Select", "Copy", "Advanced view", "Close" };
            var violations = new List<string>();

            foreach (var file in _csFiles)
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.Contains("GetString(")) continue;
                    foreach (var f in forbidden)
                    {
                        if (line.Contains($"\"{f}\""))
                        {
                            violations.Add($"{Path.GetFileName(file)}:{i + 1} => {f}");
                        }
                    }
                }
            }

            Assert.AreEqual(0, violations.Count, "Hardcoded English strings found: " + string.Join("; ", violations));
        }
    }

}
