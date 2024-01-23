using System.Text.RegularExpressions;
using Tools.Extensions;

namespace Crud.Helpers;

public static class EntityNameHelper
{
    public static string GetEntityName(string name)
    {
        return Regex.Replace(name, @"Entity$", "")
           .PascalToSpacedWords(true);
    }
}