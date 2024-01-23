using System.Text.RegularExpressions;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Crud.Helpers;

public static class EntityNameHelper
{
    public static string GetEntityName(string name)
    {
        return Regex.Replace(name, @"Entity$", "")
           .PascalToSpacedWords(true);
    }
}