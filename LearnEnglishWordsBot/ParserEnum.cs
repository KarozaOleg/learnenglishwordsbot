using System;

namespace LearnEnglishWordsBot
{
    public static class ParserEnum
    {
        public static bool Parse<T>(string value, out T parsed)
        {
            parsed = default;

            value = value.ToUpper();
            foreach (T temp in Enum.GetValues(typeof(T)))
            {
                if (value != temp.ToString().ToUpper())
                    continue;

                parsed = temp;
                return true;
            }
            return false;
        }
    }
}
