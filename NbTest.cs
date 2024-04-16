namespace NbCore;

public static class NbTest
{
    // Utils for converting strings with numbers into arrays with ints (for use in testing)

    /// <summary>
    /// Converts a set of strings to the 2D array with each string representing a row and each symbol being converted from a char to an int  
    /// </summary>
    /// <param name="decs">A set of strings with digits</param>
    /// <returns>2D array with ints</returns>
    /// <exception cref="ArgumentException"></exception>
    public static int[,] Dec2DArr(params string[] decs)
    {
        if (decs == null || decs.Length == 0)
            throw new ArgumentException("Empty Array", nameof(decs));

        int cols = decs[0].Length;
        int[,] a = new int[decs.Length, cols];
        for (int r = 0; r < decs.Length; r++)
        {
            string curRow = decs[r];
            if (curRow.Length != cols)
                throw new ArgumentException($"Given data is not rectangular. Line {r} has {decs[r].Length} symbols, while first line had {cols} symbols", nameof(decs));

            for (int c = 0; c < cols; ++c)
            {
                char ch = curRow[c];
                if (!Char.IsDigit(ch))
                    throw new ArgumentException($"Character '{ch}' in string '{curRow}' is not a digit");

                a[r, c] = ch - 0x30;
            }
        }
        return a;
    }

    /// <summary>
    /// Converts a set of strings to a jagged array with each string representing a row and each symbol being converted from a char to an int  
    /// </summary>
    /// <param name="decs">A set of strings with digits</param>
    /// <returns>Jagged array with ints</returns>
    /// <exception cref="ArgumentException"></exception>
    public static int[][] Dec2ArrJag(params string[] decs)
    {
        if (decs == null || decs.Length == 0)
            throw new ArgumentException("Empty Array", nameof(decs));

        int cols = decs[0].Length;
        int[][] a = new int[decs.Length][];
        for (int r = 0; r < decs.Length; r++)
        {
            string curRow = decs[r];
            int[] rowArr = new int[cols];
            if (curRow.Length != cols)
                throw new ArgumentException($"Given data is not rectangular. Line {r} has {decs[r].Length} symbols, while first line had {cols} symbols", nameof(decs));

            for (int c = 0; c < cols; ++c)
            {
                char ch = curRow[c];
                if (!Char.IsDigit(ch))
                    throw new ArgumentException($"Character '{ch}' in string '{curRow}' is not a digit");

                rowArr[c] = curRow[c] - 0x30;
            }
            a[r] = rowArr;
        }

        return a;
    }


    public static int[] D2A(string dec)
    {
        int[] a = new int[dec.Length];
        for (int i = a.Length - 1; i >= 0; i--)
        {
            a[i] = dec[i] - 0x30;
        }
        return a;
    }
}

public sealed class LinesCollector : IDisposable
{
    private class FileDesc
    {
        internal FileDesc(string fullPath)
        {
            FullPath = fullPath;
            lines = new List<string>(500);
        }

        internal readonly string FullPath;
        internal readonly List<string> lines;
    }

    public readonly string Dir;
    private readonly Dictionary<string, FileDesc> fFiles;
    private readonly Dictionary<string, string>? HeadersN;
    private readonly string? DefaultHeaderN;
    private readonly string? DefaultFooterN;
    public readonly string ReportPrefix;

    public LinesCollector(string dir, string reportPrefix, Dictionary<string, string>? headersN, string? defaultHeaderN, string? defaultFooterN = null)
    {
        Dir = dir;
        //NbDir.CreateDirRecursive(dirName);

        fFiles = new Dictionary<string, FileDesc>(10);
        HeadersN = headersN;
        DefaultHeaderN = defaultHeaderN;
        DefaultFooterN = defaultFooterN;
        ReportPrefix = reportPrefix;
    }

    public void Write(string subSystem, string line, string? fileName = null)
    {
        if (!fFiles.TryGetValue(subSystem, out FileDesc? fd))
        {
            fd = new FileDesc(Path.Combine(Dir, fileName ?? (subSystem + ".csv")));
            fFiles.Add(subSystem, fd);
        }
        fd.lines.Add(line);
    }

    public void Dispose()
    {
        foreach (var pair in fFiles)
        {
            var list = pair.Value.lines;
            if (list.Count == 0)
                continue;

            try
            {
                list.Sort();
                if (HeadersN?.TryGetValue(pair.Key, out string? hdr) ?? false)
                    list.Insert(0, hdr);
                else if (!String.IsNullOrEmpty(DefaultHeaderN))
                    list.Insert(0, DefaultHeaderN);

                if (!String.IsNullOrEmpty(DefaultFooterN))
                    list.Add(DefaultFooterN);

                File.WriteAllLines(pair.Value.FullPath, list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving file {pair.Value.FullPath}:\r\n" + NbException.Exception2String(ex));
            }
        }
    }
}