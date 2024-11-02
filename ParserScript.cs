using System.Linq;
using System.Text.RegularExpressions;

namespace ParserScript
{
    internal static partial class Converter
    {
        public static string Replace(this string s) => string.Join("", Replace().Replace(string.Join("", s[(int.TryParse("0", out int i) ? 0 : 0)..]), "$1").Aggregate(Array.Empty<string>(), (x, y) => [.. x.Append(s[i].ToString() is "\\" ? s[i++].ToString() + s[i++].ToString() : s[i++].ToString())]).Select(x => x is "\\n" ? "\n" : (x is "\\t" ? "\t" : (x is "\\s" ? " " : (x is "\\\\" ? "\\" : (x is "\\\"" ? "\"" : x))))));
        public static string[] Split2(this string s, string split) => [.. Regex.Split(s.EndsWith(split) ? s : s + split, @$"("".*""{split}|[^{split[0]}]*{split})").Where(x => x != "").Select(x => string.Join("", x.SkipLast(split.Length)))];

        [GeneratedRegex(@"(\\).")]
        private static partial Regex Replace();
    }
    internal partial class ParserScript
    {
        private readonly static List<string> MNM = ["match?", "varmatch?", "listmatch?", "nomatch?", "varnomatch?", "listnomatch?", "isstart?", "isnostart?"];
        private static void Jump(ref int i, string[] codes)
        {
            int ind = i;
            int count = 1;
            while (!codes[ind].Equals("end", StringComparison.CurrentCultureIgnoreCase) || count != 0)
            {
                ind++;
                if (MNM.Any(x => codes[ind].StartsWith(x, StringComparison.CurrentCultureIgnoreCase))) count++;
                else if (codes[ind].ToLower() is "end") count--;
            }
            i = ind;
        }
        public static (string Result, bool IsMatch, string Process) Run(string path, string pparse)
        {
            try
            {
                pparse = pparse.Replace();
                string[] codes = [.. Replace2().Replace(File.ReadAllText(path), "\n").Split("\n").Select(x => x.Replace("\r", "")).Select(x => string.Join("", x.SkipWhile(y => y is ' '))).Where(x => x != "")];
                List<(string Value, bool IsMatch, string Parse)> memory = [];
                memory.Add(("", true, pparse));
                Stack<int> ReturnRow = new();
                Stack<int> ToRow = new();
                Stack<string> CodeMemo = new();
                List<string> Escape = [];
                Dictionary<string, string> SVar = [];
                Dictionary<string, bool> BVar = [];
                Dictionary<string, string[]> SList = [];
                Dictionary<string, bool[]> BList = [];
                string Process = "";
                int ProcessCounter = 1;
                string MatchStr = "";
                for (int i = 0; i < codes.Length; i++)
                {
                    string[] codes2 = [.. codes[i].Split2(" ").Where(x => x != "")];
                    for (int j = 0; j < codes2.Length; j++)
                    {
                        string code = codes2[j].ToLower();
                        switch (code)
                        {
                            case "escape":
                                j++;
                                if (!IsMatch1().IsMatch(codes2[j])) break;
                                Escape.Add(string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace());
                                break;
                            case "noescape":
                                j++;
                                if (!IsMatch1().IsMatch(codes2[j])) break;
                                string c = string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace();
                                Escape.Remove(c);
                                break;
                            case "record":
                                memory.Add((memory[^1].Value, memory[^1].IsMatch, memory[^1].Parse));
                                break;
                            case "vrecord":
                                j++;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                memory.Add((string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)), memory[^1].IsMatch, memory[^1].Parse));
                                break;
                            case "match":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                string s = string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace();
                                if (memory[^1].Parse.StartsWith(s))
                                {
                                    MatchStr = s;
                                    memory[^1] = (memory[^1].Value + s, memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(s.Length)).Replace());
                                }
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "varmatch":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                s = SVar[codes2[j]];
                                if (memory[^1].Parse.StartsWith(s)) memory[^1] = (memory[^1].Value + s, memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(s.Length)).Replace());
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "nomatch?":
                                if (memory[^1].IsMatch) Jump(ref i, codes);
                                break;
                            case "varnomatch?":
                                if (!BVar[codes2[++j]]) Jump(ref i, codes);
                                break;
                            case "listnomatch?":
                                if (!BList[codes2[++j]][^(int.Parse(codes2[++j]))]) Jump(ref i, codes);
                                break;
                            case "match?":
                                if (memory[^1].IsMatch) Jump(ref i, codes);
                                break;
                            case "varmatch?":
                                if (BVar[codes2[++j]]) Jump(ref i, codes);
                                break;
                            case "listmatch?":
                                if (BList[codes2[++j]][^(int.Parse(codes2[++j]))]) Jump(ref i, codes);
                                break;
                            case "break":
                                memory.RemoveAt(memory.Count - 1);
                                break;
                            case "varbreak":
                                switch (codes2[++j])
                                {
                                    case "s":
                                        SVar.Remove(codes2[++j]);
                                        break;
                                    case "b":
                                        BVar.Remove(codes2[++j]);
                                        break;
                                }
                                break;
                            case "listbreak":
                                switch (codes2[++j])
                                {
                                    case "s":
                                        SList.Remove(codes2[++j]);
                                        break;
                                    case "b":
                                        BList.Remove(codes2[++j]);
                                        break;
                                }
                                break;
                            case "call":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                ReturnRow.Push(i);
                                i = codes.Select((x, i) => Regex.IsMatch(x.ToLower(), $"(ast\\s+{codes2[j]})") ? i : -1).First(x => x != -1);
                                break;
                            case "number":
                                if (!memory[^1].IsMatch) break;
                                if (Number().Match(memory[^1].Parse).Index is 0) memory[^1] = (memory[^1].Value + Number().Match(memory[^1].Parse), memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(Number().Match(memory[^1].Parse).Value.Length)));
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "sq":
                                if (!memory[^1].IsMatch) break;
                                if (memory[^1].Parse.StartsWith('\''))
                                {
                                    MatchStr = "'";
                                    memory[^1] = (memory[^1].Value + '\'', memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(1)));
                                }
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "dq":
                                if (!memory[^1].IsMatch) break;
                                if (memory[^1].Parse.StartsWith('"'))
                                {
                                    MatchStr = '"'.ToString();
                                    memory[^1] = (memory[^1].Value + '"', memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(1)));
                                }
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "char":
                                if (!memory[^1].IsMatch) break;
                                if (Char().Match(memory[^1].Parse).Index is 0)
                                {
                                    MatchStr = Char().Match(memory[^1].Parse).Value;
                                    memory[^1] = (memory[^1].Value + Char().Match(memory[^1].Parse).Value, memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(1)));
                                }
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "string":
                                if (!memory[^1].IsMatch) break;
                                if (String().IsMatch(memory[^1].Parse))
                                {
                                    MatchStr = String2().Match(memory[^1].Parse).Value;
                                    memory[^1] = (memory[^1].Value + String2().Match(memory[^1].Parse).Value, memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(String2().Match(memory[^1].Parse).Value.Length)));
                                }
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "nochars":
                                if (!memory[^1].IsMatch) break;
                                if (!memory[^1].Parse.Any(x => codes2[++j].Contains(x)))
                                {
                                    MatchStr = string.Join("", memory[^1].Parse.TakeWhile(x => !codes2[j].Contains(x)));
                                    memory[^1] = (memory[^1].Value + string.Join("", memory[^1].Parse.TakeWhile(x => !codes2[j].Contains(x))), memory[^1].IsMatch, string.Join("", memory[^1].Parse.SkipWhile(x => !codes2[j].Contains(x))));
                                }
                                else memory[^1] = (memory[^1].Value, false, "");
                                break;
                            case "ast":
                                if (true)
                                {
                                    int count = 1;
                                    while (!codes[i].Equals("last", StringComparison.CurrentCultureIgnoreCase) || count != 0)
                                    {
                                        i++;
                                        if (codes[i].StartsWith("ast", StringComparison.CurrentCultureIgnoreCase)) count++;
                                        else if (codes[i].ToLower() is "last") count--;
                                    }
                                }
                                break;
                            case "last":
                                if (ReturnRow.Count != 0) i = ReturnRow.Pop();
                                break;
                            case "push":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                s = string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace();
                                memory[^1] = (memory[^1].Value + s, memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(s.Length)));
                                break;
                            case "vpush":
                                if (!memory[^1].IsMatch) break;
                                if (!IsMatch2().IsMatch(codes2[j + 2])) break;
                                s = string.Join("", codes2[j + 2].Take(codes2[j + 2].Length - 1).Skip(1)).Replace();
                                memory[^(int.Parse(codes2[j + 1]))] = (memory[^(int.Parse(codes2[j + 1]))].Value + s, memory[^(int.Parse(codes2[j + 1]))].IsMatch, string.Join("", memory[^1].Parse.Skip(s.Length)));
                                j = codes2.Length;
                                break;
                            case "varpush":
                                if (!memory[^1].IsMatch) break;
                                memory[^1] = (memory[^1].Value + SVar[codes2[++j]], memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(SVar[codes2[j]].Length)));
                                break;
                            case "varvpush":
                                if (!memory[^1].IsMatch) break;
                                s = SVar[codes2[j + 2]];
                                memory[^(int.Parse(codes2[j + 1]))] = (memory[^(int.Parse(codes2[j + 1]))].Value + s, memory[^(int.Parse(codes2[j + 1]))].IsMatch, string.Join("", memory[^1].Parse.Skip(s.Length)));
                                j = codes2.Length;
                                break;
                            case "listpush":
                                if (!memory[^1].IsMatch) break;
                                memory[^1] = (memory[^1].Value + SList[codes2[++j]][^(int.Parse(codes2[++j]))], memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(SList[codes2[j - 1]][^(int.Parse(codes2[j]))].Length)));
                                break;
                            case "listvpush":
                                if (!memory[^1].IsMatch) break;
                                j++;
                                memory[^(int.Parse(codes2[j]))] = (memory[^(int.Parse(codes2[j]))].Value + SList[codes2[j + 1]][^(int.Parse(codes2[j + 2]))], memory[^(int.Parse(codes2[j]))].IsMatch, string.Join("", memory[^1].Parse.Skip(SList[codes2[j + 1]][^(int.Parse(codes2[j + 2]))].Length)));
                                j = codes2.Length;
                                break;
                            case "downpush":
                                if (!memory[^1].IsMatch) break;
                                memory[^2] = (memory[^2].Value + memory[^1].Value, memory[^1].IsMatch, memory[^1].Parse);
                                memory.RemoveAt(memory.Count - 1);
                                break;
                            case "set":
                                ToRow.Push(i - 1);
                                break;
                            case "jump":
                                i = ToRow.Pop();
                                break;
                            case "down":
                                j++;
                                memory[^2] = (memory[^1].Value, codes2[j] is "now" ? memory[^1].IsMatch : (codes2[j] is "true"), memory[^1].Parse);
                                memory.RemoveAt(memory.Count - 1);
                                break;
                            case string str when str.StartsWith('#'):
                                j = codes2.Length;
                                break;
                            case string str when str.StartsWith("#="):
                                while (!codes[i].Contains("=#")) i++;
                                Console.WriteLine(i);
                                break;
                            case "memo":
                                CodeMemo.Push(string.Join(" ", codes2.Skip(1)));
                                j = codes2.Length;
                                break;
                            case "pop":
                                j = -1;
                                codes2 = CodeMemo.Pop().Split(' ');
                                break;
                            case "change":
                                if (!(codes2[j + 2] is "true" or "false")) break;
                                memory[^(int.Parse(codes2[++j]))] = (memory[^(int.Parse(codes2[j]))].Value, codes2[++j] is "true", memory[^1].Parse);
                                break;
                            case "clear":
                                memory.Clear();
                                memory.Add(("", true, pparse));
                                break;
                            case "parseresult":
                                pparse = memory[^1].Value;
                                break;
                            case "vparseresult":
                                pparse = memory[^(int.Parse(codes2[++j]))].Value;
                                break;
                            case "let":
                                char type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!IsMatch2().IsMatch(codes2[j + 1]) && codes2[j + 1] != "now") break;
                                        SVar.Add(codes2[j], codes2[++j] != "now" ? string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace() : memory[^1].Value);
                                        break;
                                    case 'b':
                                        if (codes2[j + 1] != "true" && codes2[j + 1] != "false" && codes2[j + 1] != "now") break;
                                        BVar.Add(codes2[j], codes2[++j] != "now" ? codes2[j] is "true" : memory[^1].IsMatch);
                                        break;
                                }
                                break;
                            case "vlet":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!Number().IsMatch(codes2[j + 1])) break;
                                        SVar.Add(codes2[j], memory[^(int.Parse(codes2[++j]))].Value);
                                        break;
                                    case 'b':
                                        if (!Number().IsMatch(codes2[j + 1])) break;
                                        BVar.Add(codes2[j], memory[^(int.Parse(codes2[++j]))].IsMatch);
                                        break;
                                }
                                break;
                            case "letlist":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                switch (type)
                                {
                                    case 's':
                                        SList.Add(codes2[++j], []);
                                        break;
                                    case 'b':
                                        BList.Add(codes2[++j], []);
                                        break;
                                }
                                break;
                            case "add":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!IsMatch2().IsMatch(codes2[j + 1])) break;
                                        SList[codes2[j]] = [.. SList[codes2[j]].Append(codes2[j + 1].Trim('"'))];
                                        break;
                                    case 'b':
                                        if (!(codes2[j + 1] is "true" or "false")) break;
                                        BList[codes2[j]] = [.. BList[codes2[j]].Append(codes2[j + 1] is "true")];
                                        break;
                                }
                                j++;
                                break;
                            case "remove":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        int ind = int.Parse(codes2[j + 1]);
                                        SList[codes2[j]] = [.. SList[codes2[j]].Reverse().Where((x, i) => i != ind).Reverse()];
                                        break;
                                    case 'b':
                                        ind = int.Parse(codes2[j + 1]);
                                        BList[codes2[j]] = [.. BList[codes2[j]].Reverse().Where((x, i) => i != ind).Reverse()];
                                        break;
                                }
                                j++;
                                break;
                            case "varset":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!IsMatch2().IsMatch(codes2[j + 1]) && codes2[j + 1] != "now") break;
                                        SVar[codes2[j]] = codes2[++j] != "now" ? string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace() : memory[^1].Value;
                                        break;
                                    case 'b':
                                        if (codes2[j + 1] != "true" && codes2[j + 1] != "false" && codes2[j + 1] != "now") break;
                                        BVar[codes2[j]] = codes2[++j] != "now" ? codes2[j] is "true" : memory[^1].IsMatch;
                                        break;
                                }
                                break;
                            case "varvset":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!Number().IsMatch(codes2[j + 1])) break;
                                        SVar[codes2[j]] = memory[^(int.Parse(codes2[++j]))].Value;
                                        break;
                                    case 'b':
                                        if (!Number().IsMatch(codes2[j + 1])) break;
                                        BVar[codes2[j]] = memory[^(int.Parse(codes2[++j]))].IsMatch;
                                        break;
                                }
                                break;
                            case "listset":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!Number().IsMatch(codes2[j + 1])) break;
                                        if (!IsMatch2().IsMatch(codes2[j + 2])) break;
                                        SList[codes2[j]][^(int.Parse(codes2[j + 1]))] = IsMatch2().Match(codes2[j + 2]).Groups[0].Value;
                                        break;
                                    case 'b':
                                        if (!Number().IsMatch(codes2[j + 1])) break;
                                        if (!(codes[j + 2] is "true" or "false")) break;
                                        BList[codes2[j]][^(int.Parse(codes2[j + 1]))] = codes2[j + 2] is "true";
                                        break;
                                }
                                j = codes2.Length;
                                break;
                            case "listvset":
                                type = Convert.ToChar(codes2[++j].ToLower());
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!Number().IsMatch(codes2[j + 1]) || !Number().IsMatch(codes2[j + 2])) break;
                                        SList[codes2[j]][^(int.Parse(codes2[j + 1]))] = memory[^(int.Parse(codes2[j + 2]))].Value;
                                        break;
                                    case 'b':
                                        if (!Number().IsMatch(codes2[j + 1]) || !Number().IsMatch(codes2[j + 2])) break;
                                        BList[codes2[j]][^(int.Parse(codes2[j + 1]))] = memory[^(int.Parse(codes2[j + 2]))].IsMatch;
                                        break;
                                }
                                break;
                            case "csregex":
                                j++;
                                switch (codes2[j].ToLower())
                                {
                                    case "match":
                                        j++;
                                        if (!IsMatch2().IsMatch(codes2[j])) break;
                                        if (Regex.Match(memory[^1].Parse, string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1))).Index is 0)
                                        {
                                            MatchStr = Regex.Match(memory[^1].Parse, string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1))).Value;
                                            memory[^1] = (memory[^1].Value + Regex.Match(memory[^1].Parse, string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1))).Value, memory[^1].IsMatch, string.Join("", memory[^1].Parse.Skip(Regex.Match(memory[^1].Parse, string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1))).Value.Length)));
                                        }
                                        else memory[^1] = ("", false, "");
                                        break;
                                    case "ismatch":
                                        j++;
                                        if (!IsMatch2().IsMatch(codes2[j])) break;
                                        if (!Regex.IsMatch(memory[^1].Parse, string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1))))
                                        {
                                            int count = 1;
                                            while (!codes[i].Equals("end", StringComparison.CurrentCultureIgnoreCase) || count != 0)
                                            {
                                                i++;
                                                if (MNM.Contains(codes[i]) || CSRegexIsMatch().IsMatch(codes[i].ToLower())) count++;
                                                else if (codes[i].ToLower() is "end") count--;
                                            }
                                        }
                                        break;
                                    case "isnomatch":
                                        j++;
                                        if (!IsMatch2().IsMatch(codes2[j])) break;
                                        if (Regex.IsMatch(memory[^1].Parse, string.Join("", codes2[j].Take(codes2[j].Length).Skip(1))))
                                        {
                                            int count = 1;
                                            while (!codes[i].Equals("end", StringComparison.CurrentCultureIgnoreCase) || count != 0)
                                            {
                                                i++;
                                                if (MNM.Contains(codes[i]) || CSRegexIsMatch().IsMatch(codes[i].ToLower())) count++;
                                                else if (codes[i].ToLower() is "end") count--;
                                            }
                                        }
                                        break;
                                }
                                break;
                            case "isstart?":
                                j++;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                if (!memory[^1].Parse.StartsWith(string.Join("", codes2[j].SkipLast(1).Skip(1)).Replace())) Jump(ref i, codes);
                                break;
                            case "isnostart?":
                                j++;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                if (memory[^1].Parse.StartsWith(string.Join("", codes2[j].SkipLast(1).Skip(1)).Replace())) Jump(ref i, codes);
                                break;
                        }
                    }

                    memory[^1] = (memory[^1].Value, memory[^1].IsMatch, string.Join("", memory[^1].Parse.SkipWhile(x => Escape.Contains(x.ToString()))));
                    if (!codes[i].StartsWith('#')) Process += $"ProcessNumber: {ProcessCounter}\nCommand      : {codes[i]}\nMatchStr     : \"{MatchStr}\"\nMemory       : V = {memory[^1].Value} S = {memory[^1].IsMatch} P = \"{memory[^1].Parse}\"\n";
                    MatchStr = "";
                    ProcessCounter++;
                }
                if (!memory[^1].IsMatch || memory[^1].Parse != "" || Process is "") throw new Exception();
                return (memory[^1].Value, memory[^1].IsMatch, string.Join("", Process.SkipLast(1)));
            }
            catch { }
            return ("", false, "");
        }

        [GeneratedRegex(@"(\d+)")]
        private static partial Regex Number();
        [GeneratedRegex(@"([^\s\d\n""'])")]
        private static partial Regex String();
        [GeneratedRegex(@"([^\s\d\n""']+)")]
        private static partial Regex String2();
        [GeneratedRegex(@"(.)")]
        private static partial Regex Char();
        [GeneratedRegex(@"('.')")]
        private static partial Regex IsMatch1();
        [GeneratedRegex(@"(""(\\.|[^""])*"")")]
        public static partial Regex IsMatch2();
        [GeneratedRegex(@"(csregex\s+ismatch.+)")]
        private static partial Regex CSRegexIsMatch();
        [GeneratedRegex(@"\s*;\s*")]
        private static partial Regex Replace2();
    }
}