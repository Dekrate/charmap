using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Composition.SystemBackdrops;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.DataTransfer;

namespace charmap
{
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<CharacterItem> _characters = new();
        private ObservableCollection<CharacterItem> _allCharacters = new();
        private List<string> _fontFamilies = new();

        public MainWindow()
        {
            try
            {
                AppLogger.Info("MainWindow constructor starting");
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                this.InitializeComponent();
                SetupTitleBar();
                SetupMica();
                LoadFonts();
                SetupLocalization();
                SetupAdvancedPanel();
                LoadCharacters();
                AppLogger.Info("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                AppLogger.Fatal("MainWindow constructor failed", ex);
                throw;
            }
        }

        private void SetupTitleBar()
        {
            try
            {
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppTitleBar);
                var title = GetString("AppTitle", "Tablica znak\u00F3w");
                this.Title = title;
                TitleTextBlock.Text = title;
                AppLogger.Info("TitleBar setup completed");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SetupTitleBar failed", ex);
                throw;
            }
        }

        private void SetupMica()
        {
            try
            {
                SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
                AppLogger.Info("Mica Alt backdrop applied");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SetupMica failed", ex);
                throw;
            }
        }

        private void LoadFonts()
        {
            try
            {
                AppLogger.Info("Loading system fonts...");
                _fontFamilies = CanvasTextFormat.GetSystemFontFamilies()
                    .OrderBy(f => f)
                    .ToList();

                if (_fontFamilies.Count == 0)
                {
                    AppLogger.Info("No system fonts found, using fallback list");
                    _fontFamilies = new List<string> { "Arial", "Segoe UI", "Calibri", "Times New Roman", "Consolas" };
                }

                FontComboBox.ItemsSource = _fontFamilies;
                FontComboBox.SelectedItem = _fontFamilies.Contains("Arial") ? "Arial" : _fontFamilies.First();
                FontComboBox.SelectionChanged += FontComboBox_SelectionChanged;
                AppLogger.Info($"Loaded {_fontFamilies.Count} fonts");
            }
            catch (Exception ex)
            {
                AppLogger.Error("LoadFonts failed, using emergency fallback", ex);
                _fontFamilies = new List<string> { "Arial", "Segoe UI", "Calibri" };
                FontComboBox.ItemsSource = _fontFamilies;
                FontComboBox.SelectedItem = "Arial";
                FontComboBox.SelectionChanged += FontComboBox_SelectionChanged;
            }
        }

        internal void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (FontComboBox.SelectedItem is string fontName)
                {
                    CharacterGridView.FontFamily = new FontFamily(fontName);
                    AppLogger.Info($"Font changed to: {fontName}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("FontComboBox_SelectionChanged failed", ex);
            }
        }

        private void SetupAdvancedPanel()
        {
            try
            {
                var characterSets = GetCharacterSets();
                CharacterSetComboBox.ItemsSource = characterSets;
                CharacterSetComboBox.SelectedItem = characterSets.FirstOrDefault(c => c.Name == "Unicode");
                CharacterSetComboBox.DisplayMemberPath = "Name";

                var groups = GetGroups();
                GroupByComboBox.ItemsSource = groups;
                GroupByComboBox.SelectedItem = groups.FirstOrDefault(g => g == "Wszystko" || g == "All");

                GoToUnicodePrefix.Text = "U+";
                AppLogger.Info("Advanced panel setup completed");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SetupAdvancedPanel failed", ex);
                throw;
            }
        }

        private List<CharacterSetInfo> GetCharacterSets()
        {
            var allLabel = GetString("CharacterSetUnicode", "Unicode");
            return new List<CharacterSetInfo>
            {
                new CharacterSetInfo { Name = allLabel, CodePage = 0 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetArabic", "Arabic"), CodePage = 720 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetBaltic", "Baltic"), CodePage = 775 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetCyrillic", "Cyrillic"), CodePage = 855 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetCyrillic2", "Cyrillic II"), CodePage = 866 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetCentralEurope", "Central Europe"), CodePage = 852 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetWesternEurope", "Western Europe"), CodePage = 850 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetGreek", "Greek"), CodePage = 737 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetHebrew", "Hebrew"), CodePage = 862 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetUnitedStates", "United States"), CodePage = 437 },
                new CharacterSetInfo { Name = "DOS: " + GetString("CharSetTurkish", "Turkish"), CodePage = 857 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetArabic", "Arabic"), CodePage = 1256 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetBaltic", "Baltic"), CodePage = 1257 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetChineseTraditional", "Chinese Traditional"), CodePage = 950 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetChineseSimplified", "Chinese Simplified"), CodePage = 936 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetCyrillic", "Cyrillic"), CodePage = 1251 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetCentralEurope", "Central Europe"), CodePage = 1250 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetGreek", "Greek"), CodePage = 1253 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetHebrew", "Hebrew"), CodePage = 1255 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetJapanese", "Japanese"), CodePage = 932 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetKorean", "Korean"), CodePage = 949 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetThai", "Thai"), CodePage = 874 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetTurkish", "Turkish"), CodePage = 1254 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetVietnamese", "Vietnamese"), CodePage = 1258 },
                new CharacterSetInfo { Name = "Windows: " + GetString("CharSetWestern", "Western"), CodePage = 1252 },
                new CharacterSetInfo { Name = allLabel, CodePage = 0 },
            };
        }

        private List<string> GetGroups()
        {
            return new List<string>
            {
                GetString("GroupAll", "All"),
                GetString("GroupChineseTraditionalBopomofo", "Chinese Traditional by Bopomofo"),
                GetString("GroupChineseSimplifiedPinyin", "Chinese Simplified by PinYin"),
                GetString("GroupIdeogramsRadicals", "Ideograms by Radicals"),
                GetString("GroupKoreanHanjaHangul", "Korean Hanja by Hangul"),
                GetString("GroupUnicodeSubset", "Unicode Subset"),
            };
        }

        private void LoadCharacters()
        {
            try
            {
                AppLogger.Info("Loading Unicode characters...");
                _characters.Clear();
                _allCharacters.Clear();
                var ranges = new (int start, int end)[]
                {
                    (0x21, 0x7E),      // Basic Latin
                    (0xA1, 0xFF),      // Latin-1 Supplement
                    (0x100, 0x17F),    // Latin Extended-A
                    (0x180, 0x24F),    // Latin Extended-B
                    (0x250, 0x2AF),    // IPA Extensions
                    (0x2B0, 0x2FF),    // Spacing Modifier Letters
                    (0x300, 0x36F),    // Combining Diacritical Marks
                    (0x370, 0x3FF),    // Greek and Coptic
                    (0x400, 0x4FF),    // Cyrillic
                    (0x1E00, 0x1EFF),  // Latin Extended Additional
                    (0x2000, 0x206F),  // General Punctuation
                    (0x2070, 0x209F),  // Superscripts and Subscripts
                    (0x20A0, 0x20CF),  // Currency Symbols
                    (0x2100, 0x214F),  // Letterlike Symbols
                    (0x2150, 0x218F),  // Number Forms
                    (0x2190, 0x21FF),  // Arrows
                    (0x2200, 0x22FF),  // Mathematical Operators
                    (0x2300, 0x23FF),  // Miscellaneous Technical
                    (0x2500, 0x257F),  // Box Drawing
                    (0x2580, 0x259F),  // Block Elements
                    (0x25A0, 0x25FF),  // Geometric Shapes
                    (0x2600, 0x26FF),  // Miscellaneous Symbols
                    (0x2700, 0x27BF),  // Dingbats
                    (0x3000, 0x303F),  // CJK Symbols and Punctuation
                };

                int count = 0;
                foreach (var (start, end) in ranges)
                {
                    for (int i = start; i <= end; i++)
                    {
                        try
                        {
                            var item = new CharacterItem
                            {
                                Character = char.ConvertFromUtf32(i),
                                CodePoint = i
                            };
                            _characters.Add(item);
                            _allCharacters.Add(item);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            AppLogger.Error($"Failed to add character at U+{i:X4}", ex);
                        }
                    }
                }

                CharacterGridView.ItemsSource = _characters;
                AppLogger.Info($"Loaded {count} characters into grid");
            }
            catch (Exception ex)
            {
                AppLogger.Error("LoadCharacters failed", ex);
                throw;
            }
        }

        private void LoadCharactersForCodePage(int codePage)
        {
            try
            {
                AppLogger.Info($"Loading characters for code page {codePage}...");
                _characters.Clear();
                _allCharacters.Clear();

                if (codePage == 0)
                {
                    LoadCharacters();
                    return;
                }

                var encoding = Encoding.GetEncoding(codePage);
                int count = 0;
                for (int i = 0x20; i <= 0xFF; i++)
                {
                    try
                    {
                        var bytes = new byte[] { (byte)i };
                        string ch = encoding.GetString(bytes);
                        if (ch.Length == 1 && ch[0] == '\u003F' && i != 0x3F) continue; // skip unmapped '?' characters
                        if (ch.Length > 0 && ch[0] != '\0')
                        {
                            int cp = char.ConvertToUtf32(ch, 0);
                            var item = new CharacterItem { Character = ch, CodePoint = cp };
                            _characters.Add(item);
                            _allCharacters.Add(item);
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error($"Failed to decode byte 0x{i:X2} for code page {codePage}", ex);
                    }
                }

                CharacterGridView.ItemsSource = _characters;
                AppLogger.Info($"Loaded {count} characters for code page {codePage}");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"LoadCharactersForCodePage failed for code page {codePage}", ex);
            }
        }

        private void CharacterGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is CharacterItem item)
                {
                    CopyTextBox.Text += item.Character;
                    UpdateSelectedCharacterInfo(item);
                    AppLogger.Info($"Character clicked: U+{item.CodePoint:X4} '{item.Character}'");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("CharacterGridView_ItemClick failed", ex);
            }
        }

        internal void UpdateSelectedCharacterInfo(CharacterItem item)
        {
            try
            {
                var blockName = UnicodeHelper.GetUnicodeBlockName(item.CodePoint);
                var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(item.Character, 0);
                SelectedCharacterTextBlock.Text = $"U+{item.CodePoint:X4} ({item.Character}): {blockName} — {category}";
            }
            catch (Exception ex)
            {
                AppLogger.Error("UpdateSelectedCharacterInfo failed", ex);
            }
        }

        private void AdvancedViewCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AdvancedPanel.Visibility = Visibility.Visible;
            AppLogger.Info("Advanced view enabled");
        }

        private void AdvancedViewCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AdvancedPanel.Visibility = Visibility.Collapsed;
            AppLogger.Info("Advanced view disabled");
        }

        internal void CharacterSetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CharacterSetComboBox.SelectedItem is CharacterSetInfo info)
                {
                    LoadCharactersForCodePage(info.CodePage);
                    ApplyGroupFilter();
                    AppLogger.Info($"Character set changed to: {info.Name} (code page {info.CodePage})");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("CharacterSetComboBox_SelectionChanged failed", ex);
            }
        }

        internal void GroupByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ApplyGroupFilter();
                AppLogger.Info("Group by changed");
            }
            catch (Exception ex)
            {
                AppLogger.Error("GroupByComboBox_SelectionChanged failed", ex);
            }
        }

        private void ApplyGroupFilter()
        {
            try
            {
                if (GroupByComboBox.SelectedItem is not string group)
                    return;

                var allLabel = GetString("GroupAll", "All");
                var traditionalLabel = GetString("GroupChineseTraditionalBopomofo", "Chinese Traditional by Bopomofo");
                var simplifiedLabel = GetString("GroupChineseSimplifiedPinyin", "Chinese Simplified by PinYin");
                var radicalsLabel = GetString("GroupIdeogramsRadicals", "Ideograms by Radicals");
                var koreanLabel = GetString("GroupKoreanHanjaHangul", "Korean Hanja by Hangul");
                var subsetLabel = GetString("GroupUnicodeSubset", "Unicode Subset");

                if (group == allLabel)
                {
                    _characters = new ObservableCollection<CharacterItem>(_allCharacters);
                }
                else if (group == traditionalLabel)
                {
                    _characters = new ObservableCollection<CharacterItem>(
                        _allCharacters.Where(c => (c.CodePoint >= 0x4E00 && c.CodePoint <= 0x9FFF) || (c.CodePoint >= 0x3100 && c.CodePoint <= 0x312F)));
                }
                else if (group == simplifiedLabel)
                {
                    _characters = new ObservableCollection<CharacterItem>(
                        _allCharacters.Where(c => c.CodePoint >= 0x4E00 && c.CodePoint <= 0x9FFF));
                }
                else if (group == radicalsLabel)
                {
                    _characters = new ObservableCollection<CharacterItem>(
                        _allCharacters.Where(c => (c.CodePoint >= 0x2E80 && c.CodePoint <= 0x2EFF) || (c.CodePoint >= 0x2F00 && c.CodePoint <= 0x2FDF)));
                }
                else if (group == koreanLabel)
                {
                    _characters = new ObservableCollection<CharacterItem>(
                        _allCharacters.Where(c => c.CodePoint >= 0xAC00 && c.CodePoint <= 0xD7AF));
                }
                else if (group == subsetLabel)
                {
                    _characters = new ObservableCollection<CharacterItem>(_allCharacters);
                }

                CharacterGridView.ItemsSource = _characters;
            }
            catch (Exception ex)
            {
                AppLogger.Error("ApplyGroupFilter failed", ex);
            }
        }

        internal void GoToUnicodeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = GoToUnicodeTextBox.Text.Trim();
                if (string.IsNullOrEmpty(input))
                    return;

                if (!int.TryParse(input, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                {
                    AppLogger.Info($"Invalid Unicode input: {input}");
                    return;
                }

                var item = _characters.FirstOrDefault(c => c.CodePoint == codePoint);
                if (item != null)
                {
                    CharacterGridView.ScrollIntoView(item);
                    UpdateSelectedCharacterInfo(item);
                    AppLogger.Info($"Scrolled to U+{codePoint:X4}");
                }
                else
                {
                    AppLogger.Info($"Character U+{codePoint:X4} not found in current set");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("GoToUnicodeButton_Click failed", ex);
            }
        }

        internal void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var query = SearchTextBox.Text.Trim();
                if (string.IsNullOrEmpty(query))
                {
                    _characters = new ObservableCollection<CharacterItem>(_allCharacters);
                    CharacterGridView.ItemsSource = _characters;
                    return;
                }

                // Search by exact character or by Unicode code point
                var results = _allCharacters.Where(c =>
                    c.Character.Contains(query) ||
                    c.CodePoint.ToString("X4").Equals(query, StringComparison.OrdinalIgnoreCase) ||
                    ($"U+{c.CodePoint:X4}").Equals(query, StringComparison.OrdinalIgnoreCase) ||
                    UnicodeHelper.GetUnicodeBlockName(c.CodePoint).Contains(query, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                _characters = new ObservableCollection<CharacterItem>(results);
                CharacterGridView.ItemsSource = _characters;
                AppLogger.Info($"Search for '{query}' returned {results.Count} results");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SearchButton_Click failed", ex);
            }
        }

        internal void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyTextBox.SelectAll();
                CopyTextBox.Focus(FocusState.Programmatic);
                AppLogger.Info("SelectButton clicked");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SelectButton_Click failed", ex);
            }
        }

        internal void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var package = new DataPackage();
                package.SetText(CopyTextBox.Text);
                Clipboard.SetContent(package);
                AppLogger.Info("CopyButton clicked, text copied to clipboard");
            }
            catch (Exception ex)
            {
                AppLogger.Error("CopyButton_Click failed", ex);
            }
        }

        internal void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = GetString("HelpTitle", "Pomoc"),
                    Content = GetString("HelpContent", "Wybierz czcionk\u0119 i kliknij znak, aby doda\u0107 go do pola do skopiowania. U\u017Cyj przycisku Kopiuj, aby skopiowa\u0107 znaki do schowka."),
                    CloseButtonText = GetString("CloseButton", "Zamknij"),
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
                AppLogger.Info("Help dialog opened");
            }
            catch (Exception ex)
            {
                AppLogger.Error("HelpButton_Click failed", ex);
            }
        }

        private void SetupLocalization()
        {
            try
            {
                FontLabel.Text = GetString("FontLabel", "Czcionka:");
                HelpButton.Content = GetString("HelpButton", "Pomoc");
                CopyLabel.Text = GetString("CopyLabel", "Znaki do skopiowania:");
                SelectButton.Content = GetString("SelectButton", "Wybierz");
                CopyButton.Content = GetString("CopyButton", "Kopiuj");
                AdvancedViewCheckBox.Content = GetString("AdvancedView", "Widok zaawansowany");
                CharacterSetLabel.Text = GetString("CharacterSetLabel", "Zestaw znak\u00F3w:");
                GoToUnicodeLabel.Text = GetString("GoToUnicodeLabel", "Przejd\u017A do Unicode:");
                GoToUnicodeButton.Content = GetString("GoToUnicodeButton", "Przejd\u017A");
                GroupByLabel.Text = GetString("GroupByLabel", "Grupa wed\u0142ug:");
                SearchLabel.Text = GetString("SearchLabel", "Wyszukaj:");
                SearchButton.Content = GetString("SearchButton", "Wyszukaj");
                AppLogger.Info("Localization setup completed");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SetupLocalization failed", ex);
                throw;
            }
        }

        private string GetString(string key, string fallback)
        {
            try
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var value = loader.GetString(key);
                if (string.IsNullOrEmpty(value))
                {
                    AppLogger.Info($"Resource key '{key}' returned empty, using fallback");
                    return fallback;
                }
                return value;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"GetString failed for key '{key}', using fallback", ex);
                return fallback;
            }
        }
    }

    public class CharacterSetInfo
    {
        public string Name { get; set; } = "";
        public int CodePage { get; set; }
    }
}
