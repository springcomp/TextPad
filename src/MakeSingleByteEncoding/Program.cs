using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MakeSingleByteEncoding
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Wrong argument count. Please, specify a code page number.");
                Environment.Exit(1);
            }

            var codePage = 0;
            if (!Int32.TryParse(args[0], out codePage))
            {
                Console.WriteLine("Unable to convert {0} to an integer.", args[0]);
                Environment.Exit(1);
            }

            var encoding = Encoding.GetEncoding(codePage);
            var encodingName = encoding.WebName.Replace("-", "");
            var path = String.Format(@"C:\Temp\{0}Encoding.cs", encodingName);

            var dictionary = new Dictionary<int, byte>();

            for (int b = 0; b <= 255; b++)
            {
                var s = encoding.GetString(new byte[] { (byte) b, });
                var character = s[0];
                var codePoint = (int)character;

                dictionary.Add(codePoint, (byte) b);
            }

            // produce output file

            using (var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8))
            {
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Text;");
                writer.WriteLine("");
                writer.WriteLine("namespace TextPad.Encodings");
                writer.WriteLine("{");
                writer.WriteLine("    public sealed class {0}Encoding : CodePageEncoding", encodingName);
                writer.WriteLine("    {");
                writer.WriteLine("        public {0}Encoding(EncoderFallback encoderFallback, DecoderFallback decoderFallback)", encodingName);
                writer.WriteLine("            : base({0}, \"{1}\", \"{2}\", encoderFallback, decoderFallback)", encoding.CodePage, encodingName, encoding.WebName);
                writer.WriteLine("        {");
                writer.WriteLine("            byteToChars_ = new char[]");
                writer.WriteLine("            {");

                var s_0 = encoding.GetString(new byte[] { (byte)0, });
                var character_0 = s_0[0];
                writer.WriteLine("                          '\\u{0:x4}' // '{1}'", (int)character_0, character_0);

                    for (int b = 1; b <= 255; b++)
                {
                    var s = encoding.GetString(new byte[] { (byte)b, });
                    var character = s[0];
                    var codePoint = (int)character;
                    var c = character.ToString()
                        .Replace("\r", "<CR>")
                        .Replace("\n", "<LF>")
                        .Replace("\t", "<TAB>")
                        ;

                    writer.WriteLine("                        , '\\u{0:x4}' // '{1}'", (int)character, c);
                }

                writer.WriteLine("                };");
                writer.WriteLine("");
                writer.WriteLine("            charToBytes_ = new Dictionary<char, byte>()");
                writer.WriteLine("            {");

                foreach (var codePoint in dictionary.Keys)
                    if (codePoint != (int)dictionary[codePoint])
                        writer.WriteLine("                        {{ /* '{0}' */ '\\u{1:x4}', {2} }},"
                            , (char)codePoint
                            , codePoint
                            , dictionary[codePoint]
                            );

                writer.WriteLine("            };");
                writer.WriteLine("        }");
                writer.WriteLine("");
                writer.WriteLine("        public static {0}Encoding Create(EncoderFallback encoder, DecoderFallback decoder)", encodingName);
                writer.WriteLine("        {");
                writer.WriteLine("            return new {0}Encoding(encoder, decoder);", encodingName);
                writer.WriteLine("        }");
                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.WriteLine("");
            }

            using (var writer = new StreamWriter(File.Open(@"C:\Temp\1252_c.txt", FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.Unicode))
            {
            }
        }
    }
}

