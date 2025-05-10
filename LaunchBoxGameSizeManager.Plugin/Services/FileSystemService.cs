using System;
using System.IO;
using System.Linq;
using System.Diagnostics; // For OpenInExplorer
using Unbroken.LaunchBox.Plugins; // For PluginHelper for logging

namespace LaunchBoxGameSizeManager.Services
{
    public class FileSystemService
    {
        public long CalculateDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                PluginHelper.LogHelper.Log($"Directory not found: {directoryPath}", "FileSystemService", LogLevel.Error);
                return -1; // Or throw an exception
            }

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                // Sum size of all files in the current directory
                long size = dirInfo.EnumerateFiles().Sum(file => file.Length);
                // Recursively sum size of all files in subdirectories
                size += dirInfo.EnumerateDirectories().Sum(subDir => CalculateDirectorySize(subDir.FullName));
                return size;
            }
            catch (UnauthorizedAccessException ex)
            {
                PluginHelper.LogHelper.Log($"Access denied to directory: {directoryPath}. Details: {ex.Message}", "FileSystemService", LogLevel.Warning);
                return -2; // Indicate partial calculation or access issue
            }
            catch (Exception ex)
            {
                PluginHelper.LogHelper.Log($"Error calculating size for directory: {directoryPath}. Details: {ex.Message}", "FileSystemService", LogLevel.Error);
                return -1;
            }
        }

        public long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                PluginHelper.LogHelper.Log($"File not found: {filePath}", "FileSystemService", LogLevel.Error);
                return -1;
            }

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                PluginHelper.LogHelper.Log($"Error getting size for file: {filePath}. Details: {ex.Message}", "FileSystemService", LogLevel.Error);
                return -1;
            }
        }

        public bool PathExists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public bool IsDirectory(string path)
        {
            if (!PathExists(path)) return false; // Or throw
            FileAttributes attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }
        
        // Stub for Phase 2
        public void OpenInExplorer(string path)
        {
            if (PathExists(path))
            {
                Process.Start("explorer.exe", path);
            }
            else
            {
                 PluginHelper.LogHelper.Log($"Cannot open path in explorer, it does not exist: {path}", "FileSystemService", LogLevel.Warning);
            }
        }

        // Stub for Phase 2
        public bool DeleteFileOrDirectory(string path, bool sendToRecycleBin = true)
        {
            // Implementation for Phase 2 - Will need careful handling
            // For now, just log
            PluginHelper.LogHelper.Log($"Attempting to delete (not really): {path}", "FileSystemService", LogLevel.Info);
            return true; // Placeholder
        }
    }
}