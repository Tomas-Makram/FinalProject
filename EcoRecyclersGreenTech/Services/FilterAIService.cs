namespace EcoRecyclersGreenTech.Services
{
    public interface IFilterAIService 
    {

        string Combine(params string?[] parts);
        string Normalize(string? text);
        List<string> Tokenize(string? text);
        bool ContainsAnyToken(string? text, List<string> tokens);
        double Similarity(string? a, string? b);
    }

    public class FilterAIService : IFilterAIService
    {

        private readonly HashSet<string> Stop = new(StringComparer.OrdinalIgnoreCase)
        {
            "and","or","the","a","an","to","in","on","for","with","of","this","that",
            "في","من","على","الى","إلى","و","او","أو","عن","مع","ال","هذا","هذه","ذلك","تلك"
        };

        public string Combine(params string?[] parts)
            => string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));

        public string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var s = text.Trim().ToLowerInvariant();

            // unify Arabic chars
            s = s.Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا');
            s = s.Replace('ى', 'ي').Replace('ة', 'ه');

            // remove tatweel
            s = s.Replace("ـ", "");

            // remove Arabic diacritics
            var diacritics = new[] { "َ", "ً", "ُ", "ٌ", "ِ", "ٍ", "ْ", "ّ" };
            foreach (var d in diacritics) s = s.Replace(d, "");

            return s;
        }

        public List<string> Tokenize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

            var normalized = Normalize(text);

            var cleaned = new string(normalized
                .Select(ch => char.IsLetterOrDigit(ch) || ch == ' ' ? ch : ' ')
                .ToArray());

            return cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length >= 2)
                .Where(t => !Stop.Contains(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public bool ContainsAnyToken(string? text, List<string> tokens)
        {
            if (string.IsNullOrWhiteSpace(text) || tokens.Count == 0) return false;
            var lower = Normalize(text);
            return tokens.Any(t => lower.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        public double Similarity(string? a, string? b)
        {
            var ta = Tokenize(a);
            var tb = Tokenize(b);
            if (ta.Count == 0 || tb.Count == 0) return 0;

            var va = ta.GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var vb = tb.GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            double dot = 0;
            foreach (var kv in va)
                if (vb.TryGetValue(kv.Key, out var c2))
                    dot += kv.Value * c2;

            double na = Math.Sqrt(va.Values.Sum(v => v * v));
            double nb = Math.Sqrt(vb.Values.Sum(v => v * v));
            if (na == 0 || nb == 0) return 0;

            return dot / (na * nb);
        }
    }
}
