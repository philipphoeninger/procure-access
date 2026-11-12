// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text;

namespace ProcureAccess.Tests.Shared.Export;

/// <summary>
/// Reads an export CSV back (SPEC §8.3, EXP-04) and refuses anything else: the file must start with
/// the UTF-8 BOM and be valid UTF-8; records are separated by semicolons and end with CRLF; a field
/// is quoted per RFC 4180 when it holds a semicolon, quote or line break. A bare line break or a
/// quote outside a quoted field fails, so a test that reads the file also checks its form.
/// </summary>
internal static class CsvReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static IReadOnlyList<IReadOnlyList<string>> Read(byte[] file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Length < 3 || file[0] != 0xEF || file[1] != 0xBB || file[2] != 0xBF)
        {
            throw new FormatException("The CSV does not start with the UTF-8 BOM.");
        }

        return Parse(StrictUtf8.GetString(file, 3, file.Length - 3));
    }

    private static List<IReadOnlyList<string>> Parse(string text)
    {
        var records = new List<IReadOnlyList<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var i = 0;
        while (i < text.Length)
        {
            if (text[i] == '"' && field.Length == 0)
            {
                i = ReadQuoted(text, i + 1, field);
                if (i < text.Length && text[i] is not (';' or '\r'))
                {
                    throw new FormatException($"Text after a quoted field at {i}.");
                }

                continue;
            }

            switch (text[i])
            {
                case ';':
                    record.Add(field.ToString());
                    field.Clear();
                    i++;
                    break;
                case '\r' when i + 1 < text.Length && text[i + 1] == '\n':
                    record.Add(field.ToString());
                    field.Clear();
                    records.Add(record);
                    record = [];
                    i += 2;
                    break;
                case '\r' or '\n' or '"':
                    throw new FormatException($"Unquoted {(text[i] == '"' ? "quote" : "line break")} at {i}.");
                default:
                    field.Append(text[i]);
                    i++;
                    break;
            }
        }

        if (record.Count > 0 || field.Length > 0)
        {
            throw new FormatException("The last record does not end with CRLF.");
        }

        return records;
    }

    // Reads up to the closing quote; a doubled quote is one quote. Returns the index after it.
    private static int ReadQuoted(string text, int i, StringBuilder field)
    {
        while (i < text.Length)
        {
            if (text[i] == '"')
            {
                if (i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i += 2;
                    continue;
                }

                return i + 1;
            }

            field.Append(text[i]);
            i++;
        }

        throw new FormatException("A quoted field does not end.");
    }
}
