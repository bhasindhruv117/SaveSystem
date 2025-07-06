using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystem.Core
{
    /// <summary>
    /// Utility methods for file operations related to the save system
    /// </summary>
    public static class SaveFileUtils
    {
        /// <summary>
        /// Write data to a file asynchronously
        /// </summary>
        /// <param name="filePath">Path to write the file</param>
        /// <param name="data">Data to write</param>
        /// <returns>Task representing the operation</returns>
        public static async Task WriteFileAsync(string filePath, string data)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    await writer.WriteAsync(data);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error writing to file {filePath}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Read data from a file asynchronously
        /// </summary>
        /// <param name="filePath">Path to read from</param>
        /// <returns>File contents as string</returns>
        public static async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"File not found: {filePath}");
                    return null;
                }
                
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading from file {filePath}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Check if a save file exists
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if file exists</returns>
        public static bool SaveFileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        
        /// <summary>
        /// Delete a save file
        /// </summary>
        /// <param name="filePath">Path to delete</param>
        /// <returns>True if file was deleted</returns>
        public static bool DeleteSaveFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deleting file {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}
