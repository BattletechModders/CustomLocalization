using BattleTech;
using Localize;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BTLocalization {
  public enum SystemLanguage
  {
    Afrikaans,
    Arabic,
    Basque,
    Belarusian,
    Bulgarian,
    Catalan,
    Chinese,
    Czech,
    Danish,
    Dutch,
    English,
    Estonian,
    Faroese,
    Finnish,
    French,
    German,
    Greek,
    Hebrew,
    Hungarian,
    Icelandic,
    Indonesian,
    Italian,
    Japanese,
    Korean,
    Latvian,
    Lithuanian,
    Norwegian,
    Polish,
    Portuguese,
    Romanian,
    Russian,
    SerboCroatian,
    Slovak,
    Slovenian,
    Spanish,
    Swedish,
    Thai,
    Turkish,
    Ukrainian,
    Vietnamese,
    ChineseSimplified,
    ChineseTraditional,
    Unknown,
  }
  public class Text {
    public static BTLocalization.Text Blank = new BTLocalization.Text();
    public static readonly BTLocalization.Text Empty = new BTLocalization.Text();
    public List<BTLocalization.Text.Part> m_parts = new List<BTLocalization.Text.Part>();

    public Text() {
    }

    public Text(string text, params object[] args) {
      this.Append(text, args);
    }

    public Text(BTLocalization.Text copy) {
      foreach (BTLocalization.Text.Part part in copy.m_parts)
        this.Add(part);
    }

    public virtual void Append(string text, params object[] args) {
      this.m_parts.Add((BTLocalization.Text.Part)new BTLocalization.Text.AppendPart(text, args));
    }

    public virtual void AppendLine(string text, params object[] args) {
      this.Append(text, args);
      this.Append("\n", (object[])Array.Empty<object>());
    }

    protected virtual void Add(BTLocalization.Text.Part part) {
      this.m_parts.Add(part);
    }

    public void Add(BTLocalization.Text text) {
      this.m_parts.Add((BTLocalization.Text.Part)new BTLocalization.Text.AppendTextPart(text));
    }

    public virtual void AppendLine(BTLocalization.Text text) {
      this.Add(text);
      this.Append("\n", (object[])Array.Empty<object>());
    }

    public virtual void Replace(string text, string replacement) {
      this.m_parts.Add((BTLocalization.Text.Part)new BTLocalization.Text.ReplacePart(text, new BTLocalization.Text(replacement, (object[])Array.Empty<object>())));
    }

    public virtual void Replace(string text, BTLocalization.Text replacement) {
      this.m_parts.Add((BTLocalization.Text.Part)new BTLocalization.Text.ReplacePart(text, replacement));
    }

    public override string ToString() {
      return this.ToString(true);
    }

    public virtual string ToString(bool localize = true) {
      StringBuilder resultBuilder = new StringBuilder();
      foreach (BTLocalization.Text.Part part in this.m_parts)
        part.IntegrateWith(resultBuilder, localize);
      return resultBuilder.ToString();
    }

    public static bool IsNullOrEmpty(BTLocalization.Text text) {
      if (text == null || text.m_parts == null || text.m_parts.Count == 0)
        return true;
      for (int index = 0; index < text.m_parts.Count; ++index) {
        if (text.m_parts[index] != null && !text.m_parts[index].IsNullOrEmpty())
          return string.IsNullOrEmpty(text.ToString(true));
      }
      return true;
    }

    [Serializable]
    public class Part {
      public string text;
      public object[] args;

      public virtual void IntegrateWith(StringBuilder resultBuilder, bool localize) {
      }

      public virtual bool IsNullOrEmpty() {
        return true;
      }
    }

    [Serializable]
    public class AppendPart : BTLocalization.Text.Part {
      public AppendPart(string text, object[] args) {
        this.text = text;
        this.args = args;
      }

      public override void IntegrateWith(StringBuilder resultBuilder, bool localize) {
        try {
          if (this.args.Length == 0)
            resultBuilder.Append(localize ? Strings.T(this.text) : this.text);
          else
            resultBuilder.Append(localize ? Strings.T(this.text, this.args) : string.Format(this.text, this.args));
        } catch (FormatException) {
          if (!this.text.Contains("{"))
            return;
        }
      }

      public override bool IsNullOrEmpty() {
        if (this.text != null)
          return this.text == "";
        return true;
      }
    }

    public class AppendTextPart : BTLocalization.Text.Part {
      public new BTLocalization.Text text;

      public AppendTextPart(BTLocalization.Text text) {
        this.text = text;
      }

      public override void IntegrateWith(StringBuilder resultBuilder, bool localize) {
        resultBuilder.Append(this.text.ToString(localize));
      }

      public override bool IsNullOrEmpty() {
        return BTLocalization.Text.IsNullOrEmpty(this.text);
      }
    }

    public class ReplacePart : BTLocalization.Text.Part {
      public string textToReplace;

      public BTLocalization.Text replacement {
        get {
          if (this.args == null)
            return (BTLocalization.Text)null;
          return this.args[0] as BTLocalization.Text;
        }
        set {
          if (this.args == null)
            this.args = new object[1];
          this.args[0] = (object)value;
        }
      }

      public ReplacePart(string textToReplace, BTLocalization.Text replacement) {
        this.textToReplace = textToReplace;
        this.replacement = replacement != null ? replacement : new BTLocalization.Text();
      }

      public override void IntegrateWith(StringBuilder resultBuilder, bool localize) {
        string newValue = this.replacement.ToString(localize);
        resultBuilder.Replace(this.textToReplace, newValue);
      }

      public override bool IsNullOrEmpty() {
        return this.textToReplace == null;
      }
    }
  }
  public class CSVStringsProvider : StringsProviderBase<CSVReader> {
    public CSVStringsProvider() {
      this.resourceFileExtName = "CSV";
      this.resourceFileExt = ".csv";
    }

    public override void LoadCultureFromReader(CSVReader reader) {
      int keyRow = -1;
      int cultureRow = -1;
      reader.Rewind();
      List<string> stringList = reader.ReadRow();
      for (int index = 0; index < stringList.Count; ++index) {
        if (stringList[index] == "KEY")
          keyRow = index;
        else if (Strings.CultureFromString(stringList[index].Replace("\"", "")) == this.culture)
          cultureRow = index;
        if (keyRow > -1 && cultureRow > -1)
          break;
      }
      if (cultureRow < 0)
        return;
      this.activeStrings = CSVStringsProvider.ReadRowsFromReader(reader, stringList.Count, keyRow, cultureRow, true, this.normalizeKeys);
    }

    public static Dictionary<string, string> ReadRowsFromReader(CSVReader reader, int numColumns, int keyRow, int cultureRow, bool postProcess = true, bool normalizeKey = false) {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      List<string> stringList = reader.ReadRow();
      CaseInsensitiveIgnoresWhitespaceStringComparer whitespaceStringComparer = new CaseInsensitiveIgnoresWhitespaceStringComparer();
      int num = 0;
      for (; stringList != null; stringList = reader.ReadRow()) {
        ++num;
        if (stringList.Count == numColumns) {
          string str1 = stringList[keyRow];
          string index = str1;
          if (normalizeKey)
            index = whitespaceStringComparer.NormalizeString(index);
          try {
            string str2 = stringList[cultureRow];
            if (postProcess)
              str2 = CSVHelper.PostProcessValue(str2);
            if (!string.IsNullOrEmpty(str2)) {
              if (dictionary.ContainsKey(index)) {
                if (str2 != index) {
                  if (str2 != str1)
                    dictionary[index] = str2;
                }
              } else
                dictionary.Add(index, str2);
            }
          } catch (Exception) {
          }
        } else {
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.AppendFormat("column mismatch in row {0}, expected {1} columns, found {2}, key={3}", (object)num, (object)numColumns, (object)stringList.Count, (object)stringList[keyRow]);
          for (int index = 0; index < stringList.Count; ++index) {
            stringBuilder.Append("col[{0]]={1}");
            if (index < stringList.Count - 1)
              stringBuilder.Append(",");
          }
        }
      }
      return dictionary;
    }

    protected override CSVReader GetReader(string path) {
      return new CSVReader(path, ',');
    }
  }
  public static class Strings {
    private static Dictionary<Localize.Strings.Culture, string> languageNames = new Dictionary<Localize.Strings.Culture, string>()
    {
      {
        Localize.Strings.Culture.CULTURE_EN_US,
        "English"
      },
      {
        Localize.Strings.Culture.CULTURE_DE_DE,
        "Deutsch"
      },
      {
        Localize.Strings.Culture.CULTURE_ZH_CN,
        "中文"
      },
      {
        Localize.Strings.Culture.CULTURE_ES_ES,
        "Español"
      },
      {
        Localize.Strings.Culture.CULTURE_FR_FR,
        "Français"
      },
      {
        Localize.Strings.Culture.CULTURE_IT_IT,
        "Italiano"
      },
      {
        Localize.Strings.Culture.CULTURE_RU_RU,
        "русский"
      },
      {
        Localize.Strings.Culture.CULTURE_PT_PT,
        "Português"
      },
      {
        Localize.Strings.Culture.CULTURE_PT_BR,
        "Português"
      },
      {
        Localize.Strings.Culture.CULTURE_DEV_WWW,
        "Test"
      }
    };
    private static Dictionary<Localize.Strings.Culture, string> languageNamesEnglish = new Dictionary<Localize.Strings.Culture, string>()
    {
      {
        Localize.Strings.Culture.CULTURE_EN_US,
        "English"
      },
      {
        Localize.Strings.Culture.CULTURE_DE_DE,
        "German"
      },
      {
        Localize.Strings.Culture.CULTURE_ZH_CN,
        "Mandarin"
      },
      {
        Localize.Strings.Culture.CULTURE_ES_ES,
        "Spanish"
      },
      {
        Localize.Strings.Culture.CULTURE_FR_FR,
        "French"
      },
      {
        Localize.Strings.Culture.CULTURE_IT_IT,
        "Italian"
      },
      {
        Localize.Strings.Culture.CULTURE_RU_RU,
        "Russian"
      },
      {
        Localize.Strings.Culture.CULTURE_PT_PT,
        "Portuguese"
      },
      {
        Localize.Strings.Culture.CULTURE_PT_BR,
        "Portuguese"
      },
      {
        Localize.Strings.Culture.CULTURE_DEV_WWW,
        "Test"
      }
    };
    private static Dictionary<Localize.Strings.Culture, string> localeNames = new Dictionary<Localize.Strings.Culture, string>()
    {
      {
        Localize.Strings.Culture.CULTURE_EN_US,
        "United States"
      },
      {
        Localize.Strings.Culture.CULTURE_DE_DE,
        "Deutschland"
      },
      {
        Localize.Strings.Culture.CULTURE_ZH_CN,
        "中国"
      },
      {
        Localize.Strings.Culture.CULTURE_ES_ES,
        "España"
      },
      {
        Localize.Strings.Culture.CULTURE_FR_FR,
        "France"
      },
      {
        Localize.Strings.Culture.CULTURE_IT_IT,
        "Italia"
      },
      {
        Localize.Strings.Culture.CULTURE_RU_RU,
        "Россия"
      },
      {
        Localize.Strings.Culture.CULTURE_PT_PT,
        "Portugal"
      },
      {
        Localize.Strings.Culture.CULTURE_PT_BR,
        "Brasil"
      },
      {
        Localize.Strings.Culture.CULTURE_DEV_WWW,
        "Test"
      }
    };
    private static Dictionary<string, Localize.Strings.Culture> cultureMap = new Dictionary<string, Localize.Strings.Culture>()
    {
      {
        "en-US",
        Localize.Strings.Culture.CULTURE_EN_US
      },
      {
        "de-DE",
        Localize.Strings.Culture.CULTURE_DE_DE
      },
      {
        "zh-CN",
        Localize.Strings.Culture.CULTURE_ZH_CN
      },
      {
        "es-ES",
        Localize.Strings.Culture.CULTURE_ES_ES
      },
      {
        "fr-FR",
        Localize.Strings.Culture.CULTURE_FR_FR
      },
      {
        "it-IT",
        Localize.Strings.Culture.CULTURE_IT_IT
      },
      {
        "ru-RU",
        Localize.Strings.Culture.CULTURE_RU_RU
      },
      {
        "pt-PT",
        Localize.Strings.Culture.CULTURE_PT_PT
      },
      {
        "pt-BR",
        Localize.Strings.Culture.CULTURE_PT_BR
      },
      {
        "dev-WWW",
        Localize.Strings.Culture.CULTURE_DEV_WWW
      }
    };
    private static Dictionary<Localize.Strings.Culture, CultureInfo> cultureInfos = new Dictionary<Localize.Strings.Culture, CultureInfo>()
    {
      {
        Localize.Strings.Culture.CULTURE_EN_US,
        new CultureInfo("en-US")
      },
      {
        Localize.Strings.Culture.CULTURE_DE_DE,
        new CultureInfo("de-DE")
      },
      {
        Localize.Strings.Culture.CULTURE_ZH_CN,
        new CultureInfo("zh-CN")
      },
      {
        Localize.Strings.Culture.CULTURE_ES_ES,
        new CultureInfo("es-ES")
      },
      {
        Localize.Strings.Culture.CULTURE_FR_FR,
        new CultureInfo("fr-FR")
      },
      {
        Localize.Strings.Culture.CULTURE_IT_IT,
        new CultureInfo("it-IT")
      },
      {
        Localize.Strings.Culture.CULTURE_RU_RU,
        new CultureInfo("ru-RU")
      },
      {
        Localize.Strings.Culture.CULTURE_PT_PT,
        new CultureInfo("pt-PT")
      },
      {
        Localize.Strings.Culture.CULTURE_PT_BR,
        new CultureInfo("pt-BR")
      },
      {
        Localize.Strings.Culture.CULTURE_DEV_WWW,
        new CultureInfo("en-US")
      }
    };
    public static bool useDebugLocalization = false;
    public static bool dontLocalizeFormattedStrings = false;
    public static bool normalizeKeys = false;
    private static IStringsProvider provider;
    private static IFontProvider fontProvider;

    public static string GetCultureName(Localize.Strings.Culture c) {
      return Strings.languageNames[c];
    }

    public static string GetCultureNameEnglish(Localize.Strings.Culture c) {
      return Strings.languageNamesEnglish[c];
    }

    public static string GetLocaleName(Localize.Strings.Culture c) {
      return Strings.localeNames[c];
    }

    public static CultureInfo GetCultureInfo(Localize.Strings.Culture c) {
      return Strings.cultureInfos[c];
    }

    public static string CurrentCultureName {
      get {
        return Strings.GetCultureName(Strings.CurrentCulture);
      }
    }

    public static string CurrentCultureNameEnglish {
      get {
        return Strings.GetCultureNameEnglish(Strings.CurrentCulture);
      }
    }

    public static string CurrentLocaleName {
      get {
        return Strings.GetLocaleName(Strings.CurrentCulture);
      }
    }

    public static CultureInfo CurrentCultureInfo {
      get {
        return Strings.GetCultureInfo(Strings.CurrentCulture);
      }
    }

    public static Localize.Strings.Culture FindCultureById(string id) {
      if (Strings.cultureMap.ContainsKey(id))
        return Strings.cultureMap[id];
      return Localize.Strings.Culture.UNDEFINED;
    }

    public static string CultureToString(Localize.Strings.Culture c) {
      string str = c.ToString().Replace("CULTURE_", "");
      string[] strArray = str.Split('_');
      if (strArray.Length >= 1) {
        str = strArray[0].ToLower();
        if (strArray.Length > 1)
          str = str + "-" + strArray[1].ToUpper();
      }
      return str;
    }

    public static Localize.Strings.Culture CultureFromString(string source) {
      string str = "CULTURE_" + source.Replace("-", "_").ToUpperInvariant();
      Localize.Strings.Culture culture = Localize.Strings.Culture.CULTURE_EN_US;
      try {
        culture = (Localize.Strings.Culture)Enum.Parse(typeof(Localize.Strings.Culture), str);
      } catch (Exception) {
      }
      return culture;
    }

    public static Localize.Strings.Culture CultureFromSystemLanguage(SystemLanguage lang) {
      switch (lang) {
        case SystemLanguage.Chinese:
          return Localize.Strings.Culture.CULTURE_ZH_CN;
        case SystemLanguage.French:
          return Localize.Strings.Culture.CULTURE_FR_FR;
        case SystemLanguage.German:
          return Localize.Strings.Culture.CULTURE_DE_DE;
        case SystemLanguage.Italian:
          return Localize.Strings.Culture.CULTURE_IT_IT;
        case SystemLanguage.Portuguese:
          return Localize.Strings.Culture.CULTURE_PT_BR;
        case SystemLanguage.Russian:
          return Localize.Strings.Culture.CULTURE_RU_RU;
        case SystemLanguage.Spanish:
          return Localize.Strings.Culture.CULTURE_ES_ES;
        default:
          return Localize.Strings.Culture.CULTURE_EN_US;
      }
    }

    public static SystemLanguage CultureToSystemLanguage(Localize.Strings.Culture culture) {
      switch (culture) {
        case Localize.Strings.Culture.CULTURE_DE_DE:
          return SystemLanguage.German;
        case Localize.Strings.Culture.CULTURE_ZH_CN:
          return SystemLanguage.Chinese;
        case Localize.Strings.Culture.CULTURE_ES_ES:
          return SystemLanguage.Spanish;
        case Localize.Strings.Culture.CULTURE_FR_FR:
          return SystemLanguage.French;
        case Localize.Strings.Culture.CULTURE_IT_IT:
          return SystemLanguage.Italian;
        case Localize.Strings.Culture.CULTURE_RU_RU:
          return SystemLanguage.Italian;
        case Localize.Strings.Culture.CULTURE_PT_BR:
          return SystemLanguage.Portuguese;
        default:
          return SystemLanguage.English;
      }
    }

    public static Localize.Strings.Culture CurrentCulture {
      get {
        if (!Strings.Initialized)
          return Strings.GetDefaultCulture();
        return Strings.provider.CurrentCulture;
      }
    }

    public static void LoadCulture(Localize.Strings.Culture value, bool normalizeKeys = false) {
      if (!Strings.Initialized)
        return;
      CSVStringsProvider provider = Strings.provider as CSVStringsProvider;
      if (provider != null)
        provider.normalizeKeys = normalizeKeys;
      Strings.provider.LoadCulture(value);
      Thread.CurrentThread.CurrentCulture = Strings.CurrentCultureInfo;
    }

    public static bool Initialized {
      get {
        return Strings.provider != null;
      }
    }

    public static Localize.Strings.Culture GetDefaultCulture() {
      Localize.Strings.Culture culture = Localize.Strings.Culture.UNDEFINED;
      if (culture == Localize.Strings.Culture.UNDEFINED)
        culture = Strings.CultureFromSystemLanguage(SystemLanguage.Russian);
      return culture;
    }

    public static IFontProvider FontProvider {
      get {
        return Strings.fontProvider;
      }
    }

    public static void Initialize(IStringsProvider source, IFontProvider fontSource) {
      Strings.provider = source;
      Strings.fontProvider = fontSource;
      Thread.CurrentThread.CurrentCulture = Strings.CurrentCultureInfo;
    }

    public static string T(string msg) {
      if (!Strings.Initialized)
        return msg;
      return Strings.provider.StringForKey(msg);
    }

    public static bool GetTranslationFor(string msg, out string result) {
      return Strings.provider.GetTranslationFor(msg, out result);
    }

    public static string T(string msg, params object[] args) {
      if (!Strings.Initialized)
        return string.Format(msg, args);
      object[] objArray = new object[args.Length];
      if (objArray == null)
        return Strings.provider.StringForKey(msg);
      for (int index = 0; index < args.Length; ++index) {
        if (args[index] != null) {
          if (typeof(string) == args[index].GetType() && args[index] is string)
            objArray[index] = (object)Strings.T(args[index] as string);
          else if (args[index] is Enum)
            objArray[index] = (object)Strings.T(args[index].ToString());
          else if (args[index] is Text)
            objArray[index] = (object)((Text)args[index]).ToString(true);
          else if (args[index] is NonLocalizeableArg) {
            NonLocalizeableArg nonLocalizeableArg = (NonLocalizeableArg)args[index];
            objArray[index] = !(typeof(string) == nonLocalizeableArg.arg.GetType()) || !(nonLocalizeableArg.arg is string) ? nonLocalizeableArg.arg : (object)(nonLocalizeableArg.arg as string);
          } else
            objArray[index] = args[index];
        }
      }
      if (Strings.IsNumber(msg))
        return Strings.FormatNumber(msg);
      return Strings.provider.StringForKeyFormat(msg, objArray);
    }

    private static string FormatNumber(string strNum) {
      return strNum;
    }

    public static bool IsNumber(string value) {
      int result1;
      float result2;
      double result3;
      return int.TryParse(value, out result1) || float.TryParse(value, out result2) || double.TryParse(value, out result3);
    }

    public static void ListSupportedCultures() {
    }

    public static List<Localize.Strings.Culture> GetSupportedCultures() {
      if (Strings.Initialized)
        return provider.SupportedCultures;
      return (List<Localize.Strings.Culture>)null;
    }

    public static void SetActive(string active) {
      provider.LoadCulture(Strings.CultureFromString(active));
    }

    public static void Active() {
    }

    public static Dictionary<string, string> GetAllTranslationsForCurrentLanguage() {
      return Strings.provider.GetAllTranslations();
    }

  }
  public interface IStringsProvider {
    bool Initialized { get; }

    Localize.Strings.Culture CurrentCulture { get; }

    void LoadCulture(Localize.Strings.Culture culture);

    List<Localize.Strings.Culture> SupportedCultures { get; }

    string StringForKey(string key);

    string StringForKeyFormat(string key, params object[] args);

    Dictionary<string, string> GetAllTranslations();

    bool GetTranslationFor(string key, out string result);

    void Initialize();
  }
  public class CaseInsensitiveIgnoresWhitespaceStringComparer : IEqualityComparer<string> {
    private static readonly List<CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement> normalizeSpaceAndTags = new List<CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement>()
    {
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex(",", RegexOptions.Compiled), "^"),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\.", RegexOptions.Compiled), "*"),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\n", RegexOptions.Compiled), "newline"),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\\\n", RegexOptions.Compiled), "newline"),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\s+", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("(\\[\\[[\\w\\.\\[\\]]+,)", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\]\\]", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("<.+?>", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("[\\u2013\\u2014\\u2018\\u2019\\u2024\\u2026\\u00e9\\u2022]", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\"", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\'", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\\\303\\\\266", RegexOptions.Compiled), "ö"),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\\\\\d\\d\\d\\\\\\d\\d\\d(\\\\\\d\\d\\d)?", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\\\u\\d\\d\\d\\d", RegexOptions.Compiled), ""),
      new CaseInsensitiveIgnoresWhitespaceStringComparer.RegexReplacement(new Regex("\\\\", RegexOptions.Compiled), "")
    };
    private Dictionary<string, string> keyCache = new Dictionary<string, string>();

    public bool Equals(string x, string y) {
      if ((object)x == (object)y)
        return true;
      if (x == null || y == null)
        return false;
      return this.NormalizeString(x).Equals(this.NormalizeString(y));
    }

    public int GetHashCode(string obj) {
      return this.NormalizeString(obj).GetHashCode();
    }

    public string NormalizeString(string input) {
      if (input == null)
        return (string)null;
      string str = input;
      if (!string.IsNullOrEmpty(input) && !this.keyCache.TryGetValue(input, out str)) {
        str = CaseInsensitiveIgnoresWhitespaceStringComparer._NormalizeString(input);
        this.keyCache.Add(input, str);
      }
      return str;
    }

    public static string _NormalizeString(string input) {
      string input1 = input;
      for (int index = 0; index < CaseInsensitiveIgnoresWhitespaceStringComparer.normalizeSpaceAndTags.Count && !string.IsNullOrEmpty(input1); ++index)
        input1 = CaseInsensitiveIgnoresWhitespaceStringComparer.normalizeSpaceAndTags[index].searchPattern.Replace(input1, CaseInsensitiveIgnoresWhitespaceStringComparer.normalizeSpaceAndTags[index].replacement).ToLowerInvariant();
      if (string.IsNullOrEmpty(input1)) {
        //Debug.LogWarning((object)("'" + input + "' normalized to a blank string!"));
        return string.Empty;
      }
      if (input1.StartsWith("\\\"") && input1.EndsWith("\\\""))
        input1 = input1.Substring(2, input1.Length - 4);
      return input1;
    }

    private string TrimTags(string input) {
      StringBuilder stringBuilder = new StringBuilder();
      int num1 = 0;
      int num2 = 0;
      foreach (char ch in input) {
        switch (ch) {
          case '[':
            ++num1;
            break;
          case ']':
            --num1;
            break;
          case '{':
            ++num2;
            break;
          case '}':
            --num2;
            break;
          default:
            if (num1 == 0 && num2 == 0) {
              stringBuilder.Append(ch);
              break;
            }
            break;
        }
      }
      return stringBuilder.ToString();
    }

    private struct RegexReplacement {
      public Regex searchPattern;
      public string replacement;

      public RegexReplacement(Regex s, string r) {
        this.searchPattern = s;
        this.replacement = r;
      }
    }
  }
  public abstract class StringsProviderBase<T> : IStringsProvider where T : class {
    protected static CaseInsensitiveIgnoresWhitespaceStringComparer comparer = new CaseInsensitiveIgnoresWhitespaceStringComparer();
    public Dictionary<Localize.Strings.Culture, T> readers = new Dictionary<Localize.Strings.Culture, T>();
    //protected BattleTechResourceType resourceType = BattleTechResourceType.CSV;
    protected string resourceFileExtName = "CSV";
    protected string resourceFileExt = ".csv";
    private const string STRINGS_FILE_PREFIX = "strings_";
    //protected DataManager dataManager;
    public Localize.Strings.Culture culture;
    protected Dictionary<string, string> activeStrings;
    protected Action<T> postLoadAction;
    //private static HashSet<GameObject> updatedTexts;

    public List<Localize.Strings.Culture> SupportedCultures { get; set; }

    public bool normalizeKeys { get; set; }

    public bool Initialized { get; private set; }

    public Localize.Strings.Culture CurrentCulture {
      get {
        return this.culture;
      }
    }

    public void LoadCulture(Localize.Strings.Culture value) {
      if (value == Localize.Strings.Culture.UNDEFINED) { value = Localize.Strings.Culture.CULTURE_EN_US; };
      this.culture = value;
      if (this.readers.ContainsKey(this.culture)) {
        this.LoadCultureFromReader(this.readers[this.culture]);
      }
    }

    protected abstract T GetReader(string path);

    public abstract void LoadCultureFromReader(T reader);

    public void Initialize() {
      this.activeStrings = new Dictionary<string, string>((IEqualityComparer<string>)StringsProviderBase<T>.comparer);
      this.LoadSupportedCultures();
      this.LoadCulture(Localize.Strings.Culture.CULTURE_RU_RU);
      if (this.CurrentCulture != Localize.Strings.Culture.UNDEFINED)
        return;
      this.LoadCulture(Strings.GetDefaultCulture());
    }

    private void LoadSupportedCultures() {
      this.SupportedCultures = new List<Localize.Strings.Culture>();
      this.SupportedCultures.Add(Localize.Strings.Culture.CULTURE_RU_RU);
    }

    private Localize.Strings.Culture LoadCultureReader(string cultureName) {
      if (cultureName.StartsWith("strings_")) {
        cultureName = cultureName.Substring(cultureName.IndexOf("_", StringComparison.InvariantCulture) + 1);
        try {
          Localize.Strings.Culture culture = Strings.CultureFromString(cultureName);
          if (!this.SupportedCultures.Contains(culture))
            this.SupportedCultures.Add(culture);
          this.LoadCultureAndThen(culture, (Action<T>)null);
          return culture;
        } catch (Exception) {
        }
      }
      return Localize.Strings.Culture.UNDEFINED;
    }

    private void LoadCultureAndThen(Localize.Strings.Culture culture, Action<T> andThen = null) {
    }

    public string StringForKey(string key) {
      string result;
      if (this.GetTranslationFor(key, out result))
        return result;
      return key;
    }

    public bool GetTranslationFor(string key, out string result) {
      result = (string)null;
      if (key == null)
        return true;
      bool flag = key.StartsWith("_");
      if ((uint)this.CurrentCulture > 0U | flag) {
        result = string.Empty;
        if (this.activeStrings != null && this.activeStrings.TryGetValue(flag ? key : StringsProviderBase<T>.comparer.NormalizeString(key), out result) && (!string.IsNullOrEmpty(result) || string.IsNullOrEmpty(key)))
          return true;
      }
      return false;
    }

    public string StringForKeyFormat(string key, params object[] args) {
      string text = this.StringForKey(key);
      try {
        string format = this.EscapeNonStringFormatCurlies(text);
        return key == null || format == null ? "" : string.Format(format, args);
      } catch (FormatException) {
        return "";
      }
    }

    private string EscapeNonStringFormatCurlies(string text) {
      Regex regex = new Regex("\\{([^\\d]+)\\}");
      if (text != null)
        return regex.Replace(text, (MatchEvaluator)(match => match.ToString().Replace("{", "{{").Replace("}", "}}")));
      return (string)null;
    }

    public Dictionary<string, string> GetAllTranslations() {
      return this.activeStrings;
    }

    public void FindClosestStringFor(string matchKey, out string closestString, out int closestMatch) {
      closestMatch = 0;
      closestString = "";
      foreach (string key in this.activeStrings.Keys) {
        int num = this.LevenshteinDistance(key, matchKey);
        if (num < closestMatch) {
          closestString = key;
          closestMatch = num;
        }
      }
    }

    private int LevenshteinDistance(string s, string t) {
      int length1 = s.Length;
      int length2 = t.Length;
      int[,] numArray = new int[length1 + 1, length2 + 1];
      if (length1 == 0)
        return length2;
      if (length2 == 0)
        return length1;
      int index1 = 0;
      while (index1 <= length1)
        numArray[index1, 0] = index1++;
      int index2 = 0;
      while (index2 <= length2)
        numArray[0, index2] = index2++;
      for (int index3 = 1; index3 <= length1; ++index3) {
        for (int index4 = 1; index4 <= length2; ++index4) {
          int num = (int)t[index4 - 1] == (int)s[index3 - 1] ? 0 : 1;
          numArray[index3, index4] = Math.Min(Math.Min(numArray[index3 - 1, index4] + 1, numArray[index3, index4 - 1] + 1), numArray[index3 - 1, index4 - 1] + num);
        }
      }
      return numArray[length1, length2];
    }
  }
}