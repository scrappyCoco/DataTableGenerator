using System;
using System.IO;

namespace Coding4fun.DataTools.Analyzers
{
    public static class TableBuilderFormatter
    {
        public static string Format(string code, string prefix, string suffix, string whitespace)
        {
            using StringWriter stringWriter = new StringWriter();
            using (StringReader stringReader = new StringReader(code))
            {
                string line;
                int lineNumber = 0;
                int openedBraceCount = 0;
                
                while ((line = stringReader.ReadLine()) != null)
                {
                    ++lineNumber;
                    if (lineNumber == 1)
                    {
                        ++openedBraceCount;
                        stringWriter.Write(prefix);
                        stringWriter.WriteLine(line);
                        continue;
                    }
                    
                    stringWriter.Write(whitespace);
                    stringWriter.Write(new string(' ', openedBraceCount * 4));
                    stringWriter.WriteLine(line);
                    
                    foreach (char ch in line)
                    {
                        if (ch == '(') ++openedBraceCount;
                        else if (ch == ')') --openedBraceCount;
                    }
                }
                stringWriter.Write(suffix);
            }

            return stringWriter.ToString();
        }
    }
}