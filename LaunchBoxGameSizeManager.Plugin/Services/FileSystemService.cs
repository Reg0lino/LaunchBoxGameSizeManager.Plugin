using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic; // For HashSet in CUE parsing

namespace LaunchBoxGameSizeManager.Services
{
    public class FileSystemService
    {
        public long CalculateDirectorySize(string path) // Renamed for clarity, handles both files and dirs
        {
            if (string.IsNullOrEmpty(path))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[FileSystemService] CalculatePathSize: Path is null or empty.");
#endif
                return -3; // Specific code for No Path Found
            }

            if (File.Exists(path))
            {
                return GetFileSize(path);
            }

            if (Directory.Exists(path))
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    // Sum size of all files in the current directory and all subdirectories
                    long size = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => {
                        try { return file.Length; }
                        catch (FileNotFoundException) { return 0; } // File might be gone between Enumerate and Length
                        catch (UnauthorizedAccessException) { return 0; } // Can't access file
                    });
                    return size;
                }
                catch (UnauthorizedAccessException ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[FileSystemService] CalculatePathSize (Dir): Access denied to '{path}'. Details: {ex.Message}");
#endif
                    return -2; // Error Calculating
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[FileSystemService] CalculatePathSize (Dir): Error for '{path}'. Details: {ex.Message}");
#endif
                    return -2; // Error Calculating
                }
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[FileSystemService] CalculatePathSize: Path not found '{path}'.");
#endif
            return -3; // No Path Found / Invalid Path
        }

        public long GetFileSize(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[FileSystemService] GetFileSize: File path is null or empty.");
#endif
                return -3;
            }
            if (!File.Exists(filePath))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[FileSystemService] GetFileSize: File not found '{filePath}'.");
#endif
                return -3;
            }

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[FileSystemService] GetFileSize: Error for file '{filePath}'. Details: {ex.Message}");
#endif
                return -2;
            }
        }

        public long CalculateCueSheetAndRelatedFilesSize(string cueFilePath)
        {
            if (string.IsNullOrEmpty(cueFilePath) || !File.Exists(cueFilePath))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[FileSystemService] CalculateCueSheet: CUE file not found or path null/empty '{cueFilePath}'.");
#endif
                return -3; // No Path
            }

            long totalSize = 0;
            HashSet<string> processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // To avoid double-counting

            try
            {
                FileInfo cueInfo = new FileInfo(cueFilePath);
                totalSize += cueInfo.Length;
                processedFiles.Add(cueInfo.FullName); // Add .cue file itself

                string cueDirectory = Path.GetDirectoryName(cueFilePath);
                if (cueDirectory == null) return -2; // Should not happen if File.Exists passed

                string[] lines = File.ReadAllLines(cueFilePath);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    // Look for lines like: FILE "track01.bin" BINARY
                    if (trimmedLine.StartsWith("FILE", StringComparison.OrdinalIgnoreCase))
                    {
                        int firstQuote = trimmedLine.IndexOf('"');
                        if (firstQuote >= 0)
                        {
                            int secondQuote = trimmedLine.IndexOf('"', firstQuote + 1);
                            if (secondQuote > firstQuote)
                            {
                                string referencedFileName = trimmedLine.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                                string referencedFilePath = Path.Combine(cueDirectory, referencedFileName);

                                // Normalize to full path to ensure uniqueness in HashSet
                                string fullReferencedPath = Path.GetFullPath(referencedFilePath);

                                if (File.Exists(fullReferencedPath) && !processedFiles.Contains(fullReferencedPath))
                                {
                                    totalSize += new FileInfo(fullReferencedPath).Length;
                                    processedFiles.Add(fullReferencedPath);
                                }
                                else if (!File.Exists(fullReferencedPath))
                                {
#if DEBUG
                                    System.Diagnostics.Debug.WriteLine($"[FileSystemService] CalculateCueSheet: Referenced file not found: {referencedFilePath}");
#endif
                                    // Optionally, consider this an error for the whole CUE sheet calculation.
                                    // For now, we sum what we can find.
                                }
                            }
                        }
                    }
                }
                return totalSize;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[FileSystemService] CalculateCueSheet: Error processing CUE '{cueFilePath}': {ex.Message}");
#endif
                return -2; // Error Calculating
            }
        }


        public bool PathExists(string path) // Unchanged
        {
            if (string.IsNullOrEmpty(path)) return false;
            return File.Exists(path) || Directory.Exists(path);
        }

        public bool IsDirectory(string path) // Unchanged
        {
            if (!PathExists(path)) return false;
            FileAttributes attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public void OpenInExplorer(string path) // Unchanged
        {
            if (PathExists(path))
            {
                try { Process.Start("explorer.exe", path); }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[FileSystemService] Error opening path in explorer: {path}. Details: {ex.Message}");
#endif
                }
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[FileSystemService] Cannot open path in explorer, it does not exist: {path}");
#endif
            }
        }

        public bool DeleteFileOrDirectory(string path, bool sendToRecycleBin = true) // Unchanged
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[FileSystemService] Attempting to delete (not really): {path}");
#endif
            return true;
        }
    }
}