namespace CSVtoSPW.Core.Models
{
    public class SpecItem
    {
        public string RawPosition { get; set; }
        public string Name { get; set; }
        public string Count { get; set; }
        public string Commentary { get; set; }
        public string FormattedPosition { get; private set; }
        public string GroupPrefix { get; }
        public string GroupName { get; set; }
        public string LineBreaks { get; private set; }
        public int[] PositionNumbers { get; }

        public SpecItem(string position, string name, string count, string commentary)
        {
            this.RawPosition = position?.Trim() ?? throw new ArgumentNullException(nameof(position));
            this.Name = name;
            this.Count = count;
            this.Commentary = commentary;

            this.GroupPrefix = DetectGroupPrefix();
            this.PositionNumbers = ParseNumbers();
        }

        private string DetectGroupPrefix()
        {
            int i = 0;
            while (i < RawPosition.Length && !char.IsDigit(RawPosition[i])) 
                i++;
            return RawPosition[..i];
        }

        private int[] ParseNumbers()
        {
            var numbers = new List<int>();
            foreach (var part in RawPosition.Split(','))
            {
                string numStr = part[GroupPrefix.Length..];
                if (int.TryParse(numStr, out int num))
                    numbers.Add(num);
            }
            return numbers.ToArray();
        }

        public void FormatPosition()
        {
            if (PositionNumbers.Length == 0)
            {
                FormattedPosition = "";
                return;
            }

            var result = new StringBuilder();
            int start = PositionNumbers[0];
            int prev = start;

            for (int i = 1; i <= PositionNumbers.Length; i++)
            {
                bool isEnd = i == PositionNumbers.Length;
                bool isSequence = !isEnd && (PositionNumbers[i] == prev + 1);

                if (!isSequence)
                {
                    if (result.Length > 0)
                        result.Append(", ");

                    if (start == prev)
                        result.Append($"{GroupPrefix}{start}");
                    else
                        result.Append($"{GroupPrefix}{start}-{GroupPrefix}{prev}");

                    if (!isEnd)
                        start = PositionNumbers[i];
                }
                prev = isEnd ? prev : PositionNumbers[i];
            }

            FormattedPosition = result.ToString();
        }

        public void AddLineBreaks(int maxLineLength)
        {
            if (string.IsNullOrEmpty(FormattedPosition))
                return;

            var parts = FormattedPosition.Split(',');
            var newText = new StringBuilder();
            int currentLength = 0;

            foreach (var part in parts)
            {
                if (currentLength + part.Length > maxLineLength)
                {
                    newText.Append(",\n");
                    LineBreaks += "\n";
                    currentLength = 0;
                }
                else if (newText.Length > 0)
                {
                    newText.Append(", ");
                    currentLength += 2;
                }

                newText.Append(part);
                currentLength += part.Length;
            }

            FormattedPosition = newText.ToString();
        }
    }
}