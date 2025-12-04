using System.Globalization;

namespace lab1_oop_malinovska_sofiia.Parser {
        public class ExpressionParser
        {
            private readonly string _s;
            private readonly Func<string, double> _resolver;
            private int _i;

            public ExpressionParser(string s, Func<string, double> resolver)
            {
                _s = s ?? "0";
                _resolver = resolver;
            }

            public double Parse() => ParseAdd();

            private double ParseAdd()
            {
                double x = ParseMul();
                while (_i < _s.Length)
                {
                    SkipWs();
                    if (Match('+')) x += ParseMul();
                    else if (Match('-')) x -= ParseMul();
                    else break;
                }
        
                return x;
            }

            private double ParseMul()
            {
                double x = ParsePow();
                while (_i < _s.Length)
                {
                    SkipWs();
                    if (Match('*')) x *= ParsePow();
                    else if (Match('/')) x /= ParsePow();
                    else if (MatchWord("mod")) x %= ParsePow();
                    else if (MatchWord("div")) x = Math.Floor(x / ParsePow());
                    else break;
                }

                return x;
            }

            private double ParsePow()
            {
                double x = ParseValue();
                while (_i < _s.Length)
                {
                    SkipWs();
                    if (Match('^')) x = Math.Pow(x, ParseValue());
                    else break;
                }

                return x;
            }

            private double ParseValue()
            {
                SkipWs();

                if (MatchWord("inc"))
                {
                    Expect('(');
                    double v = Parse();
                    Expect(')');
                    return v + 1;
                }

                if (MatchWord("dec"))
                {
                    Expect('(');
                    double v = Parse();
                    Expect(')');
                    return v - 1;
                }

                if (Match('('))
                {
                    double v = Parse();
                    Expect(')');
                    return v;
                }

                if (_i < _s.Length && char.IsLetter(_s[_i]))
                {
                    int start = _i;
                    while (_i < _s.Length && char.IsLetterOrDigit(_s[_i])) _i++;
                    string name = _s[start.._i];
                    return _resolver(name);
                }

                int numStart = _i;
                while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                if (numStart == _i)
                    throw new Exception("Очікувалось число або ідентифікатор");

                string numStr = _s[numStart.._i];
                if (!double.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out double val))
                    throw new Exception($"Невірне число: {numStr}");

                return val;
            }

            private void SkipWs()
            {
                while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
            }

            private bool Match(char ch)
            {
                SkipWs();
                if (_i < _s.Length && _s[_i] == ch)
                {
                    _i++;
                    return true;
                }
                return false;
            }

            private bool MatchWord(string w)
            {
                SkipWs();
                int len = w.Length;
                if (_i + len > _s.Length) return false;
                if (!string.Equals(_s.Substring(_i, len), w, StringComparison.OrdinalIgnoreCase))
                    return false;
                _i += len;
                return true;
            }

            private void Expect(char ch)
            {
                if (!Match(ch))
                    throw new Exception($"Очікувався символ '{ch}'");
            }
        }
}