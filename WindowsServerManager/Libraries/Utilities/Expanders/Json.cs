﻿using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WindowsServerManager.Libraries.Utilities.Expanders
{
    public static class Json
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        // Dotnet devs can suck my fucking balls
        // https://github.com/dotnet/runtime/issues/1174
        public static string Serialize<TValue>(TValue obj, bool @unsafe = true)
        {
            string jsonBody = JsonSerializer.Serialize(obj, SerializerOptions);

            StringBuilder newBody = new(jsonBody.Length);

            bool insideValue = false;

            for (int i = 0; i < jsonBody.Length; i++)
            {
                if (jsonBody[i] == '"') insideValue = !insideValue;

                if (i + 1 < jsonBody.Length && jsonBody[i] == ' ' && jsonBody[i + 1] == ' ' && !insideValue)
                {
                    int indentCount = 0;

                    while (i + indentCount < jsonBody.Length && jsonBody[i + indentCount] == ' ') indentCount++;

                    int tabCount = indentCount / 2;
                    newBody.Append(new string('\t', tabCount));
                    i += indentCount - 1;
                }
                else newBody.Append(jsonBody[i]);
            }

            return newBody.ToString();

        }
    }
}
