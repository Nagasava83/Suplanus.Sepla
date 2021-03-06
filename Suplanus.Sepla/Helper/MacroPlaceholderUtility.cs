﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Suplanus.Sepla.Objects;

namespace Suplanus.Sepla.Helper
{
   /// <summary>
   /// Helper for placeholders
   /// </summary>
   public class PlaceholderUtility
   {
      public const string REAL_RECORDPLACEHOLDER_START_TEXT = @"<§"; // equals <§
      public const string REAL_RECORDPLACEHOLDER_END_TEXT = @"§>"; // equals §>
      public const string REAL_TEXTPLACEHOLDER_START_TEXT = @"<#"; // equals <#
      public const string REAL_TEXTPLACEHOLDER_END_TEXT = @"#>"; // equals #>
      public const string REAL_BRICKPLACEHOLDER_START_TEXT = @"<@"; // equals <@
      public const string REAL_BRICKPLACEHOLDER_END_TEXT = @"@>"; // equals @>

      /// <summary>
      /// Start text TextPlaceholder
      /// </summary>
      public const string TEXTPLACEHOLDER_START_TEXT_EPLAN = "&lt;#"; // equals <#

      /// <summary>
      /// End text for TextPlaceholder
      /// </summary>
      public const string TEXTPLACEHOLDER_END_TEXT_EPLAN = "#&gt;"; // equals #>

      /// <summary>
      /// Start text for BrickPlaceholder
      /// </summary>
      public const string BRICKPLACEHOLDER_START_TEXT_EPLAN = "&lt;@@"; // equals <@ double @ needed in eplan

      /// <summary>
      /// End text for BrickPlaceholder
      /// </summary>
      public const string BRICKPLACEHOLDER_END_TEXT_EPLAN = "@@&gt;"; // equals @> double @ needed in eplan

      /// <summary>
      /// Start text for RecordPlaceholder
      /// </summary>
      public const string RECORDPLACEHOLDER_START_TEXT = @"&lt;§"; // equals <§

      /// <summary>
      /// End text for RecordPlaceholder
      /// </summary>
      public const string RECORDPLACEHOLDER_END_TEXT = @"§&gt;"; // equals §>

      /// <summary>
      /// Get the Placeholder to display without start and end text
      /// </summary>
      /// <param name="placeholderPlainText">Clean value of placeholder</param>
      /// <returns></returns>
      public static string GetPlaceholderName(string placeholderPlainText)
      {
         string returnValue = placeholderPlainText
            .Replace(TEXTPLACEHOLDER_START_TEXT_EPLAN, "")
            .Replace(BRICKPLACEHOLDER_START_TEXT_EPLAN, "")
            .Replace(RECORDPLACEHOLDER_START_TEXT, "")
            
            .Replace(TEXTPLACEHOLDER_END_TEXT_EPLAN, "")
            .Replace(BRICKPLACEHOLDER_END_TEXT_EPLAN, "")
            .Replace(RECORDPLACEHOLDER_END_TEXT, "")

            .Replace(REAL_BRICKPLACEHOLDER_START_TEXT, "")
            .Replace(REAL_BRICKPLACEHOLDER_END_TEXT, "")
            .Replace(REAL_TEXTPLACEHOLDER_START_TEXT, "")
            .Replace(REAL_TEXTPLACEHOLDER_END_TEXT, "")
            .Replace(REAL_RECORDPLACEHOLDER_START_TEXT, "")
            .Replace(REAL_RECORDPLACEHOLDER_END_TEXT, "")
            ;
         return returnValue;
      }

      /// <summary>
      /// Returns alls Placeholder of a macro file
      /// </summary>
      /// <typeparam name="T">Type of Placeholder</typeparam>
      /// <param name="filename">Filename which be parsed</param>
      /// <param name="startText">Placeholder start text</param>
      /// <param name="endText">Placeholder end text</param>
      /// <returns>List of Placeholder</returns>
      public static IEnumerable<T> GetMacroPlaceholder<T>(string filename, string startText, string endText)
         where T : IMacroPlaceholder, new()
      {
         if (!File.Exists(filename))
         {
            return null;
         }

         // Getplaceholders
         var text = File.ReadAllText(filename, Encoding.UTF8);
         IEnumerable<string> matches = Regex.Matches(text, startText + "(.*?)" + endText)
            .OfType<Match>()
            .Select(m => m.Groups[0].Value)
            .Distinct();

         // Return placeholder objects
         List<T> placeholders = new List<T>();
         foreach (var match in matches)
         {
            var placeholderText = new T();
            var name = match.Replace(startText, "").Replace(endText, "");
            placeholderText.Name = name;
            placeholders.Add(placeholderText);
         }
         return placeholders;
      }

      /// <summary>
      /// Replaces all placeholder in file and returns a temp macro file
      /// </summary>
      /// <param name="macroFilename">Source macro file</param>
      /// <param name="placeholders">Placeholder to replace</param>
      /// <param name="endText">End text</param>
      /// <param name="removeText">Remove text if value is not active or empty</param>
      /// <param name="startText">Start text</param>
      /// <returns></returns>
      public static string ReplacePlaceholderTextAndGetTempFile(
         string macroFilename, List<IMacroPlaceholder> placeholders,
         string startText, string endText, bool removeText)
      {
         if (!File.Exists(macroFilename))
         {
            return null;
         }

         // needed because EPLAN is checking extension
         string extension = Path.GetExtension(macroFilename);         
         string tempFile = Path.Combine(Path.GetTempPath(), "Suplanus.Sepla.MacroPlaceholderUtility" + extension);
         string content = File.ReadAllText(macroFilename);

         foreach (var placeholder in placeholders)
         {
            // Skip if not active or empty
            if (!placeholder.IsActive || placeholder.Value == null || string.IsNullOrEmpty(placeholder.Value.ToString()))
            {
               continue;
            }

            // Replace
            var replaceText = placeholder.Value.ToString();
            var searchText = startText + placeholder.Name + endText;
            content = content.Replace(searchText, replaceText);
         }

         File.WriteAllText(tempFile, content, Encoding.UTF8);

         return tempFile;
      }
   }
}
