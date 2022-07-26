namespace SecureHttpClient
{
    /// <summary>
    /// Splits a string with one or more cookies separated with a comma (set-cookie folding).
    /// Since some attribute values can also have commas, like the expires date field, this does a look ahead to detect where it's a separator or not.
    /// Based on: https://github.com/google/j2objc/commit/16820fdbc8f76ca0c33472810ce0cb03d20efe25
    /// </summary>
    internal class SetCookieHeaderSplitter
    {
        private readonly string _s;
        private int _pos;

        public SetCookieHeaderSplitter(string setCookieHeaderString)
        {
            _s = setCookieHeaderString;
            _pos = 0;
        }

        public bool HasNext()
        {
            return _pos < _s.Length;
        }

        public string Next()
        {
            var start = _pos;
            while (SkipWhiteSpace())
            {
                if (_s[_pos] == ',')
                {
                    var lastComma = _pos++;
                    SkipWhiteSpace();
                    var nextStart = _pos;
                    while (_pos < _s.Length && _s[_pos] != '=' && _s[_pos] != ';' && _s[_pos] != ',')
                    {
                        _pos++;
                    }
                    if (_pos < _s.Length && _s[_pos] == '=')
                    {
                        // pos is inside the next cookie, so back up and return it.
                        _pos = nextStart;
                        return _s[start..lastComma];
                    }
                    _pos = lastComma;
                }
                _pos++;
            }
            return _s[start..];
        }

        // Skip whitespace, returning true if there are more chars to read.
        private bool SkipWhiteSpace()
        {
            while (_pos < _s.Length && char.IsWhiteSpace(_s[_pos]))
            {
                _pos++;
            }
            return _pos < _s.Length;
        }
    }
}
